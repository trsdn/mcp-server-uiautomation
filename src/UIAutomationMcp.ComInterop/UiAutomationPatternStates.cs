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
