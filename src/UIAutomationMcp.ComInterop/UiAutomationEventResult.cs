namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Represents the first matching UI Automation event observed during a wait.
/// </summary>
public sealed class UiAutomationEventResult
{
    public string EventKind { get; init; } = string.Empty;

    public bool TimedOut { get; init; }

    public int? EventId { get; init; }

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

    public UiAutomationElementInfo? SourceElement { get; init; }
}
