namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Describes a UI Automation action request against a located element.
/// </summary>
public sealed class UiAutomationActionRequest
{
    public string Action { get; init; } = string.Empty;

    public UiAutomationLocateRequest Locator { get; init; } = new();

    public string? StringValue { get; init; }

    public string? SecondStringValue { get; init; }

    public double? NumberValue { get; init; }

    public double? SecondNumberValue { get; init; }

    public int? IntValue { get; init; }

    /// <summary>Search case-sensitively when a text verb is addressed by search string.</summary>
    public bool MatchCase { get; init; }

    /// <summary>Search backwards from the end of the document, so the last match wins.</summary>
    public bool SearchBackward { get; init; }
}
