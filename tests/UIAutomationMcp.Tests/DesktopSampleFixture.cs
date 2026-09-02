using UIAutomationMcp.ComInterop;
using UIAutomationMcp.Service;

namespace UIAutomationMcp.Tests;

/// <summary>
/// Captures one sample of the live desktop, shared by every test that needs one.
/// </summary>
/// <remarks>
/// Each broad <c>FindAll</c> over the desktop subtree is an expensive
/// cross-process walk, and running one per test was both slow and unreliable:
/// UI Automation intermittently answered a rapid series of wide scans with
/// <c>E_UNEXPECTED</c> or a timeout. Those failures said nothing about the code
/// under test - they said the suite was hammering the UIA service.
///
/// Sampling once and sharing the result removes that pressure, and has the
/// side benefit that every assertion sees the same snapshot rather than a
/// desktop that shifted underneath it mid-run.
/// </remarks>
public sealed class DesktopSampleFixture
{
    private const int SampleSize = 250;

    public DesktopSampleFixture()
    {
        if (!DesktopAvailability.IsAvailable)
        {
            return;
        }

        var service = new UiAutomationService();
        Root = Attempt(() => service.Inspect(new UiAutomationLocateRequest { DesktopRoot = true }));
        Elements = Attempt(() => service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            MaxResults = SampleSize
        })) ?? Array.Empty<UiAutomationElementInfo>();

        TextProvider = Elements.FirstOrDefault(e =>
            e.SupportedPatterns.Any(p => p.ProgrammaticName == "Text")
            && !string.IsNullOrEmpty(e.ClassName));
    }

    /// <summary>The desktop root, or null when it could not be read.</summary>
    public UiAutomationElementInfo? Root { get; }

    /// <summary>A sample of the desktop subtree. Empty when unavailable.</summary>
    public IReadOnlyList<UiAutomationElementInfo> Elements { get; } = Array.Empty<UiAutomationElementInfo>();

    /// <summary>An element exposing the Text pattern, if the desktop has one.</summary>
    public UiAutomationElementInfo? TextProvider { get; }

    /// <summary>
    /// Runs a UIA read, retrying once on a transient failure.
    /// </summary>
    /// <remarks>
    /// The retry lives here, in test setup, rather than in the production code.
    /// Swallowing E_UNEXPECTED inside the toolkit would hide genuine faults from
    /// callers; tolerating it while taking a fixture snapshot only avoids a
    /// spurious red build.
    /// </remarks>
    private static T? Attempt<T>(Func<T> read)
        where T : class
    {
        for (var i = 0; i < 2; i++)
        {
            try
            {
                return read();
            }
            catch (Exception) when (i == 0)
            {
                Thread.Sleep(500);
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }
}

/// <summary>
/// Shares one <see cref="DesktopSampleFixture"/> across every desktop test class,
/// so the desktop is walked once per run rather than once per class.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DesktopSampleGroup : ICollectionFixture<DesktopSampleFixture>
{
    public const string Name = "desktop";
}
