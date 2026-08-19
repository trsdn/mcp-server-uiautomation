# Windows UI Automation — Claude Desktop Bundle

MCP server that lets Claude inspect and drive the Windows desktop through UI Automation.

## Install

1. Download `uia-mcp-{version}.mcpb` from the
   [latest release](https://github.com/trsdn/mcp-server-uiautomation/releases/latest).
2. Double-click the file, or drag it onto the Claude Desktop window.
3. Restart Claude Desktop if prompted.

Everything is bundled — no .NET runtime or SDK is required.

## What you can ask for

- "What window has focus right now?"
- "List the buttons in the Settings window."
- "Find the element with automation id `SearchBox` and click it."
- "Read the text of the element under the mouse pointer."
- "Wait until focus moves to a different control."

## Tools

| Tool | Purpose |
| --- | --- |
| `uia_desktop` | Probe the UI Automation root and COM bootstrap |
| `uia_snapshot` | Capture a snapshot of the current desktop state |
| `uia_focused` | Inspect the currently focused element |
| `uia_handle` | Resolve an element from a window handle |
| `uia_point` | Resolve the element at a screen coordinate |
| `uia_find_name` | Find the first descendant by name |
| `uia_find_class` | Find the first descendant by class name |
| `uia_find_automation_id` | Find the first descendant by automation id |
| `uia_inspect` | Inspect a single element resolved by a locator |
| `uia_find` | Find all elements matching a search request |
| `uia_children` | List direct children of an element |
| `uia_descendants` | List descendants of an element |
| `uia_navigate` | Walk parent, child, and sibling relationships |
| `uia_text` | Read text content via the Text pattern |
| `uia_selection` | Read selection state via the Selection pattern |
| `uia_action` | Invoke, focus, toggle, expand, scroll, resize, and more |
| `uia_wait_event` | Wait for focus, property, structure, or automation events |

## Requirements

- Windows (x64)
- An interactive desktop session — UI Automation cannot run headless or over a
  disconnected remote session

## Privacy

The server runs entirely on your machine and only reads the UI state you ask it about.
No data is sent anywhere.

## Links

- [GitHub Repository](https://github.com/trsdn/mcp-server-uiautomation)
- [Issues](https://github.com/trsdn/mcp-server-uiautomation/issues)
- [Changelog](https://github.com/trsdn/mcp-server-uiautomation/blob/main/CHANGELOG.md)

MIT licensed.
