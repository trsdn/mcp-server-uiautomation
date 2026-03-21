# Security Notes

UIAutomationMcp inspects live desktop UI on Windows. Treat it as a local automation tool with access to whatever applications your current user can see.

## Reporting
If you discover a vulnerability, report it privately to the project maintainer instead of opening a public issue with exploit details.

## Key considerations
- The tool can read metadata from live windows and controls visible to the current desktop session.
- Run it only in trusted environments.
- Avoid capturing or sharing sensitive UI output from business applications.
- Review logs, examples, and test artifacts before publishing them.
- Keep dependencies and packaged binaries up to date.

## Operational guidance
- Use the least-privileged Windows account that still allows the automation scenario you need.
- Close or hide unrelated sensitive applications before running broad UI inspections.
- Be careful when storing snapshots that may include application titles or control names.

