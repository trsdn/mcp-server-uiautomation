# Development Workflow

## Active solution
The active solution is `UIAutomationMcp.sln`.

## Projects
- `src\UIAutomationMcp.ComInterop`
- `src\UIAutomationMcp.Service`
- `src\UIAutomationMcp.CLI`
- `src\UIAutomationMcp.McpServer`
- `src\UIAutomationMcp.Smoke`
- `vscode-extension`

## Typical validation flow

```powershell
dotnet build UIAutomationMcp.sln
dotnet test UIAutomationMcp.sln
dotnet run --project src\UIAutomationMcp.Smoke\UIAutomationMcp.Smoke.csproj
dotnet run --project src\UIAutomationMcp.CLI\UIAutomationMcp.CLI.csproj -- desktop
```

For MCP server changes, verify the server starts cleanly and that stdout remains protocol-safe.

## Repository checks

Two scripts guard invariants the compiler cannot see. Both fail loudly if they
find nothing to inspect, so a check that verifies nothing can never report
success.

```powershell
pwsh -File scripts\check-com-leaks.ps1
pwsh -File scripts\check-cli-coverage.ps1
```

`check-com-leaks.ps1` asserts that every acquired UI Automation COM proxy is
released through `FinalRelease` or `ReleaseAll`. A leak produces no warning, no
test failure, and no symptom until a long-running MCP session has accumulated
enough cross-process references to matter. The script understands the
loop-and-advance ownership transfer used by the tree walkers, so a proxy handed
to another local and released under that name is not reported.

It is a heuristic over text, not a dataflow analysis: it cannot prove a release
is reachable on every path. It catches the common and costly case, an
acquisition with no release at all.

`check-cli-coverage.ps1` asserts that every action verb implemented in
`PerformAction` appears in the CLI help text, that the CLI and MCP surfaces stay
at one-to-one parity, and that no file states a tool count disagreeing with the
code. All three have drifted silently in the past.

For VS Code extension changes:

```powershell
Set-Location vscode-extension
npm install
npm run compile
npm run package
```

## Design expectations
- Windows UI Automation only
- typed interop through `Interop.UIAutomationClient`
- STA-safe execution for COM/UIA access
- aligned public behavior across CLI, MCP server, and extension packaging

## Releasing

Releases are cut by running the **Release All Components** workflow
(`.github/workflows/release.yml`) from the Actions tab. Pick a `version_bump`, or set
`custom_version` to name the version outright. One run builds every artifact, tags the
commit, publishes to NuGet, npm, the VS Code Marketplace and the MCP registry, creates the
GitHub release, and opens a pull request that rolls `[Unreleased]` into a version section.

That changelog pull request needs `gh pr merge <n> --squash --admin`. It is pushed by
`GITHUB_TOKEN`, and GitHub does not start workflow runs for such pushes, so its required
status checks never report and it would otherwise stay blocked.

### Resuming a partial release

Publishing is not atomic. If one registry fails, the others have already published and the
tag already exists, so a plain re-run would either be rejected or silently pick the next
version number. Re-run the workflow with:

- `resume: true`
- `custom_version: <the version that failed>` — required, because a bump would read the tag
  of the release being resumed and move past it

The tag and the GitHub release are reused, the build jobs check out the tag instead of the
default branch so the artifacts match the released commit rather than whatever has landed
since, and each publish step skips what is already live: NuGet uses `--skip-duplicate`, npm
and the Marketplace are checked against their registries first. Only the missing registries
are filled in.

### Secrets

| Secret | Used for |
| --- | --- |
| `NUGET_USER` | NuGet.org OIDC login |
| `NPM_TOKEN` | publishing `uia-mcp` |
| `VSCE_TOKEN` | publishing to the VS Code Marketplace |

Without `VSCE_TOKEN` the Marketplace step is skipped with a warning and the VSIX has to be
uploaded by hand — see `vscode-extension/MARKETPLACE-PUBLISHING.md`.

