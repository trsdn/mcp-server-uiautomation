# Changelog

All notable changes to UIAutomationMcp are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added
- Element metadata now includes the relationship properties that live on the base
  `IUIAutomationElement` and had simply never been read: `labeledBy`, `controllerFor`,
  `describedBy`, `flowsTo`, plus `flowsFrom` from `IUIAutomationElement2`. They are
  projected as flat element references rather than full element info, because a column
  header is its own column header and full info would not terminate.
- The properties carried by `IUIAutomationElement2` through `IUIAutomationElement9`:
  `fullDescription`, `positionInSet`, `sizeOfSet`, `level`, `landmarkType`,
  `localizedLandmarkType`, `headingLevel`, `isDialog`, `isPeripheral`, `liveSetting`,
  `annotationTypes` and `optimizeForVisualContent`. Each interface level is cast
  independently and degrades to `null`, so an older Windows build yields what it has and
  `null` means "this OS cannot answer" rather than "the provider said zero".
  `fullDescription` matters most in practice: WinUI, UWP and Edge often leave `name` terse
  and put the meaningful text there.
- Three further event kinds for `wait-event`: `notification`, which carries provider
  announcements such as "File saved" in `displayString` and is frequently the only
  programmatic signal that an operation finished; plus `changes` and
  `active-text-position`.
- Transform pattern state (`canMove`, `canResize`, `canRotate`) and a `rotate` verb.
  `ITransformProvider.Rotate` had always existed and was never wired up alongside `Move`
  and `Resize`.
- A `scroll-into-view` verb backed by the ScrollItem pattern, which composes with the
  existing `realize`: one makes an off-screen item exist, the other makes it visible.
- Negative locator criteria — `--not-name`, `--not-class`, `--not-automation-id` and
  `--not-control-type` — evaluated by the provider through `CreateNotCondition` rather
  than by fetching everything and filtering. That is not only faster; filtering after the
  fact is wrong under `--max-results`, where a cap consumed by elements the caller meant
  to skip returns a truncated list that looks complete.
- Text range operations: `text --find` reports a run's offset, length and screen
  rectangles, and `select-text`, `move-caret` and `scroll-text-into-view` act on a run
  addressed by search string or by offset. Offsets are the addressing scheme because UI
  Automation ranges are live COM objects that cannot survive between two stateless calls.
- `inspect --try` (`tryInspect` over MCP), which returns `null` instead of failing when
  nothing matches. This is how a caller asserts that something is *gone* — a dialog that
  closed, a spinner that stopped — without catching an exception and reading its message.
- `--match-case` and `--find-backward` for text search, on the `text --find` read and on
  every verb addressed by a search string. The defaults are unchanged - case-insensitive
  and forward - but a caller can now tell `ERROR` from `Error`, and ask for the last
  occurrence rather than the first, which previously could not be expressed at all because
  only the first match is ever returned.
- A test project. The repository previously had none, so nothing would catch a regression
  in the COM layer, which is where essentially every defect here lives. 69 tests covering
  the DTO and JSON contract, and real UI Automation behaviour against a live desktop.

### Fixed
- A race condition in every one-shot event handler. The captured sender was touched by
  both the UI Automation callback thread and the waiting STA thread without
  synchronisation, which produced an intermittent `NullReferenceException` under a burst
  of structure-changed events. Capture is now a lock-free first-event-wins exchange.
- An event sender read after the element had been destroyed. Synchronising the capture was
  necessary but not sufficient: the sender crosses an apartment boundary and may refer to
  an element that no longer exists, particularly after a timeout where a late callback
  lands while the result is being built. An unreadable sender is now reported as no sender
  rather than failing the caller's whole wait over one field.
- `NotSupportedException` escaping text search and text range operations. Advertising the
  Text pattern does not oblige a provider to implement `FindText`, `Clone`, the endpoint
  moves, `Select` or `ScrollIntoView`; several raise `E_NOTIMPL`, which reaches managed
  code as `NotSupportedException` rather than `COMException` and surfaced as the
  unactionable "Specified method is not supported". The failure now explains that the
  provider exposes text but not range manipulation, and that reading still works.
- `TryRead` caught only `COMException`, so a dead or partially-implemented element could
  still abort an entire property projection.
- A spurious `Pattern:0` entry in `supportedPatterns`, from providers that occasionally
  report a zero pattern id.
- `headingLevel` reported the raw UI Automation constant, so every non-heading element
  carried a meaningless `80050`. It is now the ordinary 1–9, or `null` for "not a heading".
- A notification event that timed out reported `NotificationKind_ItemAdded`, because zero
  is a valid enum value. All notification fields are now `null` unless an event arrived.

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
- **Name resolution has a third tier.** When both the native UI Automation name and the
  MSAA bridge are empty, the labelling element supplies one and `nameSource` reports
  `labeledBy`. This makes previously anonymous Win32 and WinForms inputs addressable by
  name — a form of edit boxes used to inspect as identical unnamed `Edit` elements
  distinguishable only by geometry. Callers that match on an empty `name` will see
  different results.
- **`move`, `resize` and `rotate` fail earlier and more clearly.** Advertising the
  Transform pattern is not the same as permitting every operation, so each verb now checks
  the specific capability first and names the one that is false, instead of letting the
  call reach COM and return an opaque provider error.
- Two repository checks are enforced in CI rather than left to be run by hand:
  `check-com-leaks.ps1` asserts that every acquired COM proxy is released, and
  `check-cli-coverage.ps1` asserts that every action verb appears in the CLI help text,
  that CLI and MCP stay at one-to-one parity, and that no file states a tool count that
  disagrees with the code. Both replace scripts that could not do their job: one called a
  file that did not exist, the other scanned zero files and exited `0`, reporting success
  while verifying nothing. Both now fail loudly when they find nothing to scan.

- The automated post-release changelog pull request now says in its own body that it needs
  an admin merge. It is opened by `GITHUB_TOKEN`, and GitHub does not start workflow runs
  for such pushes, so its required status checks never report and it would otherwise sit
  blocked indefinitely.
- `docs/DEVELOPMENT.md` now documents how releases are cut, how to resume a partial one, and
  which secrets the pipeline needs.

### Removed
- `Directory.Build.targets`, an empty `<Project>` element imported on every build, and
  `tests/Directory.Build.props`, which only had meaning if `tests/` held project files.
- Thirteen `PackageVersion` entries that resolved to nothing. `StreamJsonRpc`,
  `Microsoft.Extensions.Resilience` and `Microsoft.Extensions.ObjectPool` were fingerprints
  of the pre-migration JSON-RPC subprocess model; the xunit stack described test projects
  that did not exist at the time. The five genuine transitive pins are kept and now carry a
  comment explaining why, and the resolved package graph is unchanged.


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


