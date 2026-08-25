# Changelog

All notable changes to UIAutomationMcp are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Fixed
- A partially published release can now be finished instead of burning the version number.
  Publishing is not atomic, and the git tag is created before the publish job, so a failure
  in any single registry left the version tagged — the `version` job then refused that tag
  and the run could never be repeated. This is what stranded 1.0.1 as a NuGet-only release.
  Re-run the workflow with `resume: true` and `custom_version: <failed version>`: the tag and
  the GitHub release are reused, the build jobs check out the tag so the artifacts match the
  released commit, and every publish step now skips what is already live (NuGet via
  `--skip-duplicate`, npm and the Marketplace via a registry query). The changelog
  pull request and the release notes handle the resumed case too. 1.0.1 is deliberately left
  as it is rather than backfilled, since publishing it after 1.0.2 would put an older build
  on top.
- The release workflow no longer hides a failed Marketplace publish. The
  `Publish to VS Code Marketplace` step carried `continue-on-error: true`, so when
  `VSCE_TOKEN` was unset it logged `Input required and not supplied: pat` while still
  reporting `conclusion: success` — the 1.0.2 release looked fully green but never reached
  the Marketplace, and the VSIX had to be uploaded by hand. The step is now gated on the
  secret: a missing token is an explicit skip with a warning, and a genuine publish failure
  fails the job.

### Changed
- The automated post-release changelog pull request now says in its own body that it needs
  an admin merge. It is opened by `GITHUB_TOKEN`, and GitHub does not start workflow runs
  for such pushes, so its required status checks never report and it would otherwise sit
  blocked indefinitely.
- `docs/DEVELOPMENT.md` now documents how releases are cut, how to resume a partial one, and
  which secrets the pipeline needs.

## [1.0.2] - 2026-08-25

> **Note on 1.0.1** — the 1.0.1 release run aborted partway through. The NuGet packages
> `UIAutomationMcp.McpServer` and `UIAutomationMcp.CLI` were published at 1.0.1, but npm,
> the VS Code Marketplace, the MCP registry and the GitHub release were not. Everything
> below therefore ships in the next version, and 1.0.1 should be treated as a NuGet-only
> point release.

### Changed
- New project mark. The extension and MCPB icons were still the Excel MCP template
  artwork — a green spreadsheet grid captioned "Excel MCP" — which was live on the
  Marketplace listing and actively misleading about what the extension does. Replaced
  with a window frame under an inspector highlight, the Windows accessibility "Inspect"
  metaphor that matches what this server actually does. `vscode-extension/icon.svg` is
  the source of truth; `vscode-extension/Build-Icons.ps1` regenerates both rasters from
  it and verifies their PNG dimensions. The mark carries no text, because at the 42–90 px
  the Marketplace and extension list render it, a wordmark is unreadable and the name is
  already shown beside the icon.
- The extension's `Data Science` Marketplace category, another Excel MCP leftover, is now
  `Testing`.
