# MCP Configuration Examples

These files show the shape of an `mcpServers` entry for UIAutomationMcp.

## Important
Replace the placeholder executable path with the actual location of `UIAutomationMcp.McpServer.exe` on your machine.

To avoid hardcoding a path entirely, use `npx-config.json` instead: the
[`uia-mcp`](https://www.npmjs.com/package/uia-mcp) npm package bundles the server and the
.NET runtime, so `npx -y uia-mcp` works with no prior install. The shape is the same for
every client listed below.

## Suggested validation prompt
After adding the config, ask your client:

```text
List the focused desktop UI element.
```

## Files
- `npx-config.json` (no path needed)
- `claude-desktop-config.json`
- `cursor-mcp-config.json`
- `cline-mcp-config.json`
- `windsurf-mcp-config.json`
- `vscode-mcp-config.json`

