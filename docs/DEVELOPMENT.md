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
dotnet run --project src\UIAutomationMcp.Smoke\UIAutomationMcp.Smoke.csproj
dotnet run --project src\UIAutomationMcp.CLI\UIAutomationMcp.CLI.csproj -- desktop
```

For MCP server changes, verify the server starts cleanly and that stdout remains protocol-safe.

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

