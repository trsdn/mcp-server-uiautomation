# UI Automation MCP - Windows Desktop Automation for Copilot

[![GitHub](https://img.shields.io/badge/GitHub-trsdn%2Fmcp--server--uiautomation-blue)](https://github.com/trsdn/mcp-server-uiautomation)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)


**Inspect and query Windows desktop UI with AI through GitHub Copilot.**

This extension bundles both the `UIAutomationMcp` MCP server and the `uiamcp` CLI. It enables GitHub Copilot and other MCP-capable tools to inspect desktop UI elements through Windows UI Automation.

Included surfaces:

- MCP server for Copilot and other MCP clients
- bundled CLI for direct terminal use
- skill files for UIA-oriented guidance in VS Code

## Features

The current UI Automation bundle provides:

- desktop root inspection
- focused-element lookup
- handle-to-element resolution
- point-to-element resolution
- subtree search by name, class name, automation id, framework id, control type, and process id
- tree navigation with raw, control, and content views
- text and selection reads
- cache-aware inspect/search/navigation
- one-shot UIA event waiting
- common actions such as focus, invoke, set-value, toggle, expand/collapse, selection, window state changes, move/resize, scroll, range-value updates, view switching, and docking
- bundled CLI and MCP access to the same aligned surface

📚 **[Complete Feature Reference →](https://github.com/trsdn/mcp-server-uiautomation/blob/main/FEATURES.md)**

## Lineage

This extension keeps visible lineage to Stefan Brönner's earlier MCP automation work and intentionally retains a brief reference to Excel MCP as a related sibling project.

### Agent Skills (Bundled)

This extension includes bundled skill files for both MCP and CLI usage:

- `uia-mcp` - MCP workflow guidance
- `uia-cli` - CLI workflow guidance

**VS Code setup:** Enable the preview setting `chat.useAgentSkills` to allow Copilot to load skills. Skills are registered via VS Code's `chatSkills` contribution point and managed automatically.


## 💬 Example Prompts

- *"What is the currently focused control on my desktop?"*
- *"Find the first element with automation id SearchBox."*
- *"Resolve the UI Automation element for this window handle."*
- *"Inspect the desktop root and tell me the class name."*
- *"Wait for the next focus-changed event for 500 milliseconds."*
- *"Invoke the currently focused control."*


## Quick Start

1. **Install this extension** (you just did!)
2. **Ask Copilot** in the chat panel:
   - "Show me the currently focused UI Automation element"
   - "Find the first desktop element named Start"
   - "Look up the element behind a specific HWND"
   - "Wait for the next UI Automation focus event"
   - "Inspect the focused element with cache enabled"

You can also use the bundled CLI through the command palette:

- `UI Automation MCP: Copy CLI Command`
- `UI Automation MCP: Reveal Bundled Tools`

**That's it!** The extension includes self-contained binaries - no external .NET install is required on the target machine.

## Requirements

- **Windows OS** - Windows UI Automation requires Windows
- **Interactive desktop session** - UIA queries inspect the live desktop

## Potential Issues

**Copilot does not see the MCP tools:**
- Restart VS Code after installing or updating the extension
- Open the Output panel and look for extension activation messages

**CLI command does not work:**
- Use the command palette entry to copy the exact bundled CLI path
- Use the reveal command to inspect the `bin` folder that contains the packaged executables

## Documentation & Support

- **[Complete Documentation](https://github.com/trsdn/mcp-server-uiautomation)** - Full guides and examples
- **[Report Issues](https://github.com/trsdn/mcp-server-uiautomation/issues)** - Bug reports and feature requests

## License & Privacy

MIT License - see [LICENSE](https://github.com/trsdn/mcp-server-uiautomation/blob/main/LICENSE)

---

**Built with GitHub Copilot** | **Powered by Model Context Protocol**
