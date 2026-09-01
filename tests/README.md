# UIAutomationMcp Validation

## Automated tests

```powershell
dotnet test UIAutomationMcp.sln
```

62 tests in `UIAutomationMcp.Tests`, run in CI by
[the smoke workflow](../.github/workflows/integration-tests.yml).

They fall into two groups.

**Contract tests** (`ContractTests`) need no desktop. They cover the DTO
defaults and the JSON shape the CLI emits — the boundary downstream consumers
actually depend on, where a rename or a null-handling change breaks callers
silently.

**Desktop tests** (`ServiceSurfaceTests`, `ElementProjectionTests`,
`LocatorTests`, `TextTests`) drive real UI Automation. That is deliberate: UIA
is the entire subject of this project, and almost every defect worth catching
lives at the COM boundary rather than in pure logic. A suite that never touched
a provider would verify very little.

They are marked `[DesktopFact]` / `[DesktopTheory]` and **skip** rather than
fail where no desktop exists, since that is an environment limitation rather
than a defect. GitHub Actions `windows-latest` does provide one, so they run in
CI.

## What the desktop tests can and cannot promise

They assert invariants that must hold for *any* provider — offsets are
non-negative, `nameSource` is one of three known values, `headingLevel` is
normalized to 1–9 or null, a pattern id is never zero. They deliberately do not
assert exact strings or exact counts, because the desktop differs between
machines and between runs.

Two consequences worth knowing:

- **They read one shared desktop sample.** Walking the tree per test was both
  slow and unreliable: UI Automation intermittently answered a rapid series of
  wide scans with `E_UNEXPECTED` or a timeout, which said nothing about the code
  under test. `DesktopSampleFixture` takes one sample per run and shares it.
- **Treat a repeated failure as a real signal.** An earlier version of this file
  described roughly one run in seven failing as an environmental fact of testing
  against a live desktop. That was wrong: most of it was a genuine bug — an event
  sender read across an apartment boundary after the element had been destroyed.
  Fixing it produced eight consecutive clean runs of the previously-flaky tests.
  If a test fails repeatedly, assume the code is at fault before the environment.

## What these tests found

They are not decoration. Writing them surfaced six real defects in code that
had already been manually verified and shipped:

1. **A race condition in every event handler.** The captured sender was touched
   by both the UI Automation callback thread and the waiting STA thread without
   synchronisation, producing an intermittent `NullReferenceException` under a
   burst of structure-changed events. Now a lock-free first-event-wins exchange.
2. **An event sender read after the element had died.** Synchronising the
   capture was necessary but not sufficient: the sender crosses an apartment
   boundary and may refer to an element that no longer exists, especially after
   a timeout. An unreadable sender is now reported as no sender rather than
   failing the whole wait.
3. **`NotSupportedException` escaping text search.** Advertising the Text
   pattern does not oblige a provider to implement `FindText`; several raise
   `E_NOTIMPL`, which surfaces as `NotSupportedException` rather than
   `COMException` and took the whole text read down.
4. **The same defect on the offset path.** Every earlier text check used
   Notepad, which implements the entire Text pattern; providers that expose text
   but not range manipulation leaked the unactionable "Specified method is not
   supported" from `select-text`, `move-caret` and `scroll-text-into-view`.
5. **`TryRead` catching too narrowly.** It caught only `COMException`, so a dead
   or partially-implemented element could still abort a projection.
6. **A spurious `Pattern:0` entry.** Providers occasionally report a zero
   pattern id, which is not a pattern.

Defects 2 and 4 are worth dwelling on: neither could have been found by manual
testing, because both depend on a provider or a timing window that a developer
driving Notepad by hand will never hit.

## Manual validation

The automated suite does not cover everything. Runtime behaviour against
specific applications still needs a person:

```powershell
dotnet build UIAutomationMcp.sln -c Release
.\src\UIAutomationMcp.Smoke\bin\Release\net9.0-windows\UIAutomationMcp.Smoke.exe
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe desktop
.\src\UIAutomationMcp.CLI\bin\Release\net9.0-windows\uiamcp.exe focused
```

Provider-specific checks worth running after COM changes: Explorer for
virtualization and tables, Notepad for text and caret, Character Map for the
MSAA bridge, Edge for TextChild and ARIA.

One thing remains unverified end to end and is documented as such in
[the COM reference](../docs/UIAUTOMATION-COM-REFERENCE.md): the receive path for
`changes` and `active-text-position` events. That is structural rather than an
oversight — WPF's `AutomationPeer` cannot raise either, and the native exports
that can require a hosted provider whose harness would be larger than the code
under test. If you encounter an application that raises one, that observation is
worth more than another attempt at synthesising it.

`action rotate` is no longer among them. WPF's built-in automation peers all
report `canRotate: false`, so the success path was closed with a purpose-built
peer implementing `ITransformProvider`: two successive rotations moved the
provider's own state from 45 to 135 degrees, read back through UI Automation
rather than trusted from the return value.

## Repository checks

```powershell
pwsh -File scripts\check-com-leaks.ps1
pwsh -File scripts\check-cli-coverage.ps1
```

These guard invariants the compiler cannot see — COM proxy release, and
CLI/MCP parity. See [the development guide](../docs/DEVELOPMENT.md).

## Environment assumptions

- Windows desktop session
- `Interop.UIAutomationClient` available at build and run time
- an interactive desktop, so the root and focused element resolve
