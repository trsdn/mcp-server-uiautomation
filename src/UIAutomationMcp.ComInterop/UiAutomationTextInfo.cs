namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Represents text information read through the UI Automation text pattern.
/// </summary>
public sealed class UiAutomationTextInfo
{
    public string Text { get; init; } = string.Empty;

    public int SupportedTextSelection { get; init; }

    public string SupportedTextSelectionName { get; init; } = string.Empty;

    public IReadOnlyList<string> SelectedTexts { get; init; } = Array.Empty<string>();
}
