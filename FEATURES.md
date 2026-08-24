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
- text extraction and selected-text reads, plus caret position, annotation runs (spelling and grammar errors, comments, highlights), IME composition state, and TextChild container lookup for inline elements
- selection-state reads
- one-shot event waits for focus, automation, property-changed, structure-changed, and text-edit (auto-correct, IME composition, auto-complete) events, with automation events resolved to a readable `eventName` so drag start/cancel/complete and drop enter/leave/dropped are observable
- supported-pattern discovery
- pattern state projection for value, range value, toggle, expand/collapse, window, scroll, selection item, multiple view, dock, grid, grid item, table, table item, drag, drop target, and legacy IAccessible
- tabular reads that return a Grid/Table control as a cell matrix with row and column headers
- virtualization hints (`isItemContainer`, `isVirtualizedItem`) and an automatic ItemContainer fallback that finds and realizes items a virtualizing control has not materialized

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
- `UiAutomationService.PerformAction(...)`
- `UiAutomationService.WaitForEvent(...)`

### Public surfaces
- CLI: `uiamcp`
- MCP server: `UIAutomationMcp.McpServer`
- VS Code extension: `uia-mcp`
- npm package: `uia-mcp` (MCP server only, run with `npx uia-mcp`)

### CLI and MCP coverage
- inspect elements with generic locators (`--root`, `--focused`, `--handle`, `--x/--y`, property filters)
- search with scope and max-result controls
- enumerate immediate children from raw, control, or content view
- enumerate descendants from raw, control, or content view
- navigate UIA trees from the same locator model
- opt into build-cache requests
- read text and selection information
- read tabular controls as a cell matrix with headers
- wait for one-shot UIA events
- execute focus, invoke, value, selection, window, transform, scroll, range-value, view-switch, dock, realize, and default-action verbs

### Validation
- `UIAutomationMcp.Smoke`
- CLI smoke commands for desktop and focused-element inspection
- extension packaging that bundles the CLI and MCP server

## Near-term additions
- broader pattern coverage beyond the currently exposed high-value patterns
- longer-lived event subscriptions and higher-level event workflows
- higher-level workflows on top of the generic locator/action layer

