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

    // Negated criteria. UI Automation has CreateNotCondition, so "every button
    // except Cancel" is expressible in the provider rather than by over-fetching
    // and filtering client-side - which is both slower, because every excluded
    // element still costs a cross-process read, and unreliable, because
    // MaxResults may be consumed entirely by elements the caller meant to skip.
    public string? NotName { get; init; }

    public string? NotClassName { get; init; }

    public string? NotAutomationId { get; init; }

    public int? NotControlType { get; init; }

    public UiAutomationCacheRequestInfo? CacheRequest { get; init; }
}
