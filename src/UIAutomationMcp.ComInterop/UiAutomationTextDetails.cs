namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Caret position reported through TextPattern2. Offsets are character counts from the
/// start of the document range, which is the only stable coordinate UI Automation offers
/// for text.
/// </summary>
public sealed class UiAutomationTextCaretInfo
{
    /// <summary>
    /// The caret belongs to this element rather than being a stale position from a
    /// control that no longer has focus.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>Character offset from the start of the document range, or -1 when it could not be computed.</summary>
    public int Offset { get; init; }

    /// <summary>Text of the line containing the caret, which is what a caller usually wants to see.</summary>
    public string LineText { get; init; } = string.Empty;
}

/// <summary>
/// A run of text carrying one or more UI Automation annotations - a spelling squiggle,
/// a tracked change, a comment anchor, and so on.
/// </summary>
public sealed class UiAutomationTextAnnotation
{
    /// <summary>Raw <c>AnnotationType_*</c> id.</summary>
    public int TypeId { get; init; }

    /// <summary>Resolved <c>AnnotationType_*</c> name, or the numeric id when unknown.</summary>
    public string TypeName { get; init; } = string.Empty;

    public int StartOffset { get; init; }

    public int Length { get; init; }

    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// Where an inline element sits inside its containing text. This is the answer to
/// "this hyperlink/image is somewhere in the document - where exactly?".
/// </summary>
public sealed class UiAutomationTextChildInfo
{
    public UiAutomationElementReference? Container { get; init; }

    public string RangeText { get; init; } = string.Empty;

    /// <summary>Character offset of the element inside the container's document range, or -1.</summary>
    public int StartOffset { get; init; }
}

/// <summary>
/// In-flight text editing state reported by TextEdit. Non-empty during IME composition
/// and while a provider is auto-correcting, which is how you tell that typed text was
/// accepted rather than silently rewritten.
/// </summary>
public sealed class UiAutomationTextEditInfo
{
    public string ActiveComposition { get; init; } = string.Empty;

    public string ConversionTarget { get; init; } = string.Empty;
}
