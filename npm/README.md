# uia-mcp — Windows UI Automation MCP Server

MCP server that lets an AI assistant inspect and drive the Windows desktop through
UI Automation: elements, trees, text, selection, control patterns and events.

**Windows only.** Requires an interactive desktop session — UI Automation cannot
run headless or over a disconnected remote session.

The bundled server is a win-x64 build. It also installs and runs on Windows ARM64,
where it executes under the built-in x64 emulation.

## Install

Nothing to install ahead of time. The .NET runtime is bundled, so this works on a
machine with no .NET installed:

```json
{
  "mcpServers": {
    "uia-mcp": {
      "command": "npx",
      "args": ["-y", "uia-mcp"]
    }
  }
}
```

### VS Code / GitHub Copilot

```json
{
  "servers": {
    "uia-mcp": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "uia-mcp"]
    }
  }
}
```

Or install it globally and call it by name:

```powershell
npm install --global uia-mcp
```

```json
{
  "mcpServers": {
    "uia-mcp": { "command": "uia-mcp" }
  }
}
```

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
| `uia_text` | Read text content, caret, annotations, TextChild container, and IME composition state |
| `uia_selection` | Read selection state via the Selection pattern |
| `uia_table` | Read a Grid/Table control as a cell matrix with headers |
| `uia_action` | Invoke, focus, toggle, expand, scroll, resize, and more |
| `uia_wait_event` | Wait for focus, property, structure, or automation events |

## Other install channels

This package ships the MCP server only. The same build is also available as:

- **VS Code extension** — [`trsdn.uia-mcp`](https://marketplace.visualstudio.com/items?itemName=trsdn.uia-mcp),
  which bundles the server, the `uiamcp` CLI and the agent skills
- **Claude Desktop** — `uia-mcp-<version>.mcpb` from the
  [latest release](https://github.com/trsdn/mcp-server-uiautomation/releases/latest)
- **NuGet .NET tools** — `UIAutomationMcp.McpServer` (server) and `UIAutomationMcp.CLI` (CLI)

## Privacy

The server runs entirely on your machine and only reads the UI state you ask it about.
No data is sent anywhere.

## Links

- [GitHub Repository](https://github.com/trsdn/mcp-server-uiautomation)
- [Issues](https://github.com/trsdn/mcp-server-uiautomation/issues)
- [Changelog](https://github.com/trsdn/mcp-server-uiautomation/blob/main/CHANGELOG.md)

MIT licensed.
