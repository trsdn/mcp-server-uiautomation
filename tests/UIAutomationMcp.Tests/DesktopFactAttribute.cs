using UIAutomationMcp.ComInterop;

namespace UIAutomationMcp.Tests;

/// <summary>
/// Marks a test that needs a live UI Automation desktop.
/// </summary>
/// <remarks>
/// These are not "integration tests" in the sense of being optional extras. UI
/// Automation is the entire subject of this project, and almost every defect
/// worth catching here lives in the COM boundary rather than in pure logic, so a
/// suite that never touched a real provider would verify very little.
///
/// GitHub Actions <c>windows-latest</c> does provide a usable desktop - the smoke
/// executable already performs real COM activation there on every pull request -
/// so these run in CI rather than being skipped.
///
/// They skip rather than fail where no desktop exists (a headless session, or a
/// non-Windows machine), because that is an environment limitation and not a
/// defect in the code under test.
/// </remarks>
public sealed class DesktopFactAttribute : FactAttribute
{
    public DesktopFactAttribute()
    {
        if (!DesktopAvailability.IsAvailable)
        {
            Skip = DesktopAvailability.SkipReason;
        }
    }
}

/// <summary>Theory counterpart of <see cref="DesktopFactAttribute"/>.</summary>
public sealed class DesktopTheoryAttribute : TheoryAttribute
{
    public DesktopTheoryAttribute()
    {
        if (!DesktopAvailability.IsAvailable)
        {
            Skip = DesktopAvailability.SkipReason;
        }
    }
}

internal static class DesktopAvailability
{
    private static readonly Lazy<(bool Available, string Reason)> Probe = new(() =>
    {
        if (!OperatingSystem.IsWindows())
        {
            return (false, "UI Automation is Windows-only.");
        }

        try
        {
            // The cheapest call that proves COM activation and a reachable desktop
            // root, which is exactly what every other desktop test depends on.
            var probe = UiAutomationBootstrap.ProbeDesktop();
            return string.IsNullOrEmpty(probe.RootClassName)
                ? (false, "UI Automation returned no desktop root; no interactive session.")
                : (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"UI Automation is unavailable: {ex.GetType().Name}: {ex.Message}");
        }
    });

    public static bool IsAvailable => Probe.Value.Available;

    public static string SkipReason => Probe.Value.Reason;
}
