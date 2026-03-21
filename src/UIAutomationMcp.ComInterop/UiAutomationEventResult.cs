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

    public UiAutomationElementInfo? SourceElement { get; init; }
}
