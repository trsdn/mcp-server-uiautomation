# UIAutomationMcp

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://learn.microsoft.com/windows/win32/winauto/entry-uiauto-win32)

Windows UI Automation toolkit with three aligned entry points:
- CLI (`uiamcp`)
- MCP server (`UIAutomationMcp.McpServer`)
- VS Code extension (`uia-mcp`)

## Install

**VS Code extension** (recommended) — search for "UI Automation MCP" in the Marketplace, or
install the `UIAutomationMcp-<version>.vsix` from the
[latest release](https://github.com/trsdn/mcp-server-uiautomation/releases/latest).
Self-contained; bundles the MCP server, the CLI, and both agent skills.

**Claude Desktop** — download `uia-mcp-<version>.mcpb` from the latest release and
double-click it.

**NuGet .NET tools**

```powershell
dotnet tool install --global UIAutomationMcp.McpServer   # MCP server -> uiamcp-server
dotnet tool install --global UIAutomationMcp.CLI         # CLI        -> uiamcp
```

**Standalone ZIP** — `UIAutomationMcp-MCP-Server-<version>-windows.zip` contains the
self-contained MCP server and CLI; no .NET runtime required.

Requires Windows and an interactive desktop session. UI Automation cannot run headless.

## Lineage

This repository is part of the broader MCP automation lineage and intentionally keeps attribution to earlier work by Stefan Brönner.

It also keeps the historical reference to Excel MCP as a related sibling project in that lineage, even though this repository itself is focused on Windows UI Automation.

## What it does
- boots a typed UI Automation client through `Interop.UIAutomationClient`
- reads rich desktop-root and focused-element metadata, including bounds, runtime IDs, and supported patterns
- resolves elements from native window handles and screen coordinates
- searches UIA trees by name, class name, automation id, framework id, control type, and process id
- enumerates immediate children from raw, control, or content tree views
- enumerates descendants from raw, control, or content tree views
- navigates the UIA tree with raw, control, and content walkers
- supports build-cache requests for inspection, search, and navigation
- reads text, selection, and key pattern states
- waits for focus, automation, property-changed, and structure-changed events
- performs common actions such as focus, invoke, set-value, toggle, expand/collapse, selection, window state changes, move/resize, scroll, and range-value updates
- exposes the same expanded surface through CLI and MCP
- packages the CLI and MCP server inside the VS Code extension

## Repository layout
- `src\UIAutomationMcp.ComInterop`
- `src\UIAutomationMcp.Service`
- `src\UIAutomationMcp.CLI`
- `src\UIAutomationMcp.McpServer`
- `src\UIAutomationMcp.Smoke`
- `vscode-extension`
- `mcpb` — Claude Desktop bundle sources and build script
- `skills` — agent skills for `uia-cli` and `uia-mcp`

## Build and validate
```powershell
dotnet build .\UIAutomationMcp.sln -c Release
.\src\UIAutomationMcp.Smoke\bin\Release\net9.0-windows\UIAutomationMcp.Smoke.exe
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe inspect --root --cache
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe find --focused --scope children --max-results 10 --cache
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe children --focused --view raw --max-results 10
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe descendants --focused --view raw --max-results 25
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe wait-event --event-kind focus --timeout-ms 500
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe action focus --focused
```

## Related docs
- [UI Automation COM Reference](docs/UIAUTOMATION-COM-REFERENCE.md)
- [Installation Guide](docs/INSTALLATION.md)
- [Development Workflow](docs/DEVELOPMENT.md)
- [Contributing](docs/CONTRIBUTING.md)

