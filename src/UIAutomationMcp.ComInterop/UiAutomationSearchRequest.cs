namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes a multi-element UI Automation search.
/// </summary>
public sealed class UiAutomationSearchRequest
{
    public bool DesktopRoot { get; init; }

    public bool FocusedElement { get; init; }

    public long? WindowHandle { get; init; }

    public int? PointX { get; init; }

    public int? PointY { get; init; }

    public string? Name { get; init; }

    public string? ClassName { get; init; }

    public string? AutomationId { get; init; }

    public string? FrameworkId { get; init; }

    public int? ControlType { get; init; }

    public int? ProcessId { get; init; }

    public bool SearchFromFocused { get; init; }

    public string Scope { get; init; } = "subtree";

    public int MaxResults { get; init; } = 50;

    public UiAutomationCacheRequestInfo? CacheRequest { get; init; }
}
