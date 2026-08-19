---
mcp-name: io.github.trsdn/mcp-server-uiautomation
---

# UIAutomationMcp MCP Server

Windows UI Automation MCP server for AI assistants. Inspect the live desktop, look up
elements, walk the automation tree, read text and selection, drive control patterns, and
wait for automation events — all over the Model Context Protocol.

**Windows only.** Requires an interactive desktop session.

## Install

```powershell
dotnet tool install --global UIAutomationMcp.McpServer
```

## Configure

### VS Code / GitHub Copilot

```json
{
  "servers": {
    "uia-mcp": {
      "type": "stdio",
      "command": "uiamcp-server"
    }
  }
}
```

### Claude Desktop

```json
{
  "mcpServers": {
    "uia-mcp": {
      "command": "uiamcp-server"
    }
  }
}
```

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

- Windows
- .NET 9 Runtime or SDK
- An interactive desktop session (UI Automation cannot run headless)

## Links

- [GitHub Repository](https://github.com/trsdn/mcp-server-uiautomation)
- [Changelog](https://github.com/trsdn/mcp-server-uiautomation/blob/main/CHANGELOG.md)
- [Issues](https://github.com/trsdn/mcp-server-uiautomation/issues)

## License

MIT
