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

    public UiAutomationDragPatternState? DragPattern { get; init; }

    public UiAutomationDropTargetPatternState? DropTargetPattern { get; init; }

    public UiAutomationLegacyAccessiblePatternState? LegacyAccessiblePattern { get; init; }

    /// <summary>
    /// The element that labels this one, typically the static text beside an input
    /// control. Without it a form of edit boxes reads as a set of anonymous
    /// <c>Edit</c> elements with no way to tell them apart but their geometry.
    /// </summary>
    public UiAutomationElementReference? LabeledBy { get; init; }

    /// <summary>
    /// Elements whose state or content this element affects - a filter combo that
    /// drives a results list names that list here.
    /// </summary>
    public IReadOnlyList<UiAutomationElementReference> ControllerFor { get; init; } = Array.Empty<UiAutomationElementReference>();

    /// <summary>Elements that describe this one, the UIA counterpart of aria-describedby.</summary>
    public IReadOnlyList<UiAutomationElementReference> DescribedBy { get; init; } = Array.Empty<UiAutomationElementReference>();

    /// <summary>Elements that follow this one in reading order, where the provider defines one.</summary>
    public IReadOnlyList<UiAutomationElementReference> FlowsTo { get; init; } = Array.Empty<UiAutomationElementReference>();

    /// <summary>Elements that precede this one in reading order. Requires IUIAutomationElement2.</summary>
    public IReadOnlyList<UiAutomationElementReference> FlowsFrom { get; init; } = Array.Empty<UiAutomationElementReference>();

    // Properties below live on IUIAutomationElement2..9 rather than the base
    // interface. Each is null when the running Windows build does not expose the
    // interface level that carries it, so a caller can tell "provider said no"
    // from "this OS cannot answer". See docs/UIAUTOMATION-COM-REFERENCE.md.

    /// <summary>
    /// The provider's full accessible description. WinUI, UWP and Edge frequently
    /// leave <see cref="Name"/> terse and put the meaningful text here.
    /// Requires IUIAutomationElement6.
    /// </summary>
    public string? FullDescription { get; init; }

    /// <summary>1-based position within its set of siblings. Requires IUIAutomationElement4.</summary>
    public int? PositionInSet { get; init; }

    /// <summary>Size of the set this element belongs to. Requires IUIAutomationElement4.</summary>
    public int? SizeOfSet { get; init; }

    /// <summary>Nesting depth within a tree or list. Requires IUIAutomationElement4.</summary>
    public int? Level { get; init; }

    /// <summary>Annotation type ids applied to the element itself. Requires IUIAutomationElement4.</summary>
    public IReadOnlyList<int>? AnnotationTypes { get; init; }

    /// <summary>Landmark type id. Requires IUIAutomationElement5.</summary>
    public int? LandmarkType { get; init; }

    /// <summary>Provider-supplied landmark name, and therefore localized. Requires IUIAutomationElement5.</summary>
    public string? LocalizedLandmarkType { get; init; }

    /// <summary>
    /// Heading level 1-9, or null when the element is not a heading. UIA reports
    /// this as HeadingLevel_None (80050) through HeadingLevel9 (80059); the raw
    /// constant is normalized here. Requires IUIAutomationElement8.
    /// </summary>
    public int? HeadingLevel { get; init; }

    /// <summary>Whether the element is a dialog. Requires IUIAutomationElement9.</summary>
    public bool? IsDialog { get; init; }

    /// <summary>Whether the element is peripheral UI such as a tooltip or flyout. Requires IUIAutomationElement3.</summary>
    public bool? IsPeripheral { get; init; }

    /// <summary>Politeness of a live region: 0 off, 1 polite, 2 assertive. Requires IUIAutomationElement2.</summary>
    public int? LiveSetting { get; init; }

    /// <summary>Readable form of <see cref="LiveSetting"/>.</summary>
    public string? LiveSettingName { get; init; }

    /// <summary>Whether the provider is optimized for visual content. Requires IUIAutomationElement2.</summary>
    public bool? OptimizeForVisualContent { get; init; }

    /// <summary>
    /// Where <see cref="Name"/> came from: <c>uia</c> for a native UI Automation name,
    /// <c>legacy</c> when the native name was empty and the MSAA bridge supplied one,
    /// <c>labeledBy</c> when both were empty and the labelling element supplied one.
    /// </summary>
    public string NameSource { get; init; } = "uia";

    /// <summary>
    /// Where <see cref="LocalizedControlType"/> came from: <c>uia</c> or <c>legacy</c>
    /// when it was filled in from the bridged MSAA role.
    /// </summary>
    public string LocalizedControlTypeSource { get; init; } = "uia";
}
