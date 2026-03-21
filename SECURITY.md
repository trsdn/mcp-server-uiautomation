# Security Policy

## Supported Versions

We actively support the latest `UIAutomationMcp` release line with security updates.

| Version | Supported | Status |
| ------- | --------- | ------ |
| 1.x     | :white_check_mark: | Active |
| < 1.0   | :x: | Unsupported |

## Security Characteristics

`UIAutomationMcp` is a Windows-only local automation tool for inspecting and interacting with live desktop UI through Windows UI Automation.

Key security boundaries:

- **Local only**: the active product surfaces are the local CLI, the local MCP server, and the local VS Code extension.
- **Current-user context**: all UI Automation operations run with the permissions of the current Windows user.
- **No privilege elevation**: the tool does not bypass Windows security boundaries or elevate rights.
- **Live UI access**: the tool can inspect on-screen UI metadata and, when requested, perform supported UI actions against desktop apps visible to the current user session.

## Security Features

### Code quality and dependency hygiene

- Central package management across .NET projects
- Automated dependency updates and vulnerability surfacing
- Warnings treated seriously in build validation
- CI build checks for the active shipped surfaces

### UI Automation runtime safety

- UI Automation COM access is executed on STA threads
- COM objects are explicitly released to avoid stale automation handles
- MCP stdio traffic is kept clean by routing diagnostics away from protocol output
- Queries and actions are scoped to the current desktop session

### MCP server behavior

- The MCP server is intended for local assistant integrations
- Stdio is the primary transport and does not expose a network listener by default
- Tool execution only has the same local desktop visibility and action authority as the signed-in user

## Security Considerations for Users

### Live desktop visibility

UI Automation can expose information about:

- window titles
- control names and automation IDs
- focused elements
- selected text or values when the target control supports it

Only use the tool on machines and sessions where that level of UI inspection is acceptable.

### Action execution

Some UI Automation operations can change local application state, for example:

- setting focus
- invoking controls
- changing values
- toggling selection or expand/collapse state
- moving or resizing supported windows or controls

Review assistant-issued actions before running them on sensitive applications.

### Sensitive applications

Be careful when targeting:

- password managers
- authentication prompts
- internal admin tools
- regulated or privacy-sensitive apps

Even if a target control does not expose secret values through UI Automation, metadata and window structure can still be sensitive.

## Reporting a Vulnerability

Please do not open a public issue for a suspected security vulnerability.

Preferred reporting path:

1. Open a GitHub Security Advisory draft for this repository:
   - <https://github.com/trsdn/mcp-server-uiautomation/security/advisories>
2. Include:
   - a clear description
   - impact
   - affected versions
   - reproduction steps or proof of concept
   - a suggested mitigation if available

Alternative contact:

- GitHub: [@trsdn](https://github.com/trsdn)

## What to Expect

- acknowledgment after review
- follow-up questions if reproduction details are needed
- a private fix when the issue is confirmed
- coordinated disclosure through a security advisory when appropriate

## User Best Practices

- run the tool only in trusted local sessions
- review automation requests before execution
- avoid exposing sensitive desktop sessions to untrusted assistants
- keep Windows, .NET, and the repository dependencies up to date
- restrict use on shared machines to trusted users

## Security Updates

Security updates are published through:

- GitHub Security Advisories: <https://github.com/trsdn/mcp-server-uiautomation/security/advisories>
- Release notes: <https://github.com/trsdn/mcp-server-uiautomation/releases>

Thank you for helping keep `UIAutomationMcp` and its users safe.
