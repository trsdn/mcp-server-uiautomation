namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes a supported UI Automation control pattern.
/// </summary>
public sealed class UiAutomationPatternInfo
{
    public int Id { get; init; }

    public string ProgrammaticName { get; init; } = string.Empty;
}
