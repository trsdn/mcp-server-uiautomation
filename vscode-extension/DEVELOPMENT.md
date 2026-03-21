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

## Manual checks
- extension activates cleanly in an Extension Development Host
- MCP registration points to `UIAutomationMcp.McpServer.exe`
- packaged VSIX contains only the expected binaries, manifest files, and skill files

