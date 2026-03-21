---
applyTo: "src/**/*.cs"
---

# UIAutomationMcp Architecture Patterns

- Keep the active layers aligned: ComInterop -> Service -> CLI/MCP Server.
- UI Automation access must remain safe on STA threads.
- Shared behavior belongs in `UIAutomationMcp.Service` or `UIAutomationMcp.ComInterop`, not duplicated across entry points.
- If a UIA query is added publicly, keep CLI and MCP behavior aligned.

