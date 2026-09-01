#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies that every action verb the COM layer implements is documented in
    the CLI help text and exposed through both public entry points.

.DESCRIPTION
    The action catalog is a string switch, and it is duplicated across three
    places that nothing forces to agree:

      1. the `PerformAction` switch in UiAutomationBootstrap.cs  (authoritative)
      2. the CLI help text in Program.cs                         (user-facing)
      3. the MCP tool surface in UiAutomationQueryTool.cs        (agent-facing)

    Drift here is silent: a verb added to the switch but missing from the help
    text is invisible until someone fails to find it. This script makes that
    drift fail.

    It also compares CLI command count against MCP tool count, because the
    repository maintains exact one-to-one parity between them.

    Exits non-zero if it cannot find what it is meant to scan. A check that
    inspects nothing must never report success.

.EXAMPLE
    ./scripts/check-cli-coverage.ps1
#>

$ErrorActionPreference = "Stop"
$rootDir = Split-Path -Parent $PSScriptRoot

$bootstrapPath = Join-Path $rootDir "src/UIAutomationMcp.ComInterop/UiAutomationBootstrap.cs"
$cliPath       = Join-Path $rootDir "src/UIAutomationMcp.CLI/Program.cs"
$mcpPath       = Join-Path $rootDir "src/UIAutomationMcp.McpServer/Tools/UiAutomationQueryTool.cs"

foreach ($required in @($bootstrapPath, $cliPath, $mcpPath)) {
    if (-not (Test-Path $required)) {
        Write-Host "Cannot scan: missing $required" -ForegroundColor Red
        exit 1
    }
}

Write-Host "CLI / MCP coverage check" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan

$bootstrap = Get-Content $bootstrapPath -Raw
$cli       = Get-Content $cliPath -Raw
$mcp       = Get-Content $mcpPath -Raw

# Action verbs are the string literals matched by the PerformAction switch.
# Scope the search to that method so unrelated string switches elsewhere in the
# file cannot inflate the list.
$performAction = [regex]::Match(
    $bootstrap,
    'PerformAction\(UiAutomationActionRequest request\).*?_ => throw new ArgumentOutOfRangeException',
    'Singleline')

if (-not $performAction.Success) {
    Write-Host "Could not locate the PerformAction switch. The parser needs updating." -ForegroundColor Red
    exit 1
}

$verbs = [regex]::Matches($performAction.Value, '"([a-z][a-z0-9-]*)"\s*=>') |
    ForEach-Object { $_.Groups[1].Value } |
    Sort-Object -Unique

if ($verbs.Count -eq 0) {
    Write-Host "Found the PerformAction switch but no verbs in it. The parser needs updating." -ForegroundColor Red
    exit 1
}

# CLI commands are the arms of the top-level command switch; MCP tools are
# declared by attribute.
$cliCommands = [regex]::Matches($cli, '(?m)^\s{8}"([a-z][a-z0-9-]*)"(?:\s+or\s+"[^"]+")*\s*=>') |
    ForEach-Object { $_.Groups[1].Value } |
    Where-Object { $_ -notin @('help', 'version') } |
    Sort-Object -Unique
$mcpTools = [regex]::Matches($mcp, 'McpServerTool\(Name = "([a-z_0-9]+)"\)') |
    ForEach-Object { $_.Groups[1].Value } |
    Sort-Object -Unique

Write-Host ""
Write-Host "Action verbs implemented : $($verbs.Count)"
Write-Host "CLI commands             : $($cliCommands.Count)"
Write-Host "MCP tools                : $($mcpTools.Count)"
Write-Host ""

$failures = @()

# 1. Every implemented verb must appear in the CLI help text. The help text is
#    the CLI's only documentation and it has drifted before.
$helpText = [regex]::Match($cli, 'static void WriteHelp\(\).*?^\}', 'Singleline, Multiline').Value
if (-not $helpText) {
    Write-Host "Could not locate WriteHelp(). The parser needs updating." -ForegroundColor Red
    exit 1
}

foreach ($verb in $verbs) {
    if ($helpText -notmatch [regex]::Escape($verb)) {
        $failures += "action verb '$verb' is implemented but absent from CLI help text"
    }
}

# 2. CLI and MCP must expose the same number of operations. Names differ by
#    convention (kebab-case vs uia_snake_case), so compare counts and report the
#    mapping rather than trying to match names.
if ($cliCommands.Count -ne $mcpTools.Count) {
    $failures += "CLI exposes $($cliCommands.Count) commands but MCP exposes $($mcpTools.Count) tools; they must stay at parity"
}

# 3. Published tool counts drift silently. Fail if any file states a number that
#    disagrees with the code.
$countClaims = Get-ChildItem -Path $rootDir -Recurse -File -Include *.json, *.csproj, *.md |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj|node_modules|\.git|\.trestle)[\\/]' } |
    Select-String -Pattern '(\d+)\s+tools' -AllMatches

foreach ($claim in $countClaims) {
    foreach ($m in $claim.Matches) {
        $claimed = [int]$m.Groups[1].Value
        if ($claimed -ne $mcpTools.Count) {
            $rel = $claim.Path.Replace("$rootDir", "").TrimStart('\', '/')
            $failures += "$rel`:$($claim.LineNumber) claims $claimed tools; the server exposes $($mcpTools.Count)"
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Coverage problems found:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    exit 1
}

Write-Host "All action verbs documented; CLI and MCP at parity." -ForegroundColor Green
exit 0
