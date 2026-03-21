namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Represents a UI Automation bounding rectangle.
/// </summary>
public sealed class UiAutomationRect
{
    public int Left { get; init; }

    public int Top { get; init; }

    public int Right { get; init; }

    public int Bottom { get; init; }

    public int Width => Math.Max(0, Right - Left);

    public int Height => Math.Max(0, Bottom - Top);
}
