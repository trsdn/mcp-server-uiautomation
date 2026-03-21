# UIAutomationMcp Validation Notes

## Current validation workflow

```powershell
dotnet build .\UIAutomationMcp.sln -c Release
.\src\UIAutomationMcp.Smoke\bin\Release\net9.0-windows\UIAutomationMcp.Smoke.exe
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe desktop
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe focused
```

## What gets verified
- typed UI Automation bootstrap
- desktop root lookup
- focused-element lookup
- handle lookup
- subtree search helpers
- CLI output shape
- MCP server buildability and stdio safety

## Environment assumptions
- Windows desktop session
- `Interop.UIAutomationClient` available at build/runtime
- interactive desktop so the root and focused element can be resolved

