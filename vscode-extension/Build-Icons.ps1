<#
.SYNOPSIS
    Regenerates the raster icons from vscode-extension/icon.svg.

.DESCRIPTION
    icon.svg is the single source of truth for the project mark. Two rasters are
    derived from it and both are committed, because their consumers cannot read
    SVG:

      vscode-extension/icon.png  256x256  referenced by package.json, shown in
                                          the Marketplace and the extension list
      mcpb/icon-512.png          512x512  referenced by mcpb/manifest.json, shown
                                          by Claude Desktop

    Rendering needs a real SVG rasteriser. .NET has none in the box and this repo
    has no Node toolchain of its own, so the script provisions sharp into a temp
    directory on demand. That keeps a Node dependency out of the repository for
    what is a rare, manual operation - the icons only change when the mark does.

    Run this after editing icon.svg and commit the regenerated PNGs:

        pwsh vscode-extension/Build-Icons.ps1

.PARAMETER SkipInstall
    Reuse an existing sharp installation in the temp working directory instead of
    running npm install. Useful when iterating on the mark offline.
#>
[CmdletBinding()]
param(
    [switch]$SkipInstall
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

$svgPath = Join-Path $scriptDir 'icon.svg'
if (-not (Test-Path $svgPath)) {
    throw "Source mark not found: $svgPath"
}

# size -> output path
$targets = [ordered]@{
    256 = Join-Path $scriptDir 'icon.png'
    512 = Join-Path $repoRoot 'mcpb\icon-512.png'
}

Write-Host "Building icons from $svgPath" -ForegroundColor Cyan

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    throw 'Node.js is required to rasterise the SVG. Install Node 18 or newer and re-run.'
}

$workDir = Join-Path $env:TEMP 'uia-icon-build'
if (-not (Test-Path $workDir)) {
    New-Item -ItemType Directory -Path $workDir -Force | Out-Null
}

if (-not $SkipInstall) {
    Write-Host '  Provisioning sharp...' -ForegroundColor Gray
    Push-Location $workDir
    try {
        if (-not (Test-Path (Join-Path $workDir 'package.json'))) {
            npm init -y 2>&1 | Out-Null
        }
        npm install sharp 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "npm install sharp failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

$renderScript = Join-Path $workDir 'render-icons.js'
@'
const fs = require("fs");
const path = require("path");
const sharp = require(path.join(process.argv[2], "node_modules", "sharp"));

const svg = fs.readFileSync(process.argv[3]);
const targets = JSON.parse(fs.readFileSync(process.argv[4], "utf8"));

(async () => {
  for (const [size, out] of targets) {
    // A high density keeps the 512 render crisp; sharp rasterises the SVG at
    // density before resizing, so rendering small at low density would alias
    // the thin window stroke.
    await sharp(svg, { density: 900 })
      .resize(Number(size), Number(size), { fit: "contain", background: { r: 0, g: 0, b: 0, alpha: 0 } })
      .png({ compressionLevel: 9 })
      .toFile(out);
    console.log(`${size}\t${out}`);
  }
})().catch((err) => {
  console.error(err.message);
  process.exit(1);
});
'@ | Set-Content -Encoding UTF8 $renderScript

$targetJson = Join-Path $workDir 'icon-targets.json'
$pairs = New-Object System.Collections.ArrayList
foreach ($entry in $targets.GetEnumerator()) {
    # Comma operator keeps each pair a nested array; without it the pipeline
    # flattens them into a single flat list of sizes and paths.
    [void]$pairs.Add(@("$($entry.Key)", $entry.Value))
}
# Written to a file rather than passed as an argument: the paths contain
# backslashes and quotes that do not survive shell argument quoting reliably.
ConvertTo-Json -InputObject $pairs.ToArray() -Depth 3 | Set-Content -Encoding UTF8 $targetJson

node $renderScript $workDir $svgPath $targetJson
if ($LASTEXITCODE -ne 0) {
    throw "Icon rendering failed with exit code $LASTEXITCODE"
}

# Verify rather than trust the exit code: a truncated or zero-byte PNG would
# still leave node happy, and a broken icon is only noticed once it is live on
# the Marketplace.
foreach ($entry in $targets.GetEnumerator()) {
    $size = $entry.Key
    $file = $entry.Value

    if (-not (Test-Path $file)) {
        throw "Expected icon was not produced: $file"
    }

    $bytes = [System.IO.File]::ReadAllBytes($file)
    if ($bytes.Length -lt 1024) {
        throw "Icon looks truncated ($($bytes.Length) bytes): $file"
    }

    # PNG signature, then IHDR width/height as big-endian uint32 at offsets 16 and 20.
    $signature = @(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
    for ($i = 0; $i -lt $signature.Length; $i++) {
        if ($bytes[$i] -ne $signature[$i]) {
            throw "Not a PNG file: $file"
        }
    }

    $width = [int]$bytes[16] * 16777216 + [int]$bytes[17] * 65536 + [int]$bytes[18] * 256 + [int]$bytes[19]
    $height = [int]$bytes[20] * 16777216 + [int]$bytes[21] * 65536 + [int]$bytes[22] * 256 + [int]$bytes[23]

    if ($width -ne $size -or $height -ne $size) {
        throw "Icon has the wrong dimensions: $file is ${width}x${height}, expected ${size}x${size}"
    }

    $kb = [math]::Round($bytes.Length / 1KB, 1)
    Write-Host "  OK  $($file.Replace($repoRoot + '\', '')) - ${width}x${height}, ${kb} KB" -ForegroundColor Green
}

Write-Host 'Icons rebuilt. Commit icon.svg together with the regenerated PNGs.' -ForegroundColor Cyan
