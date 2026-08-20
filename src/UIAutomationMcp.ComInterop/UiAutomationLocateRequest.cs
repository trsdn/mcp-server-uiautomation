namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes how a single UI Automation element should be resolved.
/// </summary>
public sealed class UiAutomationLocateRequest
{
    public bool DesktopRoot { get; set; }

    public bool FocusedElement { get; set; }

    public long? WindowHandle { get; set; }

    public int? PointX { get; set; }

    public int? PointY { get; set; }

    public string? Name { get; set; }

    public string? ClassName { get; set; }

    public string? AutomationId { get; set; }

    public string? FrameworkId { get; set; }

    public int? ControlType { get; set; }

    public int? ProcessId { get; set; }

    public bool SearchFromFocused { get; set; }

    public string Scope { get; set; } = "subtree";

    /// <summary>
    /// When a selector finds nothing in the live tree, ask any ItemContainer provider
    /// in scope whether it knows the item and realize it. Only runs on the failure
    /// path, so it costs nothing for searches that already succeed. Defaults to true.
    /// </summary>
    public bool RealizeVirtualized { get; set; } = true;

    public UiAutomationCacheRequestInfo? CacheRequest { get; set; }
}
