# UI Automation COM reference

This document summarizes the COM model and practical interop choices used by UIAutomationMcp.

## Core COM objects

Windows UI Automation is COM-based. The desktop is exposed as a tree of `IUIAutomationElement` instances, and clients reach that tree through `IUIAutomation`.

Important entry points:
- `CUIAutomation`
- `CUIAutomation8`
- `IUIAutomation`
- `IUIAutomationElement`
- `IUIAutomationCondition`
- `IUIAutomationTreeWalker`

The desktop root element is obtained via `IUIAutomation.GetRootElement()`.

## Activation notes

On this machine, direct COM activation by CLSID works reliably:
- `CUIAutomation` -> `{FF48DBA4-60EF-4201-AA87-54103EEF594E}`
- `CUIAutomation8` -> `{E22AD333-B25F-460C-83D0-0581107395C9}`

The UI Automation type library GUID is:
- `{944DE083-8FB8-45CF-BCB7-C477ACB2F897}`

For SDK-style .NET builds, the practical typed path in this repository is the `Interop.UIAutomationClient` package:

```csharp
using Interop.UIAutomationClient;

IUIAutomation automation = new CUIAutomation8Class();
```

## Search model

UI Automation works over an element tree rather than a document model.

Typical starting points:
- `GetRootElement()`
- `ElementFromHandle(hwnd)`
- `ElementFromPoint(x, y)`
- `GetFocusedElement()`

Typical condition helpers:
- `CreatePropertyCondition(...)`
- `CreateAndCondition(...)`
- `CreateOrCondition(...)`
- `CreateTrueCondition()`
- `CreateTreeWalker(...)`

## Views and traversal

UI Automation exposes three common views:
- raw view
- control view
- content view

In this repo, user-facing inspection defaults should stay conservative and understandable. Control view is the safest default for discovery, while raw view is more appropriate for diagnostics.

## Patterns

Element capability is often discovered through control patterns rather than element type alone.

`supportedPatterns` reports every pattern a provider advertises, but advertising a
pattern is not the same as this toolkit being able to use it. The table below is the
authoritative status per pattern:

| Pattern | Status | Surface |
| --- | --- | --- |
| Invoke | actionable | `action invoke` |
| Value | readable + actionable | `valuePattern`, `action set-value` |
| RangeValue | readable + actionable | `rangeValuePattern`, `action set-range-value` |
| Toggle | readable + actionable | `togglePattern`, `action toggle` |
| ExpandCollapse | readable + actionable | `expandCollapsePattern`, `action expand`/`collapse` |
| Window | readable + actionable | `windowPattern`, `action maximize`/`minimize`/`restore`/`close` |
| Scroll | readable + actionable | `scrollPattern`, `action scroll`/`scroll-percent` |
| SelectionItem | readable + actionable | `selectionItemPattern`, `action select`/`add-to-selection`/`remove-from-selection` |
| Transform | actionable | `action move`/`resize` |
| MultipleView | readable + actionable | `multipleViewPattern`, `action set-view` |
| Dock | readable + actionable | `dockPattern`, `action dock` |
| Text / Text2 | readable | `text` command |
| Selection / Selection2 | readable | `selection` command |
| Grid, GridItem, Table, TableItem | detect-only | tracked in the pattern-coverage epic |
| LegacyIAccessible | detect-only | tracked in the pattern-coverage epic |
| ItemContainer, VirtualizedItem | detect-only | tracked in the pattern-coverage epic |
| Drag, DropTarget, TextChild, TextEdit | detect-only | tracked in the pattern-coverage epic |
| Annotation, Styles, Spreadsheet, SpreadsheetItem, CustomNavigation, ObjectModel, SynchronizedInput, Transform2, ScrollItem | detect-only | no consumer planned |

### MultipleView

`multipleViewPattern` reports `currentView`, `currentViewName`, and the full
`supportedViews` list as id/name pairs. View names are provider-supplied and
therefore **localized** — on a German Windows the Explorer item view reports
`Details`, `Kacheln`, `Liste` and so on.

`action set-view` accepts either an id or a name. A numeric argument is only
treated as an id when the control actually advertises that id, so controls whose
view *names* are numeric stay addressable by name. Unknown views fail with the
list of available id/name pairs rather than a generic error.

### Dock

`dockPattern` reports `dockPosition` and `dockPositionName`. `action dock` accepts
`top`, `left`, `bottom`, `right`, `fill`, and `none`.

Dock is rare in practice. Most Win32 and WinForms controls are MSAA-bridged and
expose only `LegacyIAccessible`; a real `IDockProvider` generally comes from WPF or
a custom UIA provider.

Future service-layer operations should consider both element identity and supported patterns.

## Threading guidance

UI Automation is COM and must be treated carefully.

Practical rules:
- keep client creation inside one interop layer
- avoid scattering COM activation through the codebase
- keep STA-sensitive work inside audited helpers
- isolate retry or wait logic when it is introduced

## Lifetime guidance

Even though UIA is query-heavy, it still uses COM proxies.

Guidelines:
- release temporary COM proxies created in hot loops
- prefer DTOs at the public boundary
- keep raw COM interfaces inside the interop layer where possible

## Current repository focus

The current repository surface centers on:
1. typed client bootstrap
2. desktop and focused-element inspection
3. handle-based lookup
4. search by name, class name, and automation id
5. CLI and MCP exposure for those queries

## References
- Microsoft Learn: UI Automation overview
- Microsoft Learn: `IUIAutomation`
- Microsoft Learn: `CUIAutomation`
- NuGet: `Interop.UIAutomationClient`

