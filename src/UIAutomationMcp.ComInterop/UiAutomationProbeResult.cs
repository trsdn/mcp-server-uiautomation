namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes a minimal probe of the UI Automation desktop root.
/// </summary>
public sealed class UiAutomationProbeResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UiAutomationProbeResult"/> class.
    /// </summary>
    public UiAutomationProbeResult(
        string coclass,
        string rootName,
        string rootClassName,
        int rootControlType,
        int rootProcessId)
    {
        Coclass = coclass;
        RootName = rootName;
        RootClassName = rootClassName;
        RootControlType = rootControlType;
        RootProcessId = rootProcessId;
    }

    /// <summary>
    /// Gets the COM coclass used to create the automation client.
    /// </summary>
    public string Coclass { get; }

    /// <summary>
    /// Gets the name reported by the desktop root element.
    /// </summary>
    public string RootName { get; }

    /// <summary>
    /// Gets the class name reported by the desktop root element.
    /// </summary>
    public string RootClassName { get; }

    /// <summary>
    /// Gets the control type identifier reported by the desktop root element.
    /// </summary>
    public int RootControlType { get; }

    /// <summary>
    /// Gets the process id reported by the desktop root element.
    /// </summary>
    public int RootProcessId { get; }
}
