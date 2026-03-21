# UIAutomationMcp Quick Reference

Use this repository for Windows UI Automation tasks against live desktop UI.

## Use UIAutomationMcp when the user wants to
- inspect the desktop root
- inspect the focused element
- resolve an element from a window handle
- search for UI elements by name, class name, or automation id

## Main commands
```powershell
uiamcp desktop
uiamcp focused
uiamcp snapshot
uiamcp find-name "Calculator"
uiamcp find-class "Notepad"
uiamcp find-automation-id "SearchBox"
```

## Repository expectations
- Windows-only
- keep CLI, MCP server, and VS Code extension consistent
- keep repo language aligned with UI Automation, not legacy pre-migration product wording

