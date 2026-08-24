<#
.SYNOPSIS
    Builds the npm package for the UI Automation MCP server.

.DESCRIPTION
    Publishes the MCP server as a self-contained Windows x64 executable into
    npm/server/, stamps the version into package.json, and optionally produces
    the tarball with npm pack.

    The published package contains only the server, not the CLI: `npx uia-mcp`
    exists to start an MCP server, and shipping the CLI would double the
    download for something npm users get from the NuGet tool instead.

.PARAMETER Version
    Version number for the package (e.g. "1.0.0"). Defaults to the value in Directory.Build.props.

.PARAMETER Pack
    Also run `npm pack` and leave the tarball in npm/artifacts.

.EXAMPLE
    .\Build-NpmPackage.ps1

.EXAMPLE
    .\Build-NpmPackage.ps1 -Version "1.2.0" -Pack

.NOTES
    Requires the .NET 9 SDK on Windows x64, and Node.js for -Pack.

    Output: npm/server/uia-mcp-server.exe (+ npm/artifacts/uia-mcp-{version}.tgz with -Pack)
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Version,

    [Parameter()]
    [switch]$Pack
)

$ErrorActionPreference = "Stop"

$NpmDir = $PSScriptRoot
$RootDir = Split-Path $NpmDir -Parent
$McpServerDir = Join-Path $RootDir "src/UIAutomationMcp.McpServer"

Write-Host "Building npm package..." -ForegroundColor Cyan

if (-not $Version) {
    $PropsFile = Join-Path $RootDir "Directory.Build.props"
    if (Test-Path $PropsFile) {
        $xml = [xml](Get-Content $PropsFile)
        $Version = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    }
    if (-not $Version) {
        $Version = "1.0.0"
    }
}
Write-Host "Version: $Version" -ForegroundColor Green

$ServerDir = Join-Path $NpmDir "server"
if (Test-Path $ServerDir) {
    Remove-Item -Recurse -Force $ServerDir
}
New-Item -ItemType Directory -Path $ServerDir -Force | Out-Null

$StagingDir = Join-Path $NpmDir "staging"
if (Test-Path $StagingDir) {
    Remove-Item -Recurse -Force $StagingDir
}
New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null

Write-Host "Publishing self-contained executable..." -ForegroundColor Yellow

$PublishArgs = @(
    "publish"
    "$McpServerDir/UIAutomationMcp.McpServer.csproj"
    "-c", "Release"
    "-r", "win-x64"
    "--self-contained", "true"
    "-p:PublishSingleFile=true"
    "-p:IncludeNativeLibrariesForSelfExtract=true"
    "-p:PublishTrimmed=false"
    "-p:PublishReadyToRun=false"
    "-p:NuGetAudit=false"
    "-p:Version=$Version"
    "-p:AssemblyVersion=$Version.0"
    "-p:FileVersion=$Version.0"
    "-o", $StagingDir
    "--verbosity", "quiet"
)

& dotnet @PublishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed." -ForegroundColor Red
    exit 1
}

$FinalExePath = Join-Path $ServerDir "uia-mcp-server.exe"
Move-Item (Join-Path $StagingDir "UIAutomationMcp.McpServer.exe") $FinalExePath -Force
Remove-Item -Recurse -Force $StagingDir
Write-Host "  Built server/uia-mcp-server.exe" -ForegroundColor Green

$VersionOutput = & $FinalExePath --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Executable verification failed." -ForegroundColor Red
    exit 1
}
if ("$VersionOutput".Trim() -ne $Version) {
    Write-Host "Executable reported version '$VersionOutput' but expected '$Version'." -ForegroundColor Red
    exit 1
}
Write-Host "  Verified: $VersionOutput" -ForegroundColor Green

$PackageJsonPath = Join-Path $NpmDir "package.json"
$PackageJson = Get-Content $PackageJsonPath -Raw
$PackageJson = $PackageJson -replace '"version":\s*"[\d\.]+"', "`"version`": `"$Version`""
Set-Content $PackageJsonPath $PackageJson -NoNewline
Write-Host "  Stamped package.json (version: $Version)" -ForegroundColor Green

Copy-Item (Join-Path $RootDir "LICENSE") (Join-Path $NpmDir "LICENSE") -Force
Write-Host "  Copied LICENSE" -ForegroundColor Green

if ($Pack) {
    $ArtifactsDir = Join-Path $NpmDir "artifacts"
    if (Test-Path $ArtifactsDir) {
        Remove-Item -Recurse -Force $ArtifactsDir
    }
    New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null

    Write-Host "Packing tarball..." -ForegroundColor Yellow
    Push-Location $NpmDir
    try {
        & npm pack --pack-destination $ArtifactsDir
        if ($LASTEXITCODE -ne 0) {
            Write-Host "npm pack failed." -ForegroundColor Red
            exit 1
        }
    } finally {
        Pop-Location
    }

    $Tarball = Get-ChildItem $ArtifactsDir -Filter "*.tgz" | Select-Object -First 1
    if (-not $Tarball) {
        Write-Host "No tarball produced." -ForegroundColor Red
        exit 1
    }

    # A tarball missing a file installs cleanly and then fails at run time, so
    # check the contents rather than just the exit code. bin/uia-mcp.js in
    # particular is easy to lose: the root .gitignore excludes every bin/.
    $Entries = & tar -tzf $Tarball.FullName
    $Required = @(
        "package/server/uia-mcp-server.exe",
        "package/bin/uia-mcp.js",
        "package/package.json"
    )
    $Missing = $Required | Where-Object { $Entries -notcontains $_ }
    if ($Missing) {
        Write-Host "Tarball is missing required entries:" -ForegroundColor Red
        $Missing | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
        Write-Host "Tarball contains:" -ForegroundColor Yellow
        $Entries | ForEach-Object { Write-Host "   - $_" }
        exit 1
    }

    $SizeMB = [math]::Round($Tarball.Length / 1MB, 1)
    Write-Host ""
    Write-Host "npm package created: $($Tarball.FullName) ($SizeMB MB)" -ForegroundColor Green
    $Entries | ForEach-Object { Write-Host "   - $_" -ForegroundColor White }
}

Write-Host ""
Write-Host "Run it with: npx uia-mcp" -ForegroundColor Cyan
