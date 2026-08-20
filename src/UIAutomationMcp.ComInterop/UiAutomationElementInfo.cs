namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes a UI Automation element using a stable snapshot of commonly needed properties.
/// </summary>
public sealed class UiAutomationElementInfo
{
    public string Name { get; init; } = string.Empty;

    public string ClassName { get; init; } = string.Empty;

    public int ControlType { get; init; }

    public string LocalizedControlType { get; init; } = string.Empty;

    public int ProcessId { get; init; }

    public string AutomationId { get; init; } = string.Empty;

    public string FrameworkId { get; init; } = string.Empty;

    public UiAutomationRect BoundingRectangle { get; init; } = new();

    public string AcceleratorKey { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string AriaProperties { get; init; } = string.Empty;

    public string AriaRole { get; init; } = string.Empty;

    public int Culture { get; init; }

    public bool HasKeyboardFocus { get; init; }

    public string HelpText { get; init; } = string.Empty;

    public bool IsContentElement { get; init; }

    public bool IsControlElement { get; init; }

    public bool IsDataValidForForm { get; init; }

    public bool IsEnabled { get; init; }

    public bool IsKeyboardFocusable { get; init; }

    public bool IsOffscreen { get; init; }

    public bool IsPassword { get; init; }

    public bool IsRequiredForForm { get; init; }

    public string ItemStatus { get; init; } = string.Empty;

    public string ItemType { get; init; } = string.Empty;

    public long NativeWindowHandle { get; init; }

    public int Orientation { get; init; }

    public string OrientationName { get; init; } = string.Empty;

    public string ProviderDescription { get; init; } = string.Empty;

    public IReadOnlyList<int> RuntimeId { get; init; } = Array.Empty<int>();

    public IReadOnlyList<UiAutomationPatternInfo> SupportedPatterns { get; init; } = Array.Empty<UiAutomationPatternInfo>();

    public UiAutomationValuePatternState? ValuePattern { get; init; }

    public UiAutomationRangeValuePatternState? RangeValuePattern { get; init; }

    public UiAutomationTogglePatternState? TogglePattern { get; init; }

    public UiAutomationExpandCollapsePatternState? ExpandCollapsePattern { get; init; }

    public UiAutomationWindowPatternState? WindowPattern { get; init; }

    public UiAutomationScrollPatternState? ScrollPattern { get; init; }

    public UiAutomationSelectionItemPatternState? SelectionItemPattern { get; init; }

    public UiAutomationMultipleViewPatternState? MultipleViewPattern { get; init; }

    public UiAutomationDockPatternState? DockPattern { get; init; }

    public UiAutomationGridPatternState? GridPattern { get; init; }

    public UiAutomationGridItemPatternState? GridItemPattern { get; init; }

    public UiAutomationTablePatternState? TablePattern { get; init; }

    public UiAutomationTableItemPatternState? TableItemPattern { get; init; }

    public UiAutomationVirtualizationInfo? Virtualization { get; init; }
}
