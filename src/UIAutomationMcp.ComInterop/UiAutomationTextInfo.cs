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

    /// <summary>The element supports TextPattern2, so caret and annotation data is available.</summary>
    public bool HasTextPattern2 { get; init; }

    /// <summary>The element supports TextEdit, so composition state is available.</summary>
    public bool HasTextEditPattern { get; init; }

    public UiAutomationTextCaretInfo? Caret { get; init; }

    public IReadOnlyList<UiAutomationTextAnnotation> Annotations { get; init; } = Array.Empty<UiAutomationTextAnnotation>();

    /// <summary>
    /// Set when the target element is an inline child of a text container rather than a
    /// text provider itself, so callers can locate it inside the surrounding text.
    /// </summary>
    public UiAutomationTextChildInfo? TextChild { get; init; }

    public UiAutomationTextEditInfo? TextEdit { get; init; }
}
