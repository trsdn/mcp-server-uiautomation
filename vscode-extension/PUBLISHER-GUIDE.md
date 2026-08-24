# Publisher Guide

Use this guide when you are ready to publish the `uia-mcp` VS Code extension.

## Before publishing
- verify `npm run compile`
- verify `npm run package`
- confirm the VSIX contains `UIAutomationMcp.McpServer.exe` and `uiamcp.exe`
- review `README.md` and `CHANGELOG.md`

## Marketplace setup
The publisher `trsdn` exists and the extension is live as
[`trsdn.uia-mcp`](https://marketplace.visualstudio.com/items?itemName=trsdn.uia-mcp).

1. Store the marketplace token as `VSCE_TOKEN` in repository secrets to publish
   automatically from `.github/workflows/release.yml`.
2. Without that secret, upload the VSIX by hand at
   https://marketplace.visualstudio.com/manage/publishers/trsdn — see
   `MARKETPLACE-PUBLISHING.md` for the exact steps.

## Notes
Keep publisher metadata, extension id, and repository links aligned with `uia-mcp`.

Keep the existing attribution trail to Stefan Brönner intact, and do not remove the brief lineage reference to Excel MCP from repository-facing extension docs.

