---
applyTo: ".github/workflows/**/*.yml,**/*.csproj,global.json"
---

# Development Workflow

- Use feature branches and pull requests.
- Build with `dotnet build UIAutomationMcp.sln` after code changes.
- Run `npm run compile` inside `vscode-extension` for extension changes.
- Run `npm run package` when extension packaging or bundled binaries change.

