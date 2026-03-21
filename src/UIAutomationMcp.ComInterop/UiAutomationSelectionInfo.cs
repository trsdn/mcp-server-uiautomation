namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Represents current selection information for a selectable element.
/// </summary>
public sealed class UiAutomationSelectionInfo
{
    public bool CanSelectMultiple { get; init; }

    public bool IsSelectionRequired { get; init; }

    public int? ItemCount { get; init; }

    public UiAutomationElementInfo? CurrentSelectedItem { get; init; }

    public UiAutomationElementInfo? FirstSelectedItem { get; init; }

    public UiAutomationElementInfo? LastSelectedItem { get; init; }

    public IReadOnlyList<UiAutomationElementInfo> SelectedItems { get; init; } = Array.Empty<UiAutomationElementInfo>();
}
