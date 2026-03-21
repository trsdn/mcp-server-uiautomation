namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Captures a small UI Automation snapshot for initial diagnostics and service validation.
/// </summary>
public sealed class UiAutomationSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UiAutomationSnapshot"/> class.
    /// </summary>
    public UiAutomationSnapshot(
        string coclass,
        UiAutomationElementInfo desktop,
        UiAutomationElementInfo? focusedElement)
    {
        Coclass = coclass;
        Desktop = desktop;
        FocusedElement = focusedElement;
    }

    /// <summary>
    /// Gets the COM coclass used to create the automation client.
    /// </summary>
    public string Coclass { get; }

    /// <summary>
    /// Gets the desktop root element information.
    /// </summary>
    public UiAutomationElementInfo Desktop { get; }

    /// <summary>
    /// Gets the currently focused element information, if available.
    /// </summary>
    public UiAutomationElementInfo? FocusedElement { get; }
}