- The VS Code extension is now published on the Marketplace as
  [`trsdn.uia-mcp`](https://marketplace.visualstudio.com/items?itemName=trsdn.uia-mcp)
  and can be installed with `code --install-extension trsdn.uia-mcp`. Installing the VSIX
  from a GitHub release still works and remains the fallback when `VSCE_TOKEN` is not
  configured.

### Added
- npm package [`uia-mcp`](https://www.npmjs.com/package/uia-mcp), so the MCP server can be
  run as `npx -y uia-mcp` with no prerequisites — the .NET runtime is bundled. The package
  ships the server only; the CLI stays on NuGet and in the VSIX rather than doubling the
  download. It is built and published by the release workflow (`NPM_TOKEN` secret, npm
  provenance) and is listed alongside the NuGet package in the MCP registry metadata.
- Drag and DropTarget pattern reads. Elements report `dragPattern` (`isGrabbed`,
  `dropEffect`, `dropEffects`, `grabbedItems`) and `dropTargetPattern`
  (`dropTargetEffect`, `dropTargetEffects`), so an in-progress drag can be observed:
  which element is grabbed and what a drop would do. Automation event results now also
  carry a resolved `eventName`, which makes the drag lifecycle events (`Drag_DragStart`
  20026, `Drag_DragCancel` 20027, `Drag_DragComplete` 20028, `DropTarget_DragEnter`
  20029, `DropTarget_DragLeave` 20030, `DropTarget_Dropped` 20031) usable through the
  existing `automation` event kind. No `drag` verb is provided: UI Automation offers no
  method to start a drag, and synthesizing mouse input is explicitly out of scope for
  this tool.
- TextPattern2, TextChild, and TextEdit support. `text` now additionally reports
  `caret` (offset plus the text of the caret's line), `annotations` (annotated format
  runs such as spelling and grammar errors, comments, and highlights, each with type id,
  resolved type name, offset, length, and text), `textEdit` (active IME composition and
  conversion target), and `hasTextPattern2` / `hasTextEditPattern`. Elements that are not
  text controls but sit inside one — hyperlinks, images, inline controls — no longer
  return an empty result: they report a `textChild` block naming the containing document
  and the element's range and offset within it. Adds a `text-edit` event kind to
  `wait-event` / `uia_wait_event` that reports auto-correct, IME composition,
  composition-finalized, and auto-complete changes together with the substituted text in
  `eventStrings`.
- LegacyIAccessible support for MSAA-only applications. Inspected elements report a
  `legacyAccessiblePattern` block (child id, name, value, description, resolved
  `ROLE_SYSTEM_*` role, decoded `STATE_SYSTEM_*` flags, help, keyboard shortcut, and
  default action). An empty native name or localized control type is filled in from the
  bridge, with the origin marked via `nameSource` and `localizedControlTypeSource` so
  bridged data stays distinguishable from native UI Automation data. Adds a
  `default-action` action for controls that expose no modern actionable pattern, a
  legacy `SetValue` fallback for `set-value` when the Value pattern is absent, and an
  `invoke` error that points at `default-action` on MSAA-bridged elements.
- ItemContainer and VirtualizedItem pattern support. Element lookups now fall back to
  `IItemContainerProvider.FindItemByProperty` when a tree search finds nothing, then
  realize the result, so items a virtualizing list has not materialized (for example
  the 250th file in a 300-file Explorer folder) can be inspected and acted on without
  scrolling first. Inspected elements report a `virtualization` block, and a `realize`
  action is available. The fallback only runs on the path that previously failed; pass
  `--no-virtualized` (CLI) or `realizeVirtualized: false` (MCP) to opt out.
- Grid, GridItem, Table, and TableItem pattern support: `gridPattern`,
  `gridItemPattern`, `tablePattern`, and `tableItemPattern` on inspected elements.
- `uiamcp table` / `uia_table`, which reads a tabular control as a rectangular cell
  matrix with row and column headers instead of requiring callers to walk
  descendants and reconstruct coordinates. Supports `--max-rows`/`--max-columns`
  limits and reports whether the result was truncated.
- MultipleView pattern support: `multipleViewPattern` on inspected elements (current
  view id, current view name, and all supported views as id/name pairs) plus a
  `set-view` action that accepts either a view id or a localized view name.
- Dock pattern support: `dockPattern` on inspected elements (dock position and its
  name) plus a `dock` action accepting `top`, `left`, `bottom`, `right`, `fill`, and
  `none`.
- Pattern coverage table in `docs/UIAUTOMATION-COM-REFERENCE.md` recording, per
  pattern, whether it is detect-only, readable, or actionable.

## [1.0.0] - 2026-08-20

### Added
- `--version` flag for both `uiamcp` and the MCP server executable.
- NuGet .NET tool packaging for `UIAutomationMcp.CLI` (`uiamcp`) and
  `UIAutomationMcp.McpServer` (`uiamcp-server`), including the `McpServer` package type.
- MCPB bundle for Claude Desktop under `mcpb/`, built by `mcpb/Build-McpBundle.ps1`.
- Unified release workflow (`.github/workflows/release.yml`) that builds and publishes the
  NuGet tools, VSIX, MCPB bundle, agent skills package, and self-contained ZIP, then creates
  the GitHub release.
- Lightweight root skill stubs for `uia-cli` and `uia-mcp`.
- MCP configuration examples based on `UIAutomationMcp.McpServer.exe`.
- Build-cache support for inspect/search/navigation and one-shot UIA event waiting across the shared service, CLI, and MCP surfaces.

### Changed
- Removed lingering blueprint-era docs, issue templates, skills, and workflow references from the copied baseline.
- Rewrote repo guidance, examples, and maintenance notes to match the active UI Automation codebase.
- Cleaned the VS Code extension package contents and UIA-facing documentation.
- Removed stale copied repo surfaces such as `eval\` and obsolete package artifacts.
- Rewrote `SECURITY.md` for the actual UIAutomationMcp local-desktop security model.

### Fixed
- MCP server tool calls failed at runtime because `UiAutomationService` was never registered
  in the dependency injection container. Every `uia_*` tool now resolves and executes correctly.


