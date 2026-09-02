namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes a minimal probe of the UI Automation desktop root.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UiAutomationProbeResult"/> class.
/// </remarks>
public sealed class UiAutomationProbeResult(
    string coclass,
    string rootName,
    string rootClassName,
    int rootControlType,
    int rootProcessId)
{

    /// <summary>
    /// Gets the COM coclass used to create the automation client.
    /// </summary>
    public string Coclass { get; } = coclass;

    /// <summary>
    /// Gets the name reported by the desktop root element.
    /// </summary>
    public string RootName { get; } = rootName;

    /// <summary>
    /// Gets the class name reported by the desktop root element.
    /// </summary>
    public string RootClassName { get; } = rootClassName;

    /// <summary>
    /// Gets the control type identifier reported by the desktop root element.
    /// </summary>
    public int RootControlType { get; } = rootControlType;

    /// <summary>
    /// Gets the process id reported by the desktop root element.
    /// </summary>
    public int RootProcessId { get; } = rootProcessId;
}
