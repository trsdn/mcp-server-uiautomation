# UI Automation MCP

Use the bundled MCP server for desktop inspection tasks on Windows.

Best for:

- reading the focused element
- reading desktop root metadata
- finding elements by name
- finding elements by class name
- finding elements by automation id
- resolving elements from native window handles
- resolving elements from screen coordinates
- inspecting elements with cache-enabled lookups
- navigating parent, child, and sibling relationships
- reading text and selection state, including caret offset, annotations, and TextChild containers
- reading tabular controls as a cell matrix with row and column headers
- waiting for focus, automation, property, structure, or text-edit events
- running supported actions such as focus, invoke, set-value, toggle, expand/collapse, window state changes, move/resize, scroll, range-value updates, view switching, docking, realizing virtualized items, and MSAA default actions

Useful MCP workflows:

- inspect an element with `uia_inspect`
- search multiple matches with `uia_find`
- move through the UI tree with `uia_navigate`
- read text with `uia_text`
- read selection data with `uia_selection`
- read a grid or table with `uia_table` instead of walking cells with `uia_descendants`
- wait for the next event with `uia_wait_event`
- execute actions with `uia_action`

Long lists and grids are virtualized: only the rows currently on screen exist as UI
Automation elements. `uia_inspect`, `uia_find`, and `uia_action` therefore fall back to
asking the container for items that have not been materialized, so you can address the
250th row of a 300-row list without scrolling first. Pass `realizeVirtualized: false`
when you deliberately want to assert that an element is absent from the live tree.

Old Win32, MFC, and installer windows expose no real UI Automation provider; UIA bridges
their MSAA data instead. Those elements report a `legacyAccessiblePattern` block (role,
state flags, keyboard shortcut, default action), fill in an empty name or control type
from the bridge while marking it via `nameSource` / `localizedControlTypeSource`, and can
be driven with `uia_action` `default-action` when they support no modern actionable
pattern.

`uia_text` covers more than the raw string: `caret` gives the caret offset and the text
of the line it sits on, `annotations` lists marked-up runs such as spelling and grammar
errors, and `textEdit` reports IME composition state. It also works on elements that are
not text controls — a hyperlink or an image returns a `textChild` block naming the
document that contains it and the element's offset within it, which is how you locate an
inline element inside a page. Use `uia_wait_event` with `eventKind: "text-edit"` to see
when an app rewrites typed input; the substituted text arrives in `eventStrings`.

Drag and drop are read-only in UI Automation: a provider reports drag state but exposes
no way to start a drag, and this tool deliberately does not synthesize mouse input to
fake one. Elements report `dragPattern` (`isGrabbed`, `dropEffect`, `grabbedItems`) and
`dropTargetPattern`. To watch a drag that is already happening, use `uia_wait_event` with
`eventKind: "automation"` and event id 20026 (drag start), 20027 (cancel), 20028
(complete), 20029 (drag enter), 20030 (drag leave) or 20031 (dropped); results include a
readable `eventName`.
