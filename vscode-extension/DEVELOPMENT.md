# VS Code Extension Development Notes

## Project structure
- `src\extension.ts` - extension entry point
- `package.json` - extension manifest
- `README.md` - user-facing extension docs
- `bin\` - bundled self-contained executables
- `skills\uia-mcp\SKILL.md`
- `skills\uia-cli\SKILL.md`

## Core expectations
- provider id stays `uia-mcp`
- bundled binaries stay `UIAutomationMcp.McpServer.exe` and `uiamcp.exe`
- extension docs and commands stay aligned with the UIAutomationMcp repo

## Common commands
```powershell
npm install
npm run compile
npm run package
```

## Bundle refresh
The package step rebuilds and bundles the current CLI and MCP server into `bin\`.

## Icons
`icon.svg` is the source of truth for the project mark. Two rasters are derived from it
and both are committed, because their consumers cannot read SVG: `icon.png` (256x256,
referenced by `package.json` and rendered by the Marketplace) and `..\mcpb\icon-512.png`
(512x512, referenced by `mcpb\manifest.json` and rendered by Claude Desktop).

After editing the SVG, regenerate both and commit them together with it:

```powershell
pwsh vscode-extension\Build-Icons.ps1
```

The script provisions `sharp` into a temp directory on demand, so no Node dependency is
kept in the repository for what is a rare manual operation. It verifies the PNG signature
and dimensions of each output rather than trusting the renderer's exit code.

The mark carries no text on purpose. The Marketplace and the extension list render it at
roughly 42-90 px, where a wordmark is unreadable, and the extension name is already shown
next to the icon.

## Manual checks
- extension activates cleanly in an Extension Development Host
- MCP registration points to `UIAutomationMcp.McpServer.exe`
- packaged VSIX contains only the expected binaries, manifest files, and skill files

