# Installation Guide - UIAutomationMcp

UIAutomationMcp is a Windows-only project for live desktop inspection through Windows UI Automation.

## Requirements
- Windows 10 or later
- .NET 9 SDK/runtime for local builds
- access to the target desktop application you want to inspect

## Build from source

```powershell
git clone https://github.com/trsdn/mcp-server-uiautomation.git
cd mcp-server-uiautomation
dotnet build UIAutomationMcp.sln
```

## CLI
Run the CLI directly from source:

```powershell
dotnet run --project src\UIAutomationMcp.CLI\UIAutomationMcp.CLI.csproj -- desktop
```

## MCP server
Run the MCP server from source:

```powershell
dotnet run --project src\UIAutomationMcp.McpServer\UIAutomationMcp.McpServer.csproj
```

## VS Code extension
The extension lives in `vscode-extension` and bundles the UIAutomationMcp CLI and MCP server during packaging.

```powershell
Set-Location vscode-extension
npm install
npm run package
```

## Example MCP configuration
Use the samples in `examples\mcp-configs\` and replace the placeholder path with your local `UIAutomationMcp.McpServer.exe`.

## Project lineage

UIAutomationMcp is its own Windows UI Automation project, but it intentionally keeps visible lineage to Stefan Brönner's earlier MCP automation work and the adjacent Excel MCP family.

