# Publisher Guide

Use this guide when you are ready to publish the `uia-mcp` VS Code extension.

## Before publishing
- verify `npm run compile`
- verify `npm run package`
- confirm the VSIX contains `UIAutomationMcp.McpServer.exe` and `uiamcp.exe`
- review `README.md` and `CHANGELOG.md`

## Marketplace setup
1. Create or reuse a VS Code Marketplace publisher.
2. Store the marketplace token as `VSCE_TOKEN` in repository secrets.
3. Publish with your chosen workflow or `vsce publish` flow.

## Notes
Keep publisher metadata, extension id, and repository links aligned with `uia-mcp`.

Keep the existing attribution trail to Stefan Brönner intact, and do not remove the brief lineage reference to Excel MCP from repository-facing extension docs.

