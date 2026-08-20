namespace UIAutomationMcp.ComInterop;

/// <summary>
/// A single cell of a grid, addressed by its grid coordinates.
/// </summary>
public sealed class UiAutomationTableCell
{
    public int Row { get; init; }

    public int Column { get; init; }

    public int RowSpan { get; init; } = 1;

    public int ColumnSpan { get; init; } = 1;

    public string Name { get; init; } = string.Empty;

    public string ClassName { get; init; } = string.Empty;

    public string AutomationId { get; init; } = string.Empty;

    public int ControlType { get; init; }

    public string LocalizedControlType { get; init; } = string.Empty;

    /// <summary>
    /// Text from the cell's Value pattern when it exposes one. Cells that carry
    /// their text in <see cref="Name"/> only leave this null.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// The cell's displayed text, resolved for callers. Providers disagree on
    /// where cell content lives: Explorer's details view puts the column title in
    /// <see cref="Name"/> and the content in <see cref="Value"/>, while many WPF
    /// and web grids do the opposite. This prefers <see cref="Value"/> when it is
    /// non-empty and falls back to <see cref="Name"/>.
    /// </summary>
    public string Text => string.IsNullOrEmpty(Value) ? Name : Value;

    public bool IsOffscreen { get; init; }

    /// <summary>
    /// Set when the provider could not realize this cell, typically because the
    /// row is virtualized and has not been scrolled into view.
    /// </summary>
    public bool IsUnavailable { get; init; }
}

public sealed class UiAutomationTableRow
{
    public int Row { get; init; }

    public IReadOnlyList<UiAutomationTableCell> Cells { get; init; } = Array.Empty<UiAutomationTableCell>();
}

/// <summary>
/// A rectangular read of a control that supports the Grid pattern, plus the
/// header information the Table pattern adds on top of it.
/// </summary>
public sealed class UiAutomationTableInfo
{
    public int RowCount { get; init; }

    public int ColumnCount { get; init; }

    /// <summary>
    /// True when the control also supports the Table pattern, which is what
    /// supplies row and column headers. Grid-only controls report false.
    /// </summary>
    public bool HasTablePattern { get; init; }

    public int? RowOrColumnMajor { get; init; }

    public string? RowOrColumnMajorName { get; init; }

    public IReadOnlyList<UiAutomationElementReference> RowHeaders { get; init; } = Array.Empty<UiAutomationElementReference>();

    public IReadOnlyList<UiAutomationElementReference> ColumnHeaders { get; init; } = Array.Empty<UiAutomationElementReference>();

    public IReadOnlyList<UiAutomationTableRow> Rows { get; init; } = Array.Empty<UiAutomationTableRow>();

    /// <summary>
    /// Number of rows and columns actually returned, which may be lower than
    /// <see cref="RowCount"/> and <see cref="ColumnCount"/> when limits apply.
    /// </summary>
    public int ReturnedRowCount { get; init; }

    public int ReturnedColumnCount { get; init; }

    public bool Truncated { get; init; }
}
