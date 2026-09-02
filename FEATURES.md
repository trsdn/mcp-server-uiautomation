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
- element relationship properties (`labeledBy`, `controllerFor`, `describedBy`, `flowsTo`, `flowsFrom`), with `labeledBy` acting as a third name-resolution tier after the MSAA bridge
- extended element properties from `IUIAutomationElement2..9` (`fullDescription`, `positionInSet`, `sizeOfSet`, `level`, `landmarkType`, `headingLevel`, `isDialog`, `isPeripheral`, `liveSetting`, `annotationTypes`), soft-cast per interface level so an older Windows build degrades to `null` instead of failing
- element lookup from native window handle
- element lookup from screen point
- search by name
- search by class name
- search by automation id
- search by framework id
- search by control type
- search by process id
- multi-result `FindAll(...)`
- negative locator criteria (`--not-name`, `--not-class`, `--not-automation-id`, `--not-control-type`) composed through `CreateNotCondition`
- non-throwing lookup via `inspect --try` / `tryInspect`, so absence is a result rather than an error
- immediate child enumeration from raw, control, and content views
- descendant enumeration from raw, control, and content views
- tree navigation via parent, child, sibling, and normalize operations
- raw, control, and content tree walkers
- build-cache request support for inspect/search/navigation
- text extraction and selected-text reads, plus caret position, annotation runs (spelling and grammar errors, comments, highlights), IME composition state, and TextChild container lookup for inline elements
- selection-state reads
- one-shot event waits for focus, automation, property-changed, structure-changed, text-edit (auto-correct, IME composition, auto-complete), notification (provider announcements such as `File saved`, reported in `displayString`), changes, and active-text-position events, with automation events resolved to a readable `eventName` so drag start/cancel/complete and drop enter/leave/dropped are observable
- supported-pattern discovery
- pattern state projection for value, range value, toggle, expand/collapse, window, scroll, selection item, multiple view, dock, transform, grid, grid item, table, table item, drag, drop target, and legacy IAccessible
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
- `UiAutomationService.ReadText(...)` (optional `findText` locates a run by offset, length and screen rectangles)
- `UiAutomationService.ReadSelection(...)`
- `UiAutomationService.ReadTable(...)`
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
- execute focus, invoke, value, selection, window, transform (move/resize/rotate), scroll, scroll-into-view, text (select-text/move-caret/scroll-text-into-view), range-value, view-switch, dock, realize, and default-action verbs

### Validation
- `UIAutomationMcp.Smoke`
- CLI smoke commands for desktop and focused-element inspection
- extension packaging that bundles the CLI and MCP server

### Text range operations
- `text --find` locates a run and reports offset, length and screen rectangles, case-insensitive and forward by default with `--match-case` and `--find-backward` available
- `select-text`, `move-caret` and `scroll-text-into-view` act on a run addressed by search string or by offset, since UIA text ranges are live COM objects that cannot be carried between calls

## Near-term additions
- longer-lived event subscriptions and higher-level event workflows
- higher-level workflows on top of the generic locator/action layer

