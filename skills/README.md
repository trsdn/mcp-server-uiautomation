# UIAutomationMcp Skills

Two skill entry points for UI Automation workflows:

- `uia-cli` — driving the `uiamcp` terminal CLI
- `uia-mcp` — driving the MCP server from an assistant

Both carry `name` and `description` frontmatter. The description is what skill
discovery matches on, so it names the concrete capabilities rather than
describing the repository.

## Relationship to the extension copies

`vscode-extension/skills-src/` holds a **separate** set of documents that are
packaged into the VS Code extension, and `vscode-extension/skills/` is generated
from them at package time (see `.gitignore`).

The split is deliberate, and the two are not copies of each other:

| | `skills/` | `vscode-extension/skills-src/` |
| --- | --- | --- |
| Consumer | Copilot skill discovery | VS Code extension users |
| Frontmatter | required (`name`, `description`) | none |
| Assumes | `uiamcp` on `PATH` | the bundled `uiamcp.exe` |

They drifted once before, with the extension copies carrying newer content while
the root copies went stale. When you add or change a CLI command, an MCP tool, or
an action verb, update both — and derive the list from the code
(`src/UIAutomationMcp.CLI/Program.cs`,
`src/UIAutomationMcp.McpServer/Tools/UiAutomationQueryTool.cs`) rather than from
whichever document is nearest.

`scripts/check-cli-coverage.ps1` catches the subset of this drift that is
mechanically checkable: verbs missing from CLI help, CLI/MCP count mismatch, and
stale tool counts anywhere in the repository.
