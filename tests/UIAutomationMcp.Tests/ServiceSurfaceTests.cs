using UIAutomationMcp.ComInterop;
using UIAutomationMcp.Service;

namespace UIAutomationMcp.Tests;

/// <summary>
/// Covers the entry points the CLI and MCP server are built on.
/// </summary>
[Collection(DesktopSampleGroup.Name)]
public sealed class ServiceSurfaceTests
{
    private readonly UiAutomationService service = new();

    [DesktopFact]
    public void ProbeDesktop_ReportsTheDesktopRootAndItsCoclass()
    {
        var probe = service.ProbeDesktop();

        // #32769 is the desktop window class and is stable across Windows versions.
        Assert.Equal("#32769", probe.RootClassName);
        Assert.False(string.IsNullOrWhiteSpace(probe.Coclass));
        Assert.True(probe.RootProcessId > 0);
    }

    [DesktopFact]
    public void CaptureSnapshot_ReturnsTheRootElement()
    {
        var snapshot = service.CaptureSnapshot();

        Assert.False(string.IsNullOrWhiteSpace(snapshot.Coclass));
        Assert.Equal("#32769", snapshot.Desktop.ClassName);
    }

    [DesktopFact]
    public void FindFirstByClassName_ResolvesTheDesktop()
    {
        var element = service.FindFirstByClassName("#32769");

        Assert.NotNull(element);
        Assert.Equal("#32769", element!.ClassName);
    }

    [DesktopFact]
    public void GetElementFromHandle_ResolvesAWindowHandle()
    {
        var root = service.Inspect(new UiAutomationLocateRequest { DesktopRoot = true });
        Assert.True(root.NativeWindowHandle != 0, "The desktop root should expose a native window handle.");

        var byHandle = service.GetElementFromHandle(new nint(root.NativeWindowHandle));

        Assert.Equal(root.ClassName, byHandle.ClassName);
    }

    [DesktopFact]
    public void Inspect_ThrowsWhenNothingMatches()
    {
        var request = new UiAutomationLocateRequest { Name = "NoSuchElement_UIAutomationMcpTests" };

        Assert.ThrowsAny<Exception>(() => service.Inspect(request));
    }

    [DesktopFact]
    public void TryInspect_ReturnsNullInsteadOfThrowing()
    {
        // The distinction Inspect/TryInspect exists so a caller can assert that
        // something is absent without catching an exception and reading its message.
        var request = new UiAutomationLocateRequest { Name = "NoSuchElement_UIAutomationMcpTests" };

        Assert.Null(service.TryInspect(request));
    }

    [DesktopFact]
    public void FindAll_RespectsMaxResults()
    {
        var results = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            MaxResults = 5
        });

        Assert.InRange(results.Count, 1, 5);
    }

    [DesktopFact]
    public void ListChildren_ReturnsDirectChildrenOfTheDesktop()
    {
        var children = service.ListChildren(
            new UiAutomationLocateRequest { DesktopRoot = true },
            view: "control",
            maxResults: 10);

        Assert.NotEmpty(children);
    }

    [DesktopTheory]
    [InlineData("raw")]
    [InlineData("control")]
    [InlineData("content")]
    public void ListChildren_AcceptsEveryTreeView(string view)
    {
        var children = service.ListChildren(
            new UiAutomationLocateRequest { DesktopRoot = true },
            view,
            maxResults: 3);

        Assert.NotNull(children);
    }

    [DesktopFact]
    public void ListChildren_RejectsAnUnknownTreeView()
    {
        Assert.ThrowsAny<ArgumentException>(() => service.ListChildren(
            new UiAutomationLocateRequest { DesktopRoot = true },
            view: "sideways",
            maxResults: 1));
    }

    [DesktopFact]
    public void Navigate_FromAChildReturnsToTheDesktop()
    {
        var children = service.ListChildren(
            new UiAutomationLocateRequest { DesktopRoot = true },
            view: "control",
            maxResults: 1);
        Assert.NotEmpty(children);

        var parent = service.Navigate(
            new UiAutomationLocateRequest { ClassName = children[0].ClassName },
            direction: "parent",
            view: "control");

        Assert.NotNull(parent);
    }

    [DesktopFact]
    public void WaitForEvent_TimesOutCleanlyWithoutAnEvent()
    {
        var result = service.WaitForEvent(new UiAutomationEventWaitRequest
        {
            EventKind = "notification",
            TimeoutMs = 400,
            Locator = new UiAutomationLocateRequest { DesktopRoot = true }
        });

        Assert.True(result.TimedOut);
        Assert.Equal("notification", result.EventKind);

        // A timeout must not fabricate a payload. NotificationKind 0 is a real
        // value (ItemAdded), so reporting the enum default here would describe a
        // notification that never arrived.
        Assert.Null(result.NotificationKind);
        Assert.Null(result.DisplayString);
    }

    [DesktopTheory]
    [InlineData("focus")]
    [InlineData("structure")]
    [InlineData("notification")]
    [InlineData("changes")]
    [InlineData("active-text-position")]
    public void WaitForEvent_RegistersEveryEventKindThatNeedsNoExtraId(string eventKind)
    {
        // Proves the IUIAutomation3/4/5/6 casts succeed on this machine: an
        // unavailable interface throws at registration rather than timing out.
        var result = service.WaitForEvent(new UiAutomationEventWaitRequest
        {
            EventKind = eventKind,
            TimeoutMs = 250,
            Locator = new UiAutomationLocateRequest { DesktopRoot = true }
        });

        Assert.Equal(eventKind, result.EventKind);
    }

    [DesktopFact]
    public void WaitForEvent_RejectsAnUnknownEventKind()
    {
        Assert.ThrowsAny<ArgumentException>(() => service.WaitForEvent(new UiAutomationEventWaitRequest
        {
            EventKind = "telepathy",
            TimeoutMs = 100,
            Locator = new UiAutomationLocateRequest { DesktopRoot = true }
        }));
    }

    [DesktopFact]
    public void PerformAction_RejectsAnUnknownVerb()
    {
        Assert.ThrowsAny<ArgumentException>(() => service.PerformAction(new UiAutomationActionRequest
        {
            Action = "levitate",
            Locator = new UiAutomationLocateRequest { DesktopRoot = true }
        }));
    }
}
