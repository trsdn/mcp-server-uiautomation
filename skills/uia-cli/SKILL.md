---
name: uia-cli
description: Inspect and drive live Windows desktop UI from the terminal with the uiamcp CLI - probe the desktop, find elements by name, class, automation id or screen point, walk the automation tree, read text, selection and tables, invoke control patterns, and wait for automation events. Windows only.
---

Use `uiamcp` when you need to see or drive real desktop UI from a terminal.
Everything prints JSON on stdout; errors go to stderr with a non-zero exit code.

## Start here

```powershell
uiamcp desktop      # is UI Automation reachable at all
uiamcp focused      # what has keyboard focus right now
uiamcp snapshot     # desktop root plus focused element
```

Single-criterion shortcuts, when you only need the first match:

```powershell
uiamcp find-name "Calculator"
uiamcp find-class "Notepad"
uiamcp find-automation-id "SearchBox"
uiamcp handle --handle 0x1234
uiamcp point --x 400 --y 300
```

For anything more specific, use `inspect` or `find` with the locator flags
below — they compose, and the shortcuts do not.

## Locator flags

Almost every command takes the same locator. Combine flags to narrow; they are
AND-composed.

| Flag | Selects |
| --- | --- |
| `--root` | the desktop root |
| `--focused` | the focused element |
| `--from-focused` | search starting at the focused element |
| `--handle <hwnd>` | element owning a native window handle |
| `--x <x> --y <y>` | element at a screen point |
| `--name <text>` | by name |
| `--class <class>` | by class name |
| `--automation-id <id>` | by automation id |
| `--framework-id <id>` | by framework (Win32, WPF, XAML, Chrome) |
| `--control-type <id>` | by control type id (50000 Button, 50004 Edit, 50032 Window) |
| `--process-id <pid>` | by owning process |
| `--not-name`, `--not-class`, `--not-automation-id`, `--not-control-type` | exclude matches |
| `--scope <element\|children\|descendants\|subtree>` | search depth |
| `--no-virtualized` | do not ask ItemContainer providers for unmaterialized items |
| `--cache`, `--cache-scope`, `--cache-view` | build-cache request |

## Finding and walking

```powershell
uiamcp find --root --control-type 50000 --max-results 50      # all buttons
uiamcp find --root --class Shell_TrayWnd --not-name "Taskbar" # exclusions
uiamcp inspect --name "Save" --try                            # null + exit 0 if absent
uiamcp children --handle 0x1234 --view control
uiamcp descendants --focused --view raw --max-results 100
uiamcp navigate --focused --direction parent
```

`inspect --try` is how you assert something is *gone*: it returns `null` and
exits 0 instead of failing. Pair it with `--no-virtualized` when you need to
prove an item does not exist rather than merely being unmaterialized.

## Reading content

```powershell
uiamcp text --focused        # document text, selection, caret, annotations, IME state
uiamcp selection --focused   # selection container state
uiamcp table --root --class UIItemsView --max-rows 20 --max-columns 4
```

`table` reads a Grid/Table control as a cell matrix with headers, so you do not
have to walk descendants and reconstruct coordinates. Cells the provider cannot
realize come back with `isUnavailable: true` rather than aborting the read.

## Acting

```powershell
uiamcp action invoke --name "OK"
uiamcp action set-value "hello" --automation-id "SearchBox"
uiamcp action toggle --name "Show hidden files"
uiamcp action expand --name "Documents"
uiamcp action select --name "Report 0350"
uiamcp action maximize --handle 0x1234
uiamcp action move 100 200 --name "Calculator"
uiamcp action rotate 90 --name "Canvas"
uiamcp action scroll-percent 0 50 --class "ScrollViewer"
uiamcp action scroll-into-view --name "Report 0350"
uiamcp action realize --name "Report 0350"
uiamcp action default-action --name "Select"
```

Full verb list: `focus`, `invoke`, `set-value`, `expand`, `collapse`, `toggle`,
`select`, `add-to-selection`, `remove-from-selection`, `maximize`, `minimize`,
`restore`, `close`, `move`, `resize`, `rotate`, `scroll`, `scroll-percent`,
`scroll-into-view`, `set-range-value`, `set-view`, `dock`, `realize`,
`default-action`.

Two that are easy to miss:

- **`default-action`** drives Win32 and MFC controls that expose no modern
  actionable pattern, through the MSAA bridge. When `invoke` fails on a bridged
  element, the error points here.
- **`realize` then `scroll-into-view`** reaches items a virtualizing container
  has not materialized. Element lookup already falls back to `ItemContainer`
  automatically, so try the plain locator first.

## Waiting for events

```powershell
uiamcp wait-event --event-kind focus --timeout-ms 5000
uiamcp wait-event --event-kind notification --root --timeout-ms 10000
uiamcp wait-event --event-kind property --property-id 30013 --focused
uiamcp wait-event --event-kind structure --root
uiamcp wait-event --event-kind text-edit --focused
uiamcp wait-event --event-kind automation --event-id 20028 --root   # drag complete
```

`notification` carries provider announcements ("File saved", "3 results found")
in `displayString`, and is often the only signal that an operation finished.

Drag and drop are automation events: 20026 start, 20027 cancel, 20028 complete,
20029 drag enter, 20030 drag leave, 20031 dropped.

## Reading the output

- `supportedPatterns` lists what a provider advertises; the `*Pattern` blocks
  carry the state this tool can actually read.
- `nameSource` says where `name` came from: `uia`, `legacy` (MSAA bridge), or
  `labeledBy` (the labelling element). Unnamed Win32 inputs are usually
  identifiable only through `labeledBy`.
- `fullDescription` is where WinUI, UWP and Edge often put the meaningful text
  when `name` is terse.

Windows only. Requires an interactive desktop session.
