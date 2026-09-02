namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Represents the first matching UI Automation event observed during a wait.
/// </summary>
public sealed class UiAutomationEventResult
{
    public string EventKind { get; init; } = string.Empty;

    public bool TimedOut { get; init; }

    public int? EventId { get; init; }

    /// <summary>Programmatic name of <see cref="EventId"/>, for example <c>Drag_DragStart</c>.</summary>
    public string? EventName { get; init; }

    public int? PropertyId { get; init; }

    public int? StructureChangeType { get; init; }

    public string? StructureChangeTypeName { get; init; }

    public object? Value { get; init; }

    /// <summary>Raw <c>TextEditChangeType</c> reported by a text-edit event.</summary>
    public int? TextEditChangeType { get; init; }

    public string? TextEditChangeTypeName { get; init; }

    /// <summary>
    /// Provider-supplied strings describing the change - for auto-correct this is the
    /// text that was substituted, which is how you tell that typed input was rewritten.
    /// </summary>
    public IReadOnlyList<string>? EventStrings { get; init; }

    /// <summary>Raw <c>NotificationKind</c> reported by a notification event.</summary>
    public int? NotificationKind { get; init; }

    public string? NotificationKindName { get; init; }

    /// <summary>Raw <c>NotificationProcessing</c>, describing how the announcement should be queued.</summary>
    public int? NotificationProcessing { get; init; }

    public string? NotificationProcessingName { get; init; }

    /// <summary>
    /// The text the provider wants announced - "File saved", "3 results found".
    /// This is the payload of a notification event and usually the only reason to
    /// have waited for one.
    /// </summary>
    public string? DisplayString { get; init; }

    /// <summary>Provider-chosen id correlating related notifications.</summary>
    public string? ActivityId { get; init; }

    /// <summary>Change id reported by a changes event; <c>UIA_SummaryChangeId</c> is 90000.</summary>
    public int? ChangeId { get; init; }

    /// <summary>Provider-supplied payload for the reported change, rendered as text.</summary>
    public string? ChangePayload { get; init; }

    /// <summary>
    /// How many changes the provider coalesced into this notification. Only the
    /// first is readable: the interop signature passes a single <c>ref</c> struct
    /// plus a count rather than an array.
    /// </summary>
    public int? ChangeCount { get; init; }

    /// <summary>Text of the range reported by an active-text-position event.</summary>
    public string? TextRangeText { get; init; }

    /// <summary>
    /// Document offset of that range, computed the same way the <c>text</c> command
    /// derives caret offsets, since ranges expose no offset property.
    /// </summary>
    public int? TextRangeOffset { get; init; }

    public UiAutomationElementInfo? SourceElement { get; init; }
}
