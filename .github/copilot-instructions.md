# GitHub Copilot Instructions - UIAutomationMcp

## Project focus

UIAutomationMcp is a Windows-only automation toolkit for inspecting live desktop UI with Windows UI Automation.

The active product surfaces are:
- `src\UIAutomationMcp.CLI` (`uiamcp`)
- `src\UIAutomationMcp.McpServer`
- `vscode-extension`

## Read first

When changing code, start with:
- `docs\UIAUTOMATION-COM-REFERENCE.md`
- `README.md`
- `FEATURES.md`

## Core rules

- Do not reintroduce legacy pre-migration naming.
- Keep CLI and MCP behavior aligned when you add or change UIA operations.
- UI Automation access must remain safe for Windows COM/STA execution.
- Prefer focused docs and examples that match the current UIA-only repo.
- Validate changes with the smallest relevant build or smoke command before finishing.

## Quick validation

```powershell
dotnet build UIAutomationMcp.sln

dotnet run --project src\UIAutomationMcp.Smoke\UIAutomationMcp.Smoke.csproj

dotnet run --project src\UIAutomationMcp.CLI\UIAutomationMcp.CLI.csproj -- desktop
```

## Current architecture

- `UIAutomationMcp.ComInterop` - typed bootstrap and UI Automation COM access
- `UIAutomationMcp.Service` - shared query surface used by all entry points
- `UIAutomationMcp.CLI` - JSON-friendly terminal entry point
- `UIAutomationMcp.McpServer` - MCP tool host for assistants
- `vscode-extension` - packaged VS Code surface bundling the CLI and MCP server

