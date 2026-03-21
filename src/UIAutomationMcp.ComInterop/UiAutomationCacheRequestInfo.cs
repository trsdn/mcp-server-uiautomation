namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes an optional UI Automation cache request.
/// </summary>
public sealed class UiAutomationCacheRequestInfo
{
    public bool UseCache { get; set; }

    public string Scope { get; set; } = "subtree";

    public string View { get; set; } = "control";
}
