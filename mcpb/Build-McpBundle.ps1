<#
.SYNOPSIS
    Creates the MCPB (MCP Bundle) package for Claude Desktop.

.DESCRIPTION
    Builds the UIAutomationMcp MCP server as a self-contained Windows x64 executable
    and packages it as an .mcpb file for one-click installation in Claude Desktop.

.PARAMETER Version
    Version number for the package (e.g. "1.0.0"). Defaults to the value in Directory.Build.props.

.PARAMETER OutputDir
    Output directory for the MCPB package, relative to the mcpb folder. Defaults to ./artifacts.

.EXAMPLE
    .\Build-McpBundle.ps1

.EXAMPLE
    .\Build-McpBundle.ps1 -Version "1.2.0"

.NOTES
    Requires the .NET 9 SDK on Windows x64.

    Output: mcpb/artifacts/uia-mcp-{version}.mcpb
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Version,

    [Parameter()]
    [string]$OutputDir = "./artifacts"
)

$ErrorActionPreference = "Stop"

$McpbDir = $PSScriptRoot
$RootDir = Split-Path $McpbDir -Parent
$McpServerDir = Join-Path $RootDir "src/UIAutomationMcp.McpServer"

Write-Host "Building MCPB (MCP Bundle) package..." -ForegroundColor Cyan

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

$OutputDir = Join-Path $McpbDir $OutputDir
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$StagingDir = Join-Path $OutputDir "staging"
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
Write-Host "  Built UIAutomationMcp.McpServer.exe" -ForegroundColor Green

$ServerDir = Join-Path $StagingDir "server"
New-Item -ItemType Directory -Path $ServerDir -Force | Out-Null
$FinalExePath = Join-Path $ServerDir "uia-mcp-server.exe"
Move-Item (Join-Path $StagingDir "UIAutomationMcp.McpServer.exe") $FinalExePath -Force
Write-Host "  Renamed to server/uia-mcp-server.exe" -ForegroundColor Green

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

$ManifestDst = Join-Path $StagingDir "manifest.json"
$ManifestContent = Get-Content (Join-Path $McpbDir "manifest.json") -Raw
$ManifestContent = $ManifestContent -replace '"version":\s*"[\d\.]+"', "`"version`": `"$Version`""
Set-Content $ManifestDst $ManifestContent -NoNewline
Write-Host "  Copied manifest.json (version: $Version)" -ForegroundColor Green

Copy-Item (Join-Path $McpbDir "icon-512.png") (Join-Path $StagingDir "icon-512.png") -Force
Copy-Item (Join-Path $McpbDir "README.md") (Join-Path $StagingDir "README.md") -Force
Copy-Item (Join-Path $RootDir "LICENSE") (Join-Path $StagingDir "LICENSE") -Force
Copy-Item (Join-Path $RootDir "CHANGELOG.md") (Join-Path $StagingDir "CHANGELOG.md") -Force
Write-Host "  Copied icon, README, LICENSE and CHANGELOG" -ForegroundColor Green

# The .mcp registry metadata is not used by Claude Desktop.
$McpMetaDir = Join-Path $StagingDir ".mcp"
if (Test-Path $McpMetaDir) {
    Remove-Item -Recurse -Force $McpMetaDir
}

$McpbFileName = "uia-mcp-$Version.mcpb"
$McpbPath = Join-Path $OutputDir $McpbFileName

Write-Host "Creating MCPB bundle..." -ForegroundColor Yellow

$FilesToZip = @(
    (Join-Path $StagingDir "manifest.json"),
    (Join-Path $StagingDir "icon-512.png"),
    (Join-Path $StagingDir "README.md"),
    (Join-Path $StagingDir "LICENSE"),
    (Join-Path $StagingDir "CHANGELOG.md"),
    (Join-Path $StagingDir "server")
)

Compress-Archive -Path $FilesToZip -DestinationPath $McpbPath -Force
Copy-Item $ManifestDst (Join-Path $OutputDir "manifest.json") -Force
Remove-Item -Recurse -Force $StagingDir

$McpbSize = (Get-Item $McpbPath).Length / 1MB
Write-Host ""
Write-Host "MCPB bundle created: $McpbPath ($([math]::Round($McpbSize, 1)) MB)" -ForegroundColor Green

$McpbContents = [System.IO.Compression.ZipFile]::OpenRead($McpbPath)
try {
    foreach ($entry in $McpbContents.Entries) {
        $sizeKB = [math]::Round($entry.Length / 1KB, 1)
        Write-Host "   - $($entry.FullName) ($sizeKB KB)" -ForegroundColor White
    }
} finally {
    $McpbContents.Dispose()
}

Write-Host ""
Write-Host "Install by double-clicking the .mcpb file in Claude Desktop." -ForegroundColor Cyan
