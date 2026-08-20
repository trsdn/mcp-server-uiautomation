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
- reading text and selection state
- reading tabular controls as a cell matrix with row and column headers
- waiting for focus, automation, property, or structure events
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
