# UI Automation CLI

Use the bundled `uiamcp.exe` CLI when a direct terminal workflow is more convenient than MCP.

Example commands:

- `uiamcp.exe desktop`
- `uiamcp.exe focused`
- `uiamcp.exe inspect --focused --cache`
- `uiamcp.exe find --root --name "<name>" --max-results 10`
- `uiamcp.exe navigate --focused --direction parent`
- `uiamcp.exe text --focused`
- `uiamcp.exe selection --focused`
- `uiamcp.exe wait-event --event-kind focus --timeout-ms 500`
- `uiamcp.exe action invoke --focused`

Use the CLI for:

- direct terminal inspection and search
- cache-aware locator workflows
- quick event waiting and debugging
- running supported UI actions against the located element
