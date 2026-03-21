# Contributing to UIAutomationMcp

UIAutomationMcp is a Windows-only UI Automation toolkit. Contributions should keep the repository aligned with that scope.

## Development setup

```powershell
git clone https://github.com/trsdn/mcp-server-uiautomation.git
cd mcp-server-uiautomation
dotnet restore
dotnet build UIAutomationMcp.sln
```

## Attribution

Please keep existing attribution to Stefan Brönner intact where it already exists, and keep the brief sibling-project reference to Excel MCP in repository-facing docs.

## Ground rules
- Use pull requests for all changes.
- Keep CLI and MCP behavior consistent when you add UIA capabilities.
- Keep the repository UIAutomationMcp-only; do not reintroduce legacy names, docs, or examples.
- Prefer small, verifiable changes with updated docs when behavior changes.

## Recommended checks

```powershell
dotnet build UIAutomationMcp.sln
dotnet run --project src\UIAutomationMcp.Smoke\UIAutomationMcp.Smoke.csproj
dotnet run --project src\UIAutomationMcp.CLI\UIAutomationMcp.CLI.csproj -- focused
```

If you touch the VS Code extension:

```powershell
Set-Location vscode-extension
npm install
npm run compile
```

