# Development Workflow

## Active solution
The active solution is `UIAutomationMcp.sln`.

## Projects
- `src\UIAutomationMcp.ComInterop`
- `src\UIAutomationMcp.Service`
- `src\UIAutomationMcp.CLI`
- `src\UIAutomationMcp.McpServer`
- `src\UIAutomationMcp.Smoke`
- `vscode-extension`

## Typical validation flow

```powershell
dotnet build UIAutomationMcp.sln
dotnet run --project src\UIAutomationMcp.Smoke\UIAutomationMcp.Smoke.csproj
dotnet run --project src\UIAutomationMcp.CLI\UIAutomationMcp.CLI.csproj -- desktop
```

For MCP server changes, verify the server starts cleanly and that stdout remains protocol-safe.

For VS Code extension changes:

```powershell
Set-Location vscode-extension
npm install
npm run compile
npm run package
```

## Design expectations
- Windows UI Automation only
- typed interop through `Interop.UIAutomationClient`
- STA-safe execution for COM/UIA access
- aligned public behavior across CLI, MCP server, and extension packaging

