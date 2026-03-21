---
applyTo: "tests/**/*.cs,src/**/*.cs"
---

# Testing Strategy

Use the smallest relevant verification step:

```powershell
dotnet build UIAutomationMcp.sln
dotnet run --project src\UIAutomationMcp.Smoke\UIAutomationMcp.Smoke.csproj
dotnet run --project src\UIAutomationMcp.CLI\UIAutomationMcp.CLI.csproj -- focused
```

For VS Code extension work:

```powershell
Set-Location vscode-extension
npm run compile
npm run package
```

