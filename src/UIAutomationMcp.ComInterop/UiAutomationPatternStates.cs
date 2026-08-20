namespace UIAutomationMcp.ComInterop;

public sealed class UiAutomationValuePatternState
{
    public string Value { get; init; } = string.Empty;

    public bool IsReadOnly { get; init; }
}

public sealed class UiAutomationRangeValuePatternState
{
    public double Value { get; init; }

    public bool IsReadOnly { get; init; }

    public double Minimum { get; init; }

    public double Maximum { get; init; }

    public double SmallChange { get; init; }

    public double LargeChange { get; init; }
}

public sealed class UiAutomationTogglePatternState
{
    public int ToggleState { get; init; }

    public string ToggleStateName { get; init; } = string.Empty;
}

public sealed class UiAutomationExpandCollapsePatternState
{
    public int ExpandCollapseState { get; init; }

    public string ExpandCollapseStateName { get; init; } = string.Empty;
}

public sealed class UiAutomationWindowPatternState
{
    public bool CanMaximize { get; init; }

    public bool CanMinimize { get; init; }

    public bool IsModal { get; init; }

    public bool IsTopmost { get; init; }

    public int WindowVisualState { get; init; }

    public string WindowVisualStateName { get; init; } = string.Empty;

    public int WindowInteractionState { get; init; }
}

public sealed class UiAutomationScrollPatternState
{
    public bool HorizontallyScrollable { get; init; }

    public double HorizontalScrollPercent { get; init; }

    public double HorizontalViewSize { get; init; }

    public bool VerticallyScrollable { get; init; }

    public double VerticalScrollPercent { get; init; }

    public double VerticalViewSize { get; init; }
}

public sealed class UiAutomationSelectionItemPatternState
{
    public bool IsSelected { get; init; }

    public UiAutomationElementInfo? SelectionContainer { get; init; }
}

/// <summary>
/// Describes a single view offered by a control that supports the MultipleView pattern.
/// </summary>
public sealed class UiAutomationViewInfo
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class UiAutomationMultipleViewPatternState
{
    public int CurrentView { get; init; }

    public string CurrentViewName { get; init; } = string.Empty;

    public IReadOnlyList<UiAutomationViewInfo> SupportedViews { get; init; } = Array.Empty<UiAutomationViewInfo>();
}

public sealed class UiAutomationDockPatternState
{
    public int DockPosition { get; init; }

    public string DockPositionName { get; init; } = string.Empty;
}

/// <summary>
/// Lightweight identity of an element referenced from pattern state.
/// Pattern state uses references instead of full element info so that
/// self-referencing relationships (a column header is its own column header)
/// cannot recurse, and so header lists stay small.
/// </summary>
public sealed class UiAutomationElementReference
{
    public string Name { get; init; } = string.Empty;

    public string ClassName { get; init; } = string.Empty;

    public string AutomationId { get; init; } = string.Empty;

    public int ControlType { get; init; }

    public string LocalizedControlType { get; init; } = string.Empty;

    public int[] RuntimeId { get; init; } = Array.Empty<int>();

    public UiAutomationRect? BoundingRectangle { get; init; }
}

public sealed class UiAutomationGridPatternState
{
    public int RowCount { get; init; }

    public int ColumnCount { get; init; }
}

public sealed class UiAutomationGridItemPatternState
{
    public int Row { get; init; }

    public int Column { get; init; }

    public int RowSpan { get; init; }

    public int ColumnSpan { get; init; }

    public UiAutomationElementReference? ContainingGrid { get; init; }
}

public sealed class UiAutomationTablePatternState
{
    public int RowOrColumnMajor { get; init; }

    public string RowOrColumnMajorName { get; init; } = string.Empty;

    public IReadOnlyList<UiAutomationElementReference> RowHeaders { get; init; } = Array.Empty<UiAutomationElementReference>();

    public IReadOnlyList<UiAutomationElementReference> ColumnHeaders { get; init; } = Array.Empty<UiAutomationElementReference>();
}

public sealed class UiAutomationTableItemPatternState
{
    public IReadOnlyList<UiAutomationElementReference> RowHeaderItems { get; init; } = Array.Empty<UiAutomationElementReference>();

    public IReadOnlyList<UiAutomationElementReference> ColumnHeaderItems { get; init; } = Array.Empty<UiAutomationElementReference>();
}

/// <summary>
/// Virtualization hints for an element. Present only when the element takes part
/// in virtualization, so its absence means the ordinary tree is authoritative.
/// </summary>
public sealed class UiAutomationVirtualizationInfo
{
    /// <summary>
    /// The element supports the ItemContainer pattern, so it can find items its
    /// provider knows about but has not materialized. A descendant listing of this
    /// element may therefore be incomplete.
    /// </summary>
    public bool IsItemContainer { get; init; }

    /// <summary>
    /// The element supports the VirtualizedItem pattern, so it is a placeholder that
    /// must be realized before it can be reliably read or acted on.
    /// </summary>
    public bool IsVirtualizedItem { get; init; }
}
