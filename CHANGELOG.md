# Changelog

All notable changes to UIAutomationMcp are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

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


