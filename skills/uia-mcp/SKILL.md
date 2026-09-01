---
name: uia-mcp
description: Inspect and drive live Windows desktop UI through the UIAutomationMcp MCP server - probe the desktop, resolve elements by name, class, automation id, handle or screen point, walk the automation tree, read text, selection and tables, invoke control patterns, and wait for automation events. Windows only.
---

Use the UIAutomationMcp MCP server to see and drive real Windows desktop UI.
Every tool returns JSON.

## Tools

| Tool | Purpose |
| --- | --- |
| `uia_desktop` | Probe the desktop root; confirms UI Automation is reachable |
| `uia_snapshot` | Desktop root plus focused element in one call |
| `uia_focused` | The element with keyboard focus |
| `uia_handle` | Element owning a native window handle |
| `uia_point` | Element at a screen point |
| `uia_find_name` / `uia_find_class` / `uia_find_automation_id` | Single-criterion shortcuts |
| `uia_inspect` | Full metadata for one element, using the generic locator |
| `uia_find` | Multiple matches, with scope and result cap |
| `uia_children` / `uia_descendants` | Enumerate from raw, control, or content view |
| `uia_navigate` | Parent, first/last child, sibling, normalize |
| `uia_text` | Document text, selection, caret, annotations, IME state, TextChild |
| `uia_selection` | Selection container state |
| `uia_table` | Read a Grid/Table control as a cell matrix with headers |
| `uia_action` | Invoke, focus, toggle, expand, scroll, move, resize, and more |
| `uia_wait_event` | One-shot wait for a UI Automation event |

## The locator model

Most tools share the same parameters: `root`, `focused`, `fromFocused`,
`handle`, `x`/`y`, `name`, `className`, `automationId`, `frameworkId`,
`controlType`, `processId`, `scope`, plus the negative forms `notName`,
`notClassName`, `notAutomationId`, `notControlType`. They AND-compose.

Prefer `uia_table` over walking cells with `uia_descendants`, and prefer a
narrow locator over fetching a large result set and filtering.

`uia_inspect` accepts `tryInspect: true` to return `null` instead of failing
when nothing matches — use it to assert that something is *gone*, such as a
dialog that should have closed.

## Two behaviours that surprise people

**Virtualized items.** A virtualizing container only materializes what it is
showing, so a 300-file folder may expose a dozen live elements. `uia_inspect`,
`uia_find` and `uia_action` automatically fall back to asking `ItemContainer`
providers and realizing the result, so a plain locator usually just works. Pass
`realizeVirtualized: false` when you specifically want to prove absence.

**MSAA-bridged windows.** Win32 controls, MFC dialogs and most installers never
got a native UI Automation provider, so they look near-empty: generic control
types, missing names, no usable actions. The data is on the other side of the
bridge. Check `legacyAccessiblePattern`, and drive them with `uia_action`
`default-action` when they support no modern actionable pattern.

## Reading the output

- `supportedPatterns` lists what a provider advertises; the `*Pattern` blocks
  carry the state actually readable.
- `nameSource` says where `name` came from: `uia`, `legacy` (MSAA bridge), or
  `labeledBy`. Unnamed Win32 inputs are usually identifiable only through their
  label.
- `fullDescription` is where WinUI, UWP and Edge often put the meaningful text
  when `name` is terse.

## Events

`uia_wait_event` takes `eventKind`: `focus`, `automation`, `property`,
`structure`, `text-edit`, or `notification`.

`notification` carries provider announcements ("File saved", "3 results found")
in `displayString`, and is often the only signal that an operation completed.
Drag and drop are `automation` events: 20026 start, 20027 cancel, 20028
complete, 20029 drag enter, 20030 drag leave, 20031 dropped.

Windows only. Requires an interactive desktop session.
