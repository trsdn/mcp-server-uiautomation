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
- running supported actions such as focus, invoke, set-value, toggle, expand/collapse, window state changes, move/resize, scroll, range-value updates, view switching, and docking

Useful MCP workflows:

- inspect an element with `uia_inspect`
- search multiple matches with `uia_find`
- move through the UI tree with `uia_navigate`
- read text with `uia_text`
- read selection data with `uia_selection`
- read a grid or table with `uia_table` instead of walking cells with `uia_descendants`
- wait for the next event with `uia_wait_event`
- execute actions with `uia_action`
