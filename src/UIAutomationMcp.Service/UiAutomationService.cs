using UIAutomationMcp.ComInterop;

namespace UIAutomationMcp.Service;

/// <summary>
/// Exposes UI Automation queries for the CLI and MCP server entry points.
/// </summary>
public sealed class UiAutomationService
{
    public UiAutomationProbeResult ProbeDesktop() => UiAutomationBootstrap.ProbeDesktop();

    public UiAutomationSnapshot CaptureSnapshot() => UiAutomationBootstrap.CaptureSnapshot();

    public UiAutomationElementInfo? GetFocusedElement() => UiAutomationBootstrap.GetFocusedElement();

    public UiAutomationElementInfo GetElementFromHandle(nint handle) => UiAutomationBootstrap.GetElementFromHandle(handle);

    public UiAutomationElementInfo GetElementFromPoint(int x, int y) => UiAutomationBootstrap.GetElementFromPoint(x, y);

    public UiAutomationElementInfo? FindFirstByName(string name) => UiAutomationBootstrap.FindFirstDescendantByName(name);

    public UiAutomationElementInfo? FindFirstByClassName(string className) => UiAutomationBootstrap.FindFirstDescendantByClassName(className);

    public UiAutomationElementInfo? FindFirstByAutomationId(string automationId) => UiAutomationBootstrap.FindFirstDescendantByAutomationId(automationId);

    public UiAutomationElementInfo Inspect(UiAutomationLocateRequest request) => UiAutomationBootstrap.InspectElement(request);

    public UiAutomationElementInfo? TryInspect(UiAutomationLocateRequest request) => UiAutomationBootstrap.TryInspect(request);

    public IReadOnlyList<UiAutomationElementInfo> FindAll(UiAutomationSearchRequest request) => UiAutomationBootstrap.FindAll(request);

    public IReadOnlyList<UiAutomationElementInfo> ListChildren(UiAutomationLocateRequest locator, string view = "control", int maxResults = 50) =>
        UiAutomationBootstrap.ListChildren(locator, view, maxResults);

    public IReadOnlyList<UiAutomationElementInfo> ListDescendants(UiAutomationLocateRequest locator, string view = "control", int maxResults = 50) =>
        UiAutomationBootstrap.ListDescendants(locator, view, maxResults);

    public UiAutomationElementInfo? Navigate(UiAutomationLocateRequest locator, string direction, string view = "control") =>
        UiAutomationBootstrap.Navigate(locator, direction, view);

    public UiAutomationTextInfo? ReadText(UiAutomationLocateRequest locator) => UiAutomationBootstrap.ReadText(locator);

    public UiAutomationSelectionInfo? ReadSelection(UiAutomationLocateRequest locator) => UiAutomationBootstrap.ReadSelection(locator);

    public UiAutomationAudioResult GetSystemAudioState() => UiAutomationBootstrap.GetSystemAudioState();

    public UiAutomationAudioResult SetSystemAudioMute(bool muted) => UiAutomationBootstrap.SetSystemAudioMute(muted);

    public UiAutomationAudioResult ToggleSystemAudioMute() => UiAutomationBootstrap.ToggleSystemAudioMute();

    public UiAutomationActionResult PerformAction(UiAutomationActionRequest request) => UiAutomationBootstrap.PerformAction(request);

    public UiAutomationEventResult WaitForEvent(UiAutomationEventWaitRequest request) => UiAutomationBootstrap.WaitForEvent(request);
}
