namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes a one-shot wait for a UI Automation event.
/// </summary>
public sealed class UiAutomationEventWaitRequest
{
    public string EventKind { get; init; } = "focus";

    public UiAutomationLocateRequest Locator { get; init; } = new();

    public UiAutomationCacheRequestInfo? CacheRequest { get; init; }

    public int TimeoutMs { get; init; } = 5000;

    public int? EventId { get; init; }

    public int? PropertyId { get; init; }

    /// <summary>
    /// Change type for a <c>changes</c> event. Defaults to
    /// <c>UIA_SummaryChangeId</c> (90000), the only change id UI Automation defines.
    /// </summary>
    public int? ChangeId { get; init; }
}
