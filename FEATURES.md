# UIAutomationMcp Feature Surface

## Implemented

### Bootstrap and interop
- typed UI Automation client via `Interop.UIAutomationClient`
- `CUIAutomation8Class` primary activation
- fallback to `CUIAutomationClass`
- STA-safe execution wrapper for UIA COM access

### Queries
- desktop root probe
- focused element probe
- rich element metadata projection
- element lookup from native window handle
- element lookup from screen point
- search by name
- search by class name
- search by automation id
- search by framework id
- search by control type
- search by process id
- multi-result `FindAll(...)`
- immediate child enumeration from raw, control, and content views
- descendant enumeration from raw, control, and content views
- tree navigation via parent, child, sibling, and normalize operations
- raw, control, and content tree walkers
- build-cache request support for inspect/search/navigation
- text extraction and selected-text reads
- selection-state reads
- one-shot event waits for focus, automation, property-changed, and structure-changed events
- default render-endpoint mute state reads and updates
- supported-pattern discovery
- pattern state projection for value, range value, toggle, expand/collapse, window, scroll, and selection item

### Service layer
- `UiAutomationService.ProbeDesktop()`
- `UiAutomationService.CaptureSnapshot()`
- `UiAutomationService.GetFocusedElement()`
- `UiAutomationService.GetElementFromHandle(...)`
- `UiAutomationService.GetElementFromPoint(...)`
- `UiAutomationService.FindFirstByName(...)`
- `UiAutomationService.FindFirstByClassName(...)`
- `UiAutomationService.FindFirstByAutomationId(...)`
- `UiAutomationService.Inspect(...)`
- `UiAutomationService.TryInspect(...)`
- `UiAutomationService.FindAll(...)`
- `UiAutomationService.ListChildren(...)`
- `UiAutomationService.ListDescendants(...)`
- `UiAutomationService.Navigate(...)`
- `UiAutomationService.ReadText(...)`
- `UiAutomationService.ReadSelection(...)`
- `UiAutomationService.GetSystemAudioState()`
- `UiAutomationService.SetSystemAudioMute(...)`
- `UiAutomationService.ToggleSystemAudioMute()`
- `UiAutomationService.PerformAction(...)`
- `UiAutomationService.WaitForEvent(...)`

### Public surfaces
- CLI: `uiamcp`
- MCP server: `UIAutomationMcp.McpServer`
- VS Code extension: `uia-mcp`

### CLI and MCP coverage
- inspect elements with generic locators (`--root`, `--focused`, `--handle`, `--x/--y`, property filters)
- search with scope and max-result controls
- enumerate immediate children from raw, control, or content view
- enumerate descendants from raw, control, or content view
- navigate UIA trees from the same locator model
- opt into build-cache requests
- read text and selection information
- wait for one-shot UIA events
- read and change the default system-audio mute state
- execute focus, invoke, value, selection, window, transform, scroll, and range-value actions

### Validation
- `UIAutomationMcp.Smoke`
- CLI smoke commands for desktop and focused-element inspection
- extension packaging that bundles the CLI and MCP server

## Near-term additions
- broader pattern coverage beyond the currently exposed high-value patterns
- longer-lived event subscriptions and higher-level event workflows
- higher-level workflows on top of the generic locator/action layer

