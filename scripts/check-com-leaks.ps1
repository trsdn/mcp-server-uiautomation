#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detects UI Automation COM proxies that are acquired but never released.

.DESCRIPTION
    Every UIA proxy this project holds must be released in a `finally` block.
    The cost of missing one is invisible in normal use: no warning, no test
    failure, no symptom until a long-running MCP session has leaked enough
    cross-process references to matter.

    Two acquisition sites are checked:

      1. locals typed as an IUIAutomation* interface
      2. `GetPattern<T>(...)` results

    For each, the enclosing file must release it through `FinalRelease` or
    `ReleaseAll`. Release is matched by variable name, so a proxy that is
    acquired and then forgotten is reported even when other proxies in the same
    file are released correctly.

    This is a heuristic over text, not a dataflow analysis: it cannot prove that
    a release is reachable on every path. It catches the common and costly case
    - an acquisition with no corresponding release at all.

    Exits non-zero if it finds nothing to scan, because a check that inspects
    no files must never report success.

.EXAMPLE
    ./scripts/check-com-leaks.ps1
#>

$ErrorActionPreference = "Stop"
$rootDir = Split-Path -Parent $PSScriptRoot
$srcDir = Join-Path $rootDir "src"

Write-Host "COM proxy lifetime check" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan

$files = Get-ChildItem -Path $srcDir -Recurse -Filter *.cs |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }

if ($files.Count -eq 0) {
    Write-Host "No source files found under $srcDir. Nothing was checked." -ForegroundColor Red
    exit 1
}

$acquiringFiles = 0
$totalAcquisitions = 0
$leaks = @()

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $relative = $file.FullName.Replace("$rootDir", "").TrimStart('\', '/')

    # Locals holding a UIA interface, and pattern acquisitions. Both forms bind
    # a proxy to a name that must later be released.
    $names = @()
    $names += [regex]::Matches($content, '(?m)^\s*(?:IUIAutomation[A-Za-z0-9_]*)\??\s+(\w+)\s*=') |
        ForEach-Object { $_.Groups[1].Value }
    $names += [regex]::Matches($content, '(?m)^\s*var\s+(\w+)\s*=\s*GetPattern<') |
        ForEach-Object { $_.Groups[1].Value }

    $names = $names | Where-Object { $_ -notin @('var', 'return') } | Sort-Object -Unique
    if ($names.Count -eq 0) { continue }

    $acquiringFiles++
    $totalAcquisitions += $names.Count

    # A file with no release call at all cannot be releasing anything.
    if ($content -notmatch 'FinalRelease|ReleaseAll') {
        foreach ($name in $names) {
            $leaks += "$relative : '$name' is acquired but the file never calls FinalRelease or ReleaseAll"
        }
        continue
    }

    foreach ($name in $names) {
        $escaped = [regex]::Escape($name)
        $released = $content -match "FinalRelease\(\s*$escaped\s*[\),]" -or
                    $content -match "ReleaseAll\(\s*(?:new\s*\[\]\s*)?\{[^}]*\b$escaped\b"
        if ($released) { continue }

        # Ownership transfer: a proxy assigned into another local is released
        # through that name instead. The loop-and-advance pattern relies on this
        #     next = walker.GetNextSiblingElement(current);
        #     ... finally { FinalRelease(current); }
        #     current = next;
        # where `next` is never released under its own name and must not be.
        $transferTargets = [regex]::Matches($content, "(?m)^\s*(\w+)\s*=\s*$escaped\s*;") |
            ForEach-Object { $_.Groups[1].Value } |
            Sort-Object -Unique

        $transferred = $false
        foreach ($target in $transferTargets) {
            $t = [regex]::Escape($target)
            if ($content -match "FinalRelease\(\s*$t\s*[\),]" -or
                $content -match "ReleaseAll\(\s*(?:new\s*\[\]\s*)?\{[^}]*\b$t\b") {
                $transferred = $true
                break
            }
        }

        if (-not $transferred) {
            $leaks += "$relative : '$name' is acquired but never passed to FinalRelease or ReleaseAll"
        }
    }
}

Write-Host ""
Write-Host "Files acquiring COM proxies : $acquiringFiles"
Write-Host "Proxy bindings checked      : $totalAcquisitions"
Write-Host ""

if ($totalAcquisitions -eq 0) {
    Write-Host "No COM proxy acquisitions were found. Either the interop layer moved" -ForegroundColor Red
    Write-Host "or this parser is out of date; either way nothing was verified." -ForegroundColor Red
    exit 1
}

if ($leaks.Count -gt 0) {
    Write-Host "Possible COM proxy leaks:" -ForegroundColor Red
    $leaks | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "Release every proxy in a finally block. See docs/UIAUTOMATION-COM-REFERENCE.md." -ForegroundColor Red
    exit 1
}

Write-Host "Every acquired COM proxy is released." -ForegroundColor Green
exit 0
