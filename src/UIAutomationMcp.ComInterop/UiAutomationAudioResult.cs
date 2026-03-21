namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Represents the current system audio mute state.
/// </summary>
public sealed class UiAutomationAudioResult
{
    public string Action { get; init; } = string.Empty;

    public bool Muted { get; init; }

    public string Message { get; init; } = string.Empty;
}
