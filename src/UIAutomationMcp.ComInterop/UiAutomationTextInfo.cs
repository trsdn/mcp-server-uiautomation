namespace UIAutomationMcp.ComInterop;

/// <summary>
/// A located run of text within a text provider, addressed by offset so callers
/// can act on it in a later, independent call. UI Automation text ranges are live
/// COM objects and cannot be carried between invocations, so offsets are the
/// portable currency.
/// </summary>
public sealed class UiAutomationTextFindResult
{
    public bool Found { get; init; }

    /// <summary>The text searched for.</summary>
    public string Needle { get; init; } = string.Empty;

    /// <summary>Offset of the match from the start of the document range.</summary>
    public int? StartOffset { get; init; }

    public int? Length { get; init; }

    /// <summary>The matched text as the provider returned it, which may differ in case.</summary>
    public string? Text { get; init; }

    /// <summary>
    /// Screen rectangles covering the match - more than one when it wraps across
    /// lines. Empty when the range is off-screen.
    /// </summary>
    public IReadOnlyList<UiAutomationRect> BoundingRectangles { get; init; } = [];
}

public sealed class UiAutomationTextInfo
{
    public string Text { get; init; } = string.Empty;

    public int SupportedTextSelection { get; init; }

    public string SupportedTextSelectionName { get; init; } = string.Empty;

    public IReadOnlyList<string> SelectedTexts { get; init; } = [];

    /// <summary>The element supports TextPattern2, so caret and annotation data is available.</summary>
    public bool HasTextPattern2 { get; init; }

    /// <summary>The element supports TextEdit, so composition state is available.</summary>
    public bool HasTextEditPattern { get; init; }

    public UiAutomationTextCaretInfo? Caret { get; init; }

    public IReadOnlyList<UiAutomationTextAnnotation> Annotations { get; init; } = [];

    /// <summary>
    /// Set when the target element is an inline child of a text container rather than a
    /// text provider itself, so callers can locate it inside the surrounding text.
    /// </summary>
    public UiAutomationTextChildInfo? TextChild { get; init; }

    public UiAutomationTextEditInfo? TextEdit { get; init; }

    /// <summary>
    /// Result of a <c>--find</c> search, when one was requested. Null when no
    /// search was asked for, which is different from a search that found nothing.
    /// </summary>
    public UiAutomationTextFindResult? Find { get; init; }
}
