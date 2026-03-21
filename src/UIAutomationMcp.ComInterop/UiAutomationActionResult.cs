namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Represents the outcome of a UI Automation action.
/// </summary>
public sealed class UiAutomationActionResult
{
    public string Action { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public UiAutomationElementInfo? Element { get; init; }
}
