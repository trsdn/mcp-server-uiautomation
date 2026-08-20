using System.Globalization;
using System.Runtime.InteropServices;
using Interop.UIAutomationClient;

namespace UIAutomationMcp.ComInterop;

/// <summary>
/// Provides typed access to the Windows UI Automation COM coclasses.
/// </summary>
public static class UiAutomationBootstrap
{
    private static readonly Dictionary<int, string> PatternNames = new()
    {
        [UIA_PatternIds.UIA_InvokePatternId] = "Invoke",
        [UIA_PatternIds.UIA_SelectionPatternId] = "Selection",
        [UIA_PatternIds.UIA_ValuePatternId] = "Value",
        [UIA_PatternIds.UIA_RangeValuePatternId] = "RangeValue",
        [UIA_PatternIds.UIA_ScrollPatternId] = "Scroll",
        [UIA_PatternIds.UIA_ExpandCollapsePatternId] = "ExpandCollapse",
        [UIA_PatternIds.UIA_GridPatternId] = "Grid",
        [UIA_PatternIds.UIA_GridItemPatternId] = "GridItem",
        [UIA_PatternIds.UIA_MultipleViewPatternId] = "MultipleView",
        [UIA_PatternIds.UIA_WindowPatternId] = "Window",
        [UIA_PatternIds.UIA_SelectionItemPatternId] = "SelectionItem",
        [UIA_PatternIds.UIA_DockPatternId] = "Dock",
        [UIA_PatternIds.UIA_TablePatternId] = "Table",
        [UIA_PatternIds.UIA_TableItemPatternId] = "TableItem",
        [UIA_PatternIds.UIA_TextPatternId] = "Text",
        [UIA_PatternIds.UIA_TogglePatternId] = "Toggle",
        [UIA_PatternIds.UIA_TransformPatternId] = "Transform",
        [UIA_PatternIds.UIA_ScrollItemPatternId] = "ScrollItem",
        [UIA_PatternIds.UIA_LegacyIAccessiblePatternId] = "LegacyIAccessible",
        [UIA_PatternIds.UIA_ItemContainerPatternId] = "ItemContainer",
        [UIA_PatternIds.UIA_VirtualizedItemPatternId] = "VirtualizedItem",
        [UIA_PatternIds.UIA_SynchronizedInputPatternId] = "SynchronizedInput",
        [UIA_PatternIds.UIA_ObjectModelPatternId] = "ObjectModel",
        [UIA_PatternIds.UIA_AnnotationPatternId] = "Annotation",
        [UIA_PatternIds.UIA_TextPattern2Id] = "Text2",
        [UIA_PatternIds.UIA_StylesPatternId] = "Styles",
        [UIA_PatternIds.UIA_SpreadsheetPatternId] = "Spreadsheet",
        [UIA_PatternIds.UIA_SpreadsheetItemPatternId] = "SpreadsheetItem",
        [UIA_PatternIds.UIA_TransformPattern2Id] = "Transform2",
        [UIA_PatternIds.UIA_TextChildPatternId] = "TextChild",
        [UIA_PatternIds.UIA_DragPatternId] = "Drag",
        [UIA_PatternIds.UIA_DropTargetPatternId] = "DropTarget",
        [UIA_PatternIds.UIA_TextEditPatternId] = "TextEdit",
        [UIA_PatternIds.UIA_CustomNavigationPatternId] = "CustomNavigation",
        [UIA_PatternIds.UIA_SelectionPattern2Id] = "Selection2"
    };

    public static UiAutomationProbeResult ProbeDesktop() => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? root = null;

        try
        {
            root = automation.GetRootElement();
            var info = ReadElementInfo(automation, root);
            return new UiAutomationProbeResult(
                coclass: automation.GetType().FullName ?? automation.GetType().Name,
                rootName: info.Name,
                rootClassName: info.ClassName,
                rootControlType: info.ControlType,
                rootProcessId: info.ProcessId);
        }
        finally
        {
            FinalRelease(root);
            FinalRelease(automation);
        }
    });

    public static UiAutomationSnapshot CaptureSnapshot() => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? root = null;
        IUIAutomationElement? focused = null;

        try
        {
            root = automation.GetRootElement();
            focused = TryGetFocusedElement(automation);

            return new UiAutomationSnapshot(
                coclass: automation.GetType().FullName ?? automation.GetType().Name,
                desktop: ReadElementInfo(automation, root),
                focusedElement: focused is null ? null : ReadElementInfo(automation, focused));
        }
        finally
        {
            FinalRelease(focused);
            FinalRelease(root);
            FinalRelease(automation);
        }
    });

    public static UiAutomationElementInfo? GetFocusedElement() => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? focused = null;

        try
        {
            focused = TryGetFocusedElement(automation);
            return focused is null ? null : ReadElementInfo(automation, focused);
        }
        finally
        {
            FinalRelease(focused);
            FinalRelease(automation);
        }
    });

    public static UiAutomationElementInfo GetElementFromHandle(nint windowHandle) =>
        InspectElement(new UiAutomationLocateRequest { WindowHandle = windowHandle.ToInt64() });

    public static UiAutomationElementInfo GetElementFromPoint(int x, int y) =>
        InspectElement(new UiAutomationLocateRequest { PointX = x, PointY = y });

    public static UiAutomationElementInfo? FindFirstDescendantByName(string name) =>
        TryInspect(new UiAutomationLocateRequest { Name = name });

    public static UiAutomationElementInfo? FindFirstDescendantByClassName(string className) =>
        TryInspect(new UiAutomationLocateRequest { ClassName = className });

    public static UiAutomationElementInfo? FindFirstDescendantByAutomationId(string automationId) =>
        TryInspect(new UiAutomationLocateRequest { AutomationId = automationId });

    public static UiAutomationElementInfo InspectElement(UiAutomationLocateRequest request) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? element = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, request.CacheRequest);
            element = ResolveElement(automation, request, throwIfNotFound: true, cacheRequest);
            return ReadElementInfo(automation, element!);
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(element);
            FinalRelease(automation);
        }
    });

    public static UiAutomationElementInfo? TryInspect(UiAutomationLocateRequest request) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? element = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, request.CacheRequest);
            element = ResolveElement(automation, request, throwIfNotFound: false, cacheRequest);
            return element is null ? null : ReadElementInfo(automation, element);
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(element);
            FinalRelease(automation);
        }
    });

    public static IReadOnlyList<UiAutomationElementInfo> FindAll(UiAutomationSearchRequest request) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? origin = null;
        IUIAutomationCondition? filter = null;
        IUIAutomationElementArray? matches = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, request.CacheRequest);
            origin = ResolveOrigin(automation, request, cacheRequest);
            filter = BuildFilterCondition(automation, request);
            matches = cacheRequest is null
                ? origin.FindAll(ParseTreeScope(request.Scope), filter)
                : origin.FindAllBuildCache(ParseTreeScope(request.Scope), filter, cacheRequest);

            var count = matches.Length;
            var results = new List<UiAutomationElementInfo>(Math.Min(count, Math.Max(1, request.MaxResults)));
            var max = Math.Min(count, Math.Max(1, request.MaxResults));

            for (var index = 0; index < max; index++)
            {
                IUIAutomationElement? element = null;
                try
                {
                    element = matches.GetElement(index);
                    results.Add(ReadElementInfo(automation, element));
                }
                finally
                {
                    FinalRelease(element);
                }
            }

            return results;
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(matches);
            FinalRelease(filter);
            FinalRelease(origin);
            FinalRelease(automation);
        }
    });

    public static IReadOnlyList<UiAutomationElementInfo> ListChildren(UiAutomationLocateRequest locator, string view = "control", int maxResults = 50) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? origin = null;
        IUIAutomationTreeWalker? walker = null;
        IUIAutomationElement? current = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, locator.CacheRequest);
            origin = ResolveElement(automation, locator, throwIfNotFound: true, cacheRequest);
            walker = CreateWalker(automation, view);
            current = cacheRequest is null
                ? walker.GetFirstChildElement(origin!)
                : walker.GetFirstChildElementBuildCache(origin!, cacheRequest);

            var results = new List<UiAutomationElementInfo>(Math.Max(1, maxResults));
            var limit = Math.Max(1, maxResults);
            while (current is not null && results.Count < limit)
            {
                IUIAutomationElement? next = null;
                try
                {
                    results.Add(ReadElementInfo(automation, current));
                    next = cacheRequest is null
                        ? walker.GetNextSiblingElement(current)
                        : walker.GetNextSiblingElementBuildCache(current, cacheRequest);
                }
                finally
                {
                    FinalRelease(current);
                }

                current = next;
            }

            return results;
        }
        finally
        {
            FinalRelease(current);
            FinalRelease(cacheRequest);
            FinalRelease(walker);
            FinalRelease(origin);
            FinalRelease(automation);
        }
    });

    public static IReadOnlyList<UiAutomationElementInfo> ListDescendants(UiAutomationLocateRequest locator, string view = "control", int maxResults = 50) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? origin = null;
        IUIAutomationTreeWalker? walker = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, locator.CacheRequest);
            origin = ResolveElement(automation, locator, throwIfNotFound: true, cacheRequest);
            walker = CreateWalker(automation, view);

            var results = new List<UiAutomationElementInfo>(Math.Max(1, maxResults));
            var limit = Math.Max(1, maxResults);

            VisitDescendants(origin!, walker, cacheRequest, automation, results, limit);
            return results;
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(walker);
            FinalRelease(origin);
            FinalRelease(automation);
        }
    });

    public static UiAutomationElementInfo? Navigate(UiAutomationLocateRequest locator, string direction, string view = "control") => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? origin = null;
        IUIAutomationTreeWalker? walker = null;
        IUIAutomationElement? target = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, locator.CacheRequest);
            origin = ResolveElement(automation, locator, throwIfNotFound: true, cacheRequest);
            walker = CreateWalker(automation, view);
            var normalizedDirection = direction.Trim().ToLowerInvariant();
            target = cacheRequest is null
                ? normalizedDirection switch
                {
                    "parent" => walker.GetParentElement(origin!),
                    "first-child" => walker.GetFirstChildElement(origin!),
                    "last-child" => walker.GetLastChildElement(origin!),
                    "next-sibling" => walker.GetNextSiblingElement(origin!),
                    "previous-sibling" => walker.GetPreviousSiblingElement(origin!),
                    "normalize" => walker.NormalizeElement(origin!),
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported navigation direction.")
                }
                : normalizedDirection switch
                {
                    "parent" => walker.GetParentElementBuildCache(origin!, cacheRequest),
                    "first-child" => walker.GetFirstChildElementBuildCache(origin!, cacheRequest),
                    "last-child" => walker.GetLastChildElementBuildCache(origin!, cacheRequest),
                    "next-sibling" => walker.GetNextSiblingElementBuildCache(origin!, cacheRequest),
                    "previous-sibling" => walker.GetPreviousSiblingElementBuildCache(origin!, cacheRequest),
                    "normalize" => walker.NormalizeElementBuildCache(origin!, cacheRequest),
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported navigation direction.")
                };

            return target is null ? null : ReadElementInfo(automation, target);
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(target);
            FinalRelease(walker);
            FinalRelease(origin);
            FinalRelease(automation);
        }
    });

    public static UiAutomationTextInfo? ReadText(UiAutomationLocateRequest locator) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? element = null;
        IUIAutomationTextPattern? textPattern = null;
        IUIAutomationTextRange? documentRange = null;
        IUIAutomationTextRangeArray? selections = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, locator.CacheRequest);
            element = ResolveElement(automation, locator, throwIfNotFound: true, cacheRequest);
            textPattern = GetPattern<IUIAutomationTextPattern>(element!, UIA_PatternIds.UIA_TextPatternId);
            if (textPattern is null)
            {
                return null;
            }

            documentRange = textPattern.DocumentRange;
            selections = textPattern.GetSelection();

            var selectedTexts = new List<string>();
            if (selections is not null)
            {
                for (var index = 0; index < selections.Length; index++)
                {
                    IUIAutomationTextRange? range = null;
                    try
                    {
                        range = selections.GetElement(index);
                        selectedTexts.Add(range?.GetText(-1) ?? string.Empty);
                    }
                    finally
                    {
                        FinalRelease(range);
                    }
                }
            }

            return new UiAutomationTextInfo
            {
                Text = documentRange?.GetText(-1) ?? string.Empty,
                SupportedTextSelection = (int)textPattern.SupportedTextSelection,
                SupportedTextSelectionName = textPattern.SupportedTextSelection.ToString(),
                SelectedTexts = selectedTexts
            };
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(selections);
            FinalRelease(documentRange);
            FinalRelease(textPattern);
            FinalRelease(element);
            FinalRelease(automation);
        }
    });

    public static UiAutomationSelectionInfo? ReadSelection(UiAutomationLocateRequest locator) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? element = null;
        IUIAutomationSelectionPattern? selectionPattern = null;
        IUIAutomationSelectionPattern2? selectionPattern2 = null;
        IUIAutomationElementArray? currentSelection = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, locator.CacheRequest);
            element = ResolveElement(automation, locator, throwIfNotFound: true, cacheRequest);
            selectionPattern = GetPattern<IUIAutomationSelectionPattern>(element!, UIA_PatternIds.UIA_SelectionPatternId);
            if (selectionPattern is null)
            {
                return null;
            }

            selectionPattern2 = GetPattern<IUIAutomationSelectionPattern2>(element!, UIA_PatternIds.UIA_SelectionPattern2Id);
            currentSelection = selectionPattern.GetCurrentSelection();
            var selectedItems = ReadElementArray(automation, currentSelection);

            return new UiAutomationSelectionInfo
            {
                CanSelectMultiple = selectionPattern.CurrentCanSelectMultiple != 0,
                IsSelectionRequired = selectionPattern.CurrentIsSelectionRequired != 0,
                ItemCount = selectionPattern2 is null ? null : selectionPattern2.CurrentItemCount,
                CurrentSelectedItem = selectionPattern2 is null ? null : ReadReferencedElement(automation, () => selectionPattern2.CurrentCurrentSelectedItem),
                FirstSelectedItem = selectionPattern2 is null ? null : ReadReferencedElement(automation, () => selectionPattern2.CurrentFirstSelectedItem),
                LastSelectedItem = selectionPattern2 is null ? null : ReadReferencedElement(automation, () => selectionPattern2.CurrentLastSelectedItem),
                SelectedItems = selectedItems
            };
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(currentSelection);
            FinalRelease(selectionPattern2);
            FinalRelease(selectionPattern);
            FinalRelease(element);
            FinalRelease(automation);
        }
    });

    public static UiAutomationEventResult WaitForEvent(UiAutomationEventWaitRequest request) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? origin = null;
        IUIAutomationCacheRequest? cacheRequest = null;
        FocusChangedEventHandler? focusHandler = null;
        AutomationEventHandler? automationHandler = null;
        PropertyChangedEventHandler? propertyHandler = null;
        StructureChangedEventHandler? structureHandler = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, request.CacheRequest);
            var timeoutMs = Math.Max(1, request.TimeoutMs);
            var eventKind = request.EventKind.Trim().ToLowerInvariant();

            switch (eventKind)
            {
                case "focus":
                    focusHandler = new FocusChangedEventHandler();
                    automation.AddFocusChangedEventHandler(cacheRequest, focusHandler);
                    break;

                case "automation":
                    origin = ResolveEventOrigin(automation, request, cacheRequest);
                    automationHandler = new AutomationEventHandler();
                    automation.AddAutomationEventHandler(
                        request.EventId ?? throw new InvalidOperationException("An automation event requires --event-id."),
                        origin,
                        ParseTreeScope(request.Locator.Scope),
                        cacheRequest,
                        automationHandler);
                    break;

                case "property":
                    origin = ResolveEventOrigin(automation, request, cacheRequest);
                    propertyHandler = new PropertyChangedEventHandler();
                    var propertyId = request.PropertyId ?? throw new InvalidOperationException("A property-changed event requires --property-id.");
                    automation.AddPropertyChangedEventHandler(origin, ParseTreeScope(request.Locator.Scope), cacheRequest, propertyHandler, [propertyId]);
                    break;

                case "structure":
                    origin = ResolveEventOrigin(automation, request, cacheRequest);
                    structureHandler = new StructureChangedEventHandler();
                    automation.AddStructureChangedEventHandler(origin, ParseTreeScope(request.Locator.Scope), cacheRequest, structureHandler);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(request), request.EventKind, "Unsupported event kind.");
            }

            var signaled = eventKind switch
            {
                "focus" => focusHandler!.WaitHandle.WaitOne(timeoutMs),
                "automation" => automationHandler!.WaitHandle.WaitOne(timeoutMs),
                "property" => propertyHandler!.WaitHandle.WaitOne(timeoutMs),
                "structure" => structureHandler!.WaitHandle.WaitOne(timeoutMs),
                _ => false
            };

            return eventKind switch
            {
                "focus" => focusHandler!.ToResult(automation, !signaled),
                "automation" => automationHandler!.ToResult(automation, !signaled),
                "property" => propertyHandler!.ToResult(automation, !signaled),
                "structure" => structureHandler!.ToResult(automation, !signaled),
                _ => throw new ArgumentOutOfRangeException(nameof(request), request.EventKind, "Unsupported event kind.")
            };
        }
        finally
        {
            if (focusHandler is not null)
            {
                automation.RemoveFocusChangedEventHandler(focusHandler);
            }

            if (automationHandler is not null && origin is not null && request.EventId.HasValue)
            {
                automation.RemoveAutomationEventHandler(request.EventId.Value, origin, automationHandler);
            }

            if (propertyHandler is not null && origin is not null)
            {
                automation.RemovePropertyChangedEventHandler(origin, propertyHandler);
            }

            if (structureHandler is not null && origin is not null)
            {
                automation.RemoveStructureChangedEventHandler(origin, structureHandler);
            }

            focusHandler?.Dispose();
            automationHandler?.Dispose();
            propertyHandler?.Dispose();
            structureHandler?.Dispose();
            FinalRelease(cacheRequest);
            FinalRelease(origin);
            FinalRelease(automation);
        }
    });

    /// <summary>
    /// Reads a control that supports the Grid pattern as a rectangular cell matrix,
    /// including Table pattern headers when the control also exposes them.
    /// </summary>
    public static UiAutomationTableInfo? ReadTable(UiAutomationLocateRequest locator, int maxRows = 50, int maxColumns = 25) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? element = null;
        IUIAutomationGridPattern? gridPattern = null;
        IUIAutomationTablePattern? tablePattern = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, locator.CacheRequest);
            element = ResolveElement(automation, locator, throwIfNotFound: true, cacheRequest);
            gridPattern = GetPattern<IUIAutomationGridPattern>(element!, UIA_PatternIds.UIA_GridPatternId);
            if (gridPattern is null)
            {
                return null;
            }

            var rowCount = TryRead(() => gridPattern.CurrentRowCount, 0);
            var columnCount = TryRead(() => gridPattern.CurrentColumnCount, 0);
            var rowLimit = Math.Clamp(maxRows, 0, Math.Max(rowCount, 0));
            var columnLimit = Math.Clamp(maxColumns, 0, Math.Max(columnCount, 0));

            var rows = new List<UiAutomationTableRow>(rowLimit);
            for (var row = 0; row < rowLimit; row++)
            {
                var cells = new List<UiAutomationTableCell>(columnLimit);
                for (var column = 0; column < columnLimit; column++)
                {
                    cells.Add(ReadTableCell(gridPattern, row, column));
                }

                rows.Add(new UiAutomationTableRow { Row = row, Cells = cells });
            }

            tablePattern = GetPattern<IUIAutomationTablePattern>(element!, UIA_PatternIds.UIA_TablePatternId);

            return new UiAutomationTableInfo
            {
                RowCount = rowCount,
                ColumnCount = columnCount,
                HasTablePattern = tablePattern is not null,
                RowOrColumnMajor = tablePattern is null ? null : TryRead(() => (int)tablePattern.CurrentRowOrColumnMajor, 0),
                RowOrColumnMajorName = tablePattern is null ? null : TryRead(() => tablePattern.CurrentRowOrColumnMajor.ToString(), string.Empty),
                RowHeaders = tablePattern is null
                    ? Array.Empty<UiAutomationElementReference>()
                    : ReadElementReferenceArray(() => tablePattern.GetCurrentRowHeaders()),
                ColumnHeaders = tablePattern is null
                    ? Array.Empty<UiAutomationElementReference>()
                    : ReadElementReferenceArray(() => tablePattern.GetCurrentColumnHeaders()),
                Rows = rows,
                ReturnedRowCount = rowLimit,
                ReturnedColumnCount = columnLimit,
                Truncated = rowLimit < rowCount || columnLimit < columnCount
            };
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(tablePattern);
            FinalRelease(gridPattern);
            FinalRelease(element);
        }
    });

    private static UiAutomationTableCell ReadTableCell(IUIAutomationGridPattern gridPattern, int row, int column)
    {
        IUIAutomationElement? cell = null;
        IUIAutomationGridItemPattern? gridItem = null;
        IUIAutomationValuePattern? valuePattern = null;

        try
        {
            cell = gridPattern.GetItem(row, column);
            if (cell is null)
            {
                return new UiAutomationTableCell { Row = row, Column = column, IsUnavailable = true };
            }

            gridItem = GetPattern<IUIAutomationGridItemPattern>(cell, UIA_PatternIds.UIA_GridItemPatternId);
            valuePattern = GetPattern<IUIAutomationValuePattern>(cell, UIA_PatternIds.UIA_ValuePatternId);

            return new UiAutomationTableCell
            {
                // Prefer the cell's own reported coordinates: merged cells report the
                // origin of the span rather than the coordinates that were requested.
                Row = gridItem is null ? row : TryRead(() => gridItem.CurrentRow, row),
                Column = gridItem is null ? column : TryRead(() => gridItem.CurrentColumn, column),
                RowSpan = gridItem is null ? 1 : TryRead(() => gridItem.CurrentRowSpan, 1),
                ColumnSpan = gridItem is null ? 1 : TryRead(() => gridItem.CurrentColumnSpan, 1),
                Name = TryRead(() => cell.CurrentName, string.Empty) ?? string.Empty,
                ClassName = TryRead(() => cell.CurrentClassName, string.Empty) ?? string.Empty,
                AutomationId = TryRead(() => cell.CurrentAutomationId, string.Empty) ?? string.Empty,
                ControlType = TryRead(() => cell.CurrentControlType, 0),
                LocalizedControlType = TryRead(() => cell.CurrentLocalizedControlType, string.Empty) ?? string.Empty,
                Value = valuePattern is null ? null : TryRead(() => valuePattern.CurrentValue, null!),
                IsOffscreen = TryRead(() => cell.CurrentIsOffscreen != 0, false)
            };
        }
        catch (COMException)
        {
            // Virtualized rows that have never been realized fail here rather than
            // returning null, so report the gap instead of aborting the whole read.
            return new UiAutomationTableCell { Row = row, Column = column, IsUnavailable = true };
        }
        finally
        {
            FinalRelease(valuePattern);
            FinalRelease(gridItem);
            FinalRelease(cell);
        }
    }

    public static UiAutomationActionResult PerformAction(UiAutomationActionRequest request) => RunInSta(() =>
    {
        IUIAutomation automation = CreateAutomation();
        IUIAutomationElement? element = null;
        IUIAutomationCacheRequest? cacheRequest = null;

        try
        {
            cacheRequest = BuildCacheRequest(automation, request.Locator.CacheRequest);
            element = ResolveElement(automation, request.Locator, throwIfNotFound: true, cacheRequest);
            var action = request.Action.Trim().ToLowerInvariant();
            var message = action switch
            {
                "focus" => PerformFocus(element!),
                "invoke" => PerformInvoke(element!),
                "set-value" => PerformSetValue(element!, request.StringValue),
                "expand" => PerformExpandCollapse(element!, expand: true),
                "collapse" => PerformExpandCollapse(element!, expand: false),
                "toggle" => PerformToggle(element!),
                "select" => PerformSelectionItem(element!, "select"),
                "add-to-selection" => PerformSelectionItem(element!, "add"),
                "remove-from-selection" => PerformSelectionItem(element!, "remove"),
                "maximize" => PerformWindow(element!, WindowVisualState.WindowVisualState_Maximized),
                "minimize" => PerformWindow(element!, WindowVisualState.WindowVisualState_Minimized),
                "restore" => PerformWindow(element!, WindowVisualState.WindowVisualState_Normal),
                "close" => PerformClose(element!),
                "move" => PerformMove(element!, request.NumberValue, request.SecondNumberValue),
                "resize" => PerformResize(element!, request.NumberValue, request.SecondNumberValue),
                "scroll" => PerformScroll(element!, request.StringValue, request.SecondStringValue),
                "scroll-percent" => PerformScrollPercent(element!, request.NumberValue, request.SecondNumberValue),
                "set-range-value" => PerformSetRangeValue(element!, request.NumberValue),
                "set-view" => PerformSetView(element!, request.StringValue, request.IntValue),
                "dock" => PerformDock(element!, request.StringValue),
                _ => throw new ArgumentOutOfRangeException(nameof(request), request.Action, "Unsupported action.")
            };

            return new UiAutomationActionResult
            {
                Action = request.Action,
                Message = message,
                Element = ReadElementInfo(automation, element!)
            };
        }
        finally
        {
            FinalRelease(cacheRequest);
            FinalRelease(element);
            FinalRelease(automation);
        }
    });

    private static IUIAutomation CreateAutomation()
    {
        try
        {
            return new CUIAutomation8Class();
        }
        catch (COMException)
        {
            return new CUIAutomationClass();
        }
    }

    private static T RunInSta<T>(Func<T> operation)
    {
        T? result = default;
        Exception? captured = null;

        var thread = new Thread(() =>
        {
            try
            {
                result = operation();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (captured is not null)
        {
            throw captured;
        }

        return result!;
    }

    private static IUIAutomationElement? TryGetFocusedElement(IUIAutomation automation, IUIAutomationCacheRequest? cacheRequest = null)
    {
        try
        {
            return cacheRequest is null ? automation.GetFocusedElement() : automation.GetFocusedElementBuildCache(cacheRequest);
        }
        catch (COMException)
        {
            return null;
        }
    }

    private static IUIAutomationElement ResolveOrigin(IUIAutomation automation, UiAutomationSearchRequest request, IUIAutomationCacheRequest? cacheRequest)
    {
        if (request.DesktopRoot)
        {
            return cacheRequest is null ? automation.GetRootElement() : automation.GetRootElementBuildCache(cacheRequest);
        }

        if (request.FocusedElement || request.SearchFromFocused)
        {
            return TryGetFocusedElement(automation, cacheRequest)
                ?? throw new InvalidOperationException("No focused element is currently available.");
        }

        if (request.WindowHandle.HasValue)
        {
            return cacheRequest is null
                ? automation.ElementFromHandle(new IntPtr(request.WindowHandle.Value))
                : automation.ElementFromHandleBuildCache(new IntPtr(request.WindowHandle.Value), cacheRequest);
        }

        if (request.PointX.HasValue && request.PointY.HasValue)
        {
            return cacheRequest is null
                ? automation.ElementFromPoint(new tagPOINT { x = request.PointX.Value, y = request.PointY.Value })
                : automation.ElementFromPointBuildCache(new tagPOINT { x = request.PointX.Value, y = request.PointY.Value }, cacheRequest);
        }

        return cacheRequest is null ? automation.GetRootElement() : automation.GetRootElementBuildCache(cacheRequest);
    }

    private static IUIAutomationElement? ResolveElement(IUIAutomation automation, UiAutomationLocateRequest request, bool throwIfNotFound, IUIAutomationCacheRequest? cacheRequest)
    {
        if (request.PointX.HasValue != request.PointY.HasValue)
        {
            throw new ArgumentException("Both --x and --y are required when locating from a point.");
        }

        var hasFilter = HasSearchCriteria(request);

        if (!hasFilter)
        {
            if (request.DesktopRoot)
            {
                return cacheRequest is null ? automation.GetRootElement() : automation.GetRootElementBuildCache(cacheRequest);
            }

            if (request.FocusedElement)
            {
                var focused = TryGetFocusedElement(automation, cacheRequest);
                if (focused is not null || !throwIfNotFound)
                {
                    return focused;
                }

                throw new InvalidOperationException("No focused element is currently available.");
            }

            if (request.WindowHandle.HasValue)
            {
                return cacheRequest is null
                    ? automation.ElementFromHandle(new IntPtr(request.WindowHandle.Value))
                    : automation.ElementFromHandleBuildCache(new IntPtr(request.WindowHandle.Value), cacheRequest);
            }

            if (request.PointX.HasValue && request.PointY.HasValue)
            {
                return cacheRequest is null
                    ? automation.ElementFromPoint(new tagPOINT { x = request.PointX.Value, y = request.PointY.Value })
                    : automation.ElementFromPointBuildCache(new tagPOINT { x = request.PointX.Value, y = request.PointY.Value }, cacheRequest);
            }

            return cacheRequest is null ? automation.GetRootElement() : automation.GetRootElementBuildCache(cacheRequest);
        }

        IUIAutomationElement? origin = null;
        IUIAutomationCondition? filter = null;
        try
        {
            origin = ResolveLocateOrigin(automation, request, cacheRequest);
            filter = BuildFilterCondition(automation, request);
            var match = cacheRequest is null
                ? origin.FindFirst(ParseTreeScope(request.Scope), filter)
                : origin.FindFirstBuildCache(ParseTreeScope(request.Scope), filter, cacheRequest);
            if (match is null && throwIfNotFound)
            {
                throw new InvalidOperationException("The requested UI Automation element could not be found.");
            }

            return match;
        }
        finally
        {
            FinalRelease(filter);
            FinalRelease(origin);
        }
    }

    private static IUIAutomationElement ResolveEventOrigin(IUIAutomation automation, UiAutomationEventWaitRequest request, IUIAutomationCacheRequest? cacheRequest) =>
        ResolveElement(automation, request.Locator, throwIfNotFound: true, cacheRequest)
        ?? throw new InvalidOperationException("The requested UI Automation element could not be found.");

    private static IUIAutomationElement ResolveLocateOrigin(IUIAutomation automation, UiAutomationLocateRequest request, IUIAutomationCacheRequest? cacheRequest)
    {
        if (request.FocusedElement || request.SearchFromFocused)
        {
            return TryGetFocusedElement(automation, cacheRequest)
                ?? throw new InvalidOperationException("No focused element is currently available.");
        }

        if (request.WindowHandle.HasValue)
        {
            return cacheRequest is null
                ? automation.ElementFromHandle(new IntPtr(request.WindowHandle.Value))
                : automation.ElementFromHandleBuildCache(new IntPtr(request.WindowHandle.Value), cacheRequest);
        }

        if (request.PointX.HasValue && request.PointY.HasValue)
        {
            return cacheRequest is null
                ? automation.ElementFromPoint(new tagPOINT { x = request.PointX.Value, y = request.PointY.Value })
                : automation.ElementFromPointBuildCache(new tagPOINT { x = request.PointX.Value, y = request.PointY.Value }, cacheRequest);
        }

        return cacheRequest is null ? automation.GetRootElement() : automation.GetRootElementBuildCache(cacheRequest);
    }

    private static bool HasSearchCriteria(UiAutomationLocateRequest request) =>
        !string.IsNullOrWhiteSpace(request.Name)
        || !string.IsNullOrWhiteSpace(request.ClassName)
        || !string.IsNullOrWhiteSpace(request.AutomationId)
        || !string.IsNullOrWhiteSpace(request.FrameworkId)
        || request.ControlType.HasValue
        || request.ProcessId.HasValue;

    private static IUIAutomationCondition BuildFilterCondition(IUIAutomation automation, UiAutomationLocateRequest request)
    {
        var conditions = new List<IUIAutomationCondition>();

        try
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                conditions.Add(automation.CreatePropertyCondition(UIA_PropertyIds.UIA_NamePropertyId, request.Name));
            }

            if (!string.IsNullOrWhiteSpace(request.ClassName))
            {
                conditions.Add(automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ClassNamePropertyId, request.ClassName));
            }

            if (!string.IsNullOrWhiteSpace(request.AutomationId))
            {
                conditions.Add(automation.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, request.AutomationId));
            }

            if (!string.IsNullOrWhiteSpace(request.FrameworkId))
            {
                conditions.Add(automation.CreatePropertyCondition(UIA_PropertyIds.UIA_FrameworkIdPropertyId, request.FrameworkId));
            }

            if (request.ControlType.HasValue)
            {
                conditions.Add(automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, request.ControlType.Value));
            }

            if (request.ProcessId.HasValue)
            {
                conditions.Add(automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ProcessIdPropertyId, request.ProcessId.Value));
            }

            if (conditions.Count == 0)
            {
                return automation.CreateTrueCondition();
            }

            if (conditions.Count == 1)
            {
                return automation.CreateAndConditionFromArray(conditions.ToArray());
            }

            return automation.CreateAndConditionFromArray(conditions.ToArray());
        }
        finally
        {
            ReleaseAll(conditions);
        }
    }

    private static IUIAutomationCondition BuildFilterCondition(IUIAutomation automation, UiAutomationSearchRequest request) =>
        BuildFilterCondition(
            automation,
            new UiAutomationLocateRequest
            {
                Name = request.Name,
                ClassName = request.ClassName,
                AutomationId = request.AutomationId,
                FrameworkId = request.FrameworkId,
                ControlType = request.ControlType,
                ProcessId = request.ProcessId,
                Scope = request.Scope
            });

    private static IUIAutomationTreeWalker CreateWalker(IUIAutomation automation, string view) =>
        view.Trim().ToLowerInvariant() switch
        {
            "raw" => automation.RawViewWalker,
            "content" => automation.ContentViewWalker,
            "control" => automation.ControlViewWalker,
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, "Unsupported tree view.")
        };

    private static TreeScope ParseTreeScope(string? scope) => (scope ?? "subtree").Trim().ToLowerInvariant() switch
    {
        "element" => TreeScope.TreeScope_Element,
        "children" => TreeScope.TreeScope_Children,
        "descendants" => TreeScope.TreeScope_Descendants,
        "subtree" => TreeScope.TreeScope_Subtree,
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported tree scope.")
    };

    private static void VisitDescendants(
        IUIAutomationElement parent,
        IUIAutomationTreeWalker walker,
        IUIAutomationCacheRequest? cacheRequest,
        IUIAutomation automation,
        List<UiAutomationElementInfo> results,
        int limit)
    {
        if (results.Count >= limit)
        {
            return;
        }

        IUIAutomationElement? current = null;
        try
        {
            current = cacheRequest is null
                ? walker.GetFirstChildElement(parent)
                : walker.GetFirstChildElementBuildCache(parent, cacheRequest);

            while (current is not null && results.Count < limit)
            {
                IUIAutomationElement? next = null;
                try
                {
                    results.Add(ReadElementInfo(automation, current));
                    if (results.Count < limit)
                    {
                        VisitDescendants(current, walker, cacheRequest, automation, results, limit);
                    }

                    if (results.Count < limit)
                    {
                        next = cacheRequest is null
                            ? walker.GetNextSiblingElement(current)
                            : walker.GetNextSiblingElementBuildCache(current, cacheRequest);
                    }
                }
                finally
                {
                    FinalRelease(current);
                }

                current = next;
            }
        }
        finally
        {
            FinalRelease(current);
        }
    }

    private static IUIAutomationCacheRequest? BuildCacheRequest(IUIAutomation automation, UiAutomationCacheRequestInfo? cacheInfo)
    {
        if (cacheInfo is null || !cacheInfo.UseCache)
        {
            return null;
        }

        var cacheRequest = automation.CreateCacheRequest();
        cacheRequest.TreeScope = ParseTreeScope(cacheInfo.Scope);
        cacheRequest.TreeFilter = ParseViewCondition(automation, cacheInfo.View);
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;

        cacheRequest.AddProperty(UIA_PropertyIds.UIA_NamePropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_ClassNamePropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_ControlTypePropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_LocalizedControlTypePropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_ProcessIdPropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_AutomationIdPropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_FrameworkIdPropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_BoundingRectanglePropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_NativeWindowHandlePropertyId);
        cacheRequest.AddProperty(UIA_PropertyIds.UIA_IsEnabledPropertyId);

        return cacheRequest;
    }

    private static IUIAutomationCondition ParseViewCondition(IUIAutomation automation, string? view) => (view ?? "control").Trim().ToLowerInvariant() switch
    {
        "raw" => automation.RawViewCondition,
        "content" => automation.ContentViewCondition,
        "control" => automation.ControlViewCondition,
        _ => throw new ArgumentOutOfRangeException(nameof(view), view, "Unsupported cache view.")
    };

    private static UiAutomationElementInfo ReadElementInfo(IUIAutomation automation, IUIAutomationElement element)
    {
        var runtimeId = ReadRuntimeId(element);
        var supportedPatterns = ReadSupportedPatterns(automation, element);
        var bounds = element.CurrentBoundingRectangle;

        return new UiAutomationElementInfo
        {
            Name = element.CurrentName ?? string.Empty,
            ClassName = element.CurrentClassName ?? string.Empty,
            ControlType = element.CurrentControlType,
            LocalizedControlType = element.CurrentLocalizedControlType ?? string.Empty,
            ProcessId = element.CurrentProcessId,
            AutomationId = element.CurrentAutomationId ?? string.Empty,
            FrameworkId = element.CurrentFrameworkId ?? string.Empty,
            BoundingRectangle = ToRect(bounds),
            AcceleratorKey = element.CurrentAcceleratorKey ?? string.Empty,
            AccessKey = element.CurrentAccessKey ?? string.Empty,
            AriaProperties = TryRead(() => element.CurrentAriaProperties, string.Empty),
            AriaRole = TryRead(() => element.CurrentAriaRole, string.Empty),
            Culture = element.CurrentCulture,
            HasKeyboardFocus = element.CurrentHasKeyboardFocus != 0,
            HelpText = element.CurrentHelpText ?? string.Empty,
            IsContentElement = element.CurrentIsContentElement != 0,
            IsControlElement = element.CurrentIsControlElement != 0,
            IsDataValidForForm = TryRead(() => element.CurrentIsDataValidForForm != 0, false),
            IsEnabled = element.CurrentIsEnabled != 0,
            IsKeyboardFocusable = element.CurrentIsKeyboardFocusable != 0,
            IsOffscreen = element.CurrentIsOffscreen != 0,
            IsPassword = element.CurrentIsPassword != 0,
            IsRequiredForForm = TryRead(() => element.CurrentIsRequiredForForm != 0, false),
            ItemStatus = element.CurrentItemStatus ?? string.Empty,
            ItemType = element.CurrentItemType ?? string.Empty,
            NativeWindowHandle = TryRead(() => element.CurrentNativeWindowHandle.ToInt64(), 0L),
            Orientation = TryRead(() => (int)element.CurrentOrientation, 0),
            OrientationName = TryRead(() => element.CurrentOrientation.ToString(), string.Empty),
            ProviderDescription = TryRead(() => element.CurrentProviderDescription, string.Empty),
            RuntimeId = runtimeId,
            SupportedPatterns = supportedPatterns,
            ValuePattern = ReadValuePattern(element),
            RangeValuePattern = ReadRangeValuePattern(element),
            TogglePattern = ReadTogglePattern(element),
            ExpandCollapsePattern = ReadExpandCollapsePattern(element),
            WindowPattern = ReadWindowPattern(element),
            ScrollPattern = ReadScrollPattern(element),
            SelectionItemPattern = ReadSelectionItemPattern(automation, element),
            MultipleViewPattern = ReadMultipleViewPattern(element),
            DockPattern = ReadDockPattern(element),
            GridPattern = ReadGridPattern(element),
            GridItemPattern = ReadGridItemPattern(element),
            TablePattern = ReadTablePattern(element),
            TableItemPattern = ReadTableItemPattern(element)
        };
    }

    private static int[] ReadRuntimeId(IUIAutomationElement element)
    {
        try
        {
            var values = (int[])element.GetRuntimeId();
            return values;
        }
        catch (COMException)
        {
            return Array.Empty<int>();
        }
    }

    private static UiAutomationPatternInfo[] ReadSupportedPatterns(IUIAutomation automation, IUIAutomationElement element)
    {
        try
        {
            automation.PollForPotentialSupportedPatterns(element, out var patternIds, out _);
            var ids = patternIds as int[] ?? Array.Empty<int>();
            return ids.Select(id => new UiAutomationPatternInfo
            {
                Id = id,
                ProgrammaticName = PatternNames.TryGetValue(id, out var name) ? name : $"Pattern:{id}"
            }).ToArray();
        }
        catch (COMException)
        {
            return Array.Empty<UiAutomationPatternInfo>();
        }
    }

    private static UiAutomationValuePatternState? ReadValuePattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationValuePattern>(element, UIA_PatternIds.UIA_ValuePatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationValuePatternState
            {
                Value = pattern.CurrentValue ?? string.Empty,
                IsReadOnly = pattern.CurrentIsReadOnly != 0
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationRangeValuePatternState? ReadRangeValuePattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationRangeValuePattern>(element, UIA_PatternIds.UIA_RangeValuePatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationRangeValuePatternState
            {
                Value = pattern.CurrentValue,
                IsReadOnly = pattern.CurrentIsReadOnly != 0,
                Minimum = pattern.CurrentMinimum,
                Maximum = pattern.CurrentMaximum,
                SmallChange = pattern.CurrentSmallChange,
                LargeChange = pattern.CurrentLargeChange
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationTogglePatternState? ReadTogglePattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationTogglePattern>(element, UIA_PatternIds.UIA_TogglePatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationTogglePatternState
            {
                ToggleState = (int)pattern.CurrentToggleState,
                ToggleStateName = pattern.CurrentToggleState.ToString()
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationExpandCollapsePatternState? ReadExpandCollapsePattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationExpandCollapsePattern>(element, UIA_PatternIds.UIA_ExpandCollapsePatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationExpandCollapsePatternState
            {
                ExpandCollapseState = (int)pattern.CurrentExpandCollapseState,
                ExpandCollapseStateName = pattern.CurrentExpandCollapseState.ToString()
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationWindowPatternState? ReadWindowPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationWindowPattern>(element, UIA_PatternIds.UIA_WindowPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationWindowPatternState
            {
                CanMaximize = pattern.CurrentCanMaximize != 0,
                CanMinimize = pattern.CurrentCanMinimize != 0,
                IsModal = pattern.CurrentIsModal != 0,
                IsTopmost = pattern.CurrentIsTopmost != 0,
                WindowVisualState = (int)pattern.CurrentWindowVisualState,
                WindowVisualStateName = pattern.CurrentWindowVisualState.ToString(),
                WindowInteractionState = (int)pattern.CurrentWindowInteractionState
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationScrollPatternState? ReadScrollPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationScrollPattern>(element, UIA_PatternIds.UIA_ScrollPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationScrollPatternState
            {
                HorizontallyScrollable = pattern.CurrentHorizontallyScrollable != 0,
                HorizontalScrollPercent = pattern.CurrentHorizontalScrollPercent,
                HorizontalViewSize = pattern.CurrentHorizontalViewSize,
                VerticallyScrollable = pattern.CurrentVerticallyScrollable != 0,
                VerticalScrollPercent = pattern.CurrentVerticalScrollPercent,
                VerticalViewSize = pattern.CurrentVerticalViewSize
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationSelectionItemPatternState? ReadSelectionItemPattern(IUIAutomation automation, IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationSelectionItemPattern>(element, UIA_PatternIds.UIA_SelectionItemPatternId);
        if (pattern is null)
        {
            return null;
        }

        IUIAutomationElement? selectionContainer = null;
        try
        {
            selectionContainer = TryRead(() => pattern.CurrentSelectionContainer, null as IUIAutomationElement);
            return new UiAutomationSelectionItemPatternState
            {
                IsSelected = pattern.CurrentIsSelected != 0,
                SelectionContainer = selectionContainer is null ? null : ReadElementInfo(automation, selectionContainer)
            };
        }
        finally
        {
            FinalRelease(selectionContainer);
            FinalRelease(pattern);
        }
    }

    private static UiAutomationMultipleViewPatternState? ReadMultipleViewPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationMultipleViewPattern>(element, UIA_PatternIds.UIA_MultipleViewPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            var currentView = pattern.CurrentCurrentView;
            var supportedViews = TryRead(() => pattern.GetCurrentSupportedViews(), Array.Empty<int>()) ?? Array.Empty<int>();

            return new UiAutomationMultipleViewPatternState
            {
                CurrentView = currentView,
                CurrentViewName = ReadViewName(pattern, currentView),
                SupportedViews = supportedViews
                    .Select(id => new UiAutomationViewInfo { Id = id, Name = ReadViewName(pattern, id) })
                    .ToArray()
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string ReadViewName(IUIAutomationMultipleViewPattern pattern, int viewId) =>
        TryRead(() => pattern.GetViewName(viewId), string.Empty) ?? string.Empty;

    private static UiAutomationDockPatternState? ReadDockPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationDockPattern>(element, UIA_PatternIds.UIA_DockPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationDockPatternState
            {
                DockPosition = (int)pattern.CurrentDockPosition,
                DockPositionName = pattern.CurrentDockPosition.ToString()
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationGridPatternState? ReadGridPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationGridPattern>(element, UIA_PatternIds.UIA_GridPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationGridPatternState
            {
                RowCount = TryRead(() => pattern.CurrentRowCount, 0),
                ColumnCount = TryRead(() => pattern.CurrentColumnCount, 0)
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationGridItemPatternState? ReadGridItemPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationGridItemPattern>(element, UIA_PatternIds.UIA_GridItemPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationGridItemPatternState
            {
                Row = TryRead(() => pattern.CurrentRow, -1),
                Column = TryRead(() => pattern.CurrentColumn, -1),
                RowSpan = TryRead(() => pattern.CurrentRowSpan, 1),
                ColumnSpan = TryRead(() => pattern.CurrentColumnSpan, 1),
                ContainingGrid = ReadElementReference(() => pattern.CurrentContainingGrid)
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationTablePatternState? ReadTablePattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationTablePattern>(element, UIA_PatternIds.UIA_TablePatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationTablePatternState
            {
                RowOrColumnMajor = TryRead(() => (int)pattern.CurrentRowOrColumnMajor, 0),
                RowOrColumnMajorName = TryRead(() => pattern.CurrentRowOrColumnMajor.ToString(), string.Empty) ?? string.Empty,
                RowHeaders = ReadElementReferenceArray(() => pattern.GetCurrentRowHeaders()),
                ColumnHeaders = ReadElementReferenceArray(() => pattern.GetCurrentColumnHeaders())
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationTableItemPatternState? ReadTableItemPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationTableItemPattern>(element, UIA_PatternIds.UIA_TableItemPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationTableItemPatternState
            {
                RowHeaderItems = ReadElementReferenceArray(() => pattern.GetCurrentRowHeaderItems()),
                ColumnHeaderItems = ReadElementReferenceArray(() => pattern.GetCurrentColumnHeaderItems())
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static IReadOnlyList<UiAutomationElementInfo> ReadElementArray(IUIAutomation automation, IUIAutomationElementArray? elements)
    {
        if (elements is null)
        {
            return Array.Empty<UiAutomationElementInfo>();
        }

        var result = new List<UiAutomationElementInfo>(elements.Length);
        for (var index = 0; index < elements.Length; index++)
        {
            IUIAutomationElement? item = null;
            try
            {
                item = elements.GetElement(index);
                result.Add(ReadElementInfo(automation, item));
            }
            finally
            {
                FinalRelease(item);
            }
        }

        return result;
    }

    private static UiAutomationElementInfo? ReadReferencedElement(IUIAutomation automation, Func<IUIAutomationElement?> getter)
    {
        IUIAutomationElement? element = null;
        try
        {
            element = getter();
            return element is null ? null : ReadElementInfo(automation, element);
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            FinalRelease(element);
        }
    }

    private static UiAutomationElementReference? ReadElementReference(Func<IUIAutomationElement?> getter)
    {
        IUIAutomationElement? element = null;
        try
        {
            element = getter();
            return element is null ? null : ToElementReference(element);
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            FinalRelease(element);
        }
    }

    private static IReadOnlyList<UiAutomationElementReference> ReadElementReferenceArray(Func<IUIAutomationElementArray?> getter)
    {
        IUIAutomationElementArray? array = null;
        try
        {
            array = getter();
            if (array is null)
            {
                return Array.Empty<UiAutomationElementReference>();
            }

            var results = new List<UiAutomationElementReference>(array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                IUIAutomationElement? item = null;
                try
                {
                    item = array.GetElement(i);
                    if (item is not null)
                    {
                        results.Add(ToElementReference(item));
                    }
                }
                catch (COMException)
                {
                    // Skip entries the provider cannot realize.
                }
                finally
                {
                    FinalRelease(item);
                }
            }

            return results;
        }
        catch (COMException)
        {
            return Array.Empty<UiAutomationElementReference>();
        }
        finally
        {
            FinalRelease(array);
        }
    }

    private static UiAutomationElementReference ToElementReference(IUIAutomationElement element) => new()
    {
        Name = TryRead(() => element.CurrentName, string.Empty) ?? string.Empty,
        ClassName = TryRead(() => element.CurrentClassName, string.Empty) ?? string.Empty,
        AutomationId = TryRead(() => element.CurrentAutomationId, string.Empty) ?? string.Empty,
        ControlType = TryRead(() => element.CurrentControlType, 0),
        LocalizedControlType = TryRead(() => element.CurrentLocalizedControlType, string.Empty) ?? string.Empty,
        RuntimeId = ReadRuntimeId(element),
        BoundingRectangle = TryRead<UiAutomationRect?>(() => ToRect(element.CurrentBoundingRectangle), null)
    };

    private static TPattern? GetPattern<TPattern>(IUIAutomationElement element, int patternId)
        where TPattern : class
    {
        try
        {
            return element.GetCurrentPattern(patternId) as TPattern;
        }
        catch (COMException)
        {
            return null;
        }
    }

    private static string PerformFocus(IUIAutomationElement element)
    {
        element.SetFocus();
        return "Focus set.";
    }

    private static string PerformInvoke(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationInvokePattern>(element, UIA_PatternIds.UIA_InvokePatternId)
            ?? throw new InvalidOperationException("Element does not support the Invoke pattern.");

        try
        {
            pattern.Invoke();
            return "Invoked element.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformSetValue(IUIAutomationElement element, string? value)
    {
        var pattern = GetPattern<IUIAutomationValuePattern>(element, UIA_PatternIds.UIA_ValuePatternId)
            ?? throw new InvalidOperationException("Element does not support the Value pattern.");

        try
        {
            pattern.SetValue(value ?? string.Empty);
            return "Value updated.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformExpandCollapse(IUIAutomationElement element, bool expand)
    {
        var pattern = GetPattern<IUIAutomationExpandCollapsePattern>(element, UIA_PatternIds.UIA_ExpandCollapsePatternId)
            ?? throw new InvalidOperationException("Element does not support the ExpandCollapse pattern.");

        try
        {
            if (expand)
            {
                pattern.Expand();
                return "Expanded element.";
            }

            pattern.Collapse();
            return "Collapsed element.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformToggle(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationTogglePattern>(element, UIA_PatternIds.UIA_TogglePatternId)
            ?? throw new InvalidOperationException("Element does not support the Toggle pattern.");

        try
        {
            pattern.Toggle();
            return "Toggled element.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformSelectionItem(IUIAutomationElement element, string mode)
    {
        var pattern = GetPattern<IUIAutomationSelectionItemPattern>(element, UIA_PatternIds.UIA_SelectionItemPatternId)
            ?? throw new InvalidOperationException("Element does not support the SelectionItem pattern.");

        try
        {
            switch (mode)
            {
                case "select":
                    pattern.Select();
                    return "Selected item.";
                case "add":
                    pattern.AddToSelection();
                    return "Added item to selection.";
                case "remove":
                    pattern.RemoveFromSelection();
                    return "Removed item from selection.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported selection action.");
            }
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformWindow(IUIAutomationElement element, WindowVisualState state)
    {
        var pattern = GetPattern<IUIAutomationWindowPattern>(element, UIA_PatternIds.UIA_WindowPatternId)
            ?? throw new InvalidOperationException("Element does not support the Window pattern.");

        try
        {
            pattern.SetWindowVisualState(state);
            return $"Window state changed to {state}.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformClose(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationWindowPattern>(element, UIA_PatternIds.UIA_WindowPatternId)
            ?? throw new InvalidOperationException("Element does not support the Window pattern.");

        try
        {
            pattern.Close();
            return "Closed window.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformMove(IUIAutomationElement element, double? x, double? y)
    {
        var pattern = GetPattern<IUIAutomationTransformPattern>(element, UIA_PatternIds.UIA_TransformPatternId)
            ?? throw new InvalidOperationException("Element does not support the Transform pattern.");

        try
        {
            pattern.Move(
                x ?? throw new InvalidOperationException("The move action requires X and Y coordinates."),
                y ?? throw new InvalidOperationException("The move action requires X and Y coordinates."));
            return "Moved element.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformResize(IUIAutomationElement element, double? width, double? height)
    {
        var pattern = GetPattern<IUIAutomationTransformPattern>(element, UIA_PatternIds.UIA_TransformPatternId)
            ?? throw new InvalidOperationException("Element does not support the Transform pattern.");

        try
        {
            pattern.Resize(
                width ?? throw new InvalidOperationException("The resize action requires width and height."),
                height ?? throw new InvalidOperationException("The resize action requires width and height."));
            return "Resized element.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformScroll(IUIAutomationElement element, string? horizontal, string? vertical)
    {
        var pattern = GetPattern<IUIAutomationScrollPattern>(element, UIA_PatternIds.UIA_ScrollPatternId)
            ?? throw new InvalidOperationException("Element does not support the Scroll pattern.");

        try
        {
            pattern.Scroll(ParseScrollAmount(horizontal), ParseScrollAmount(vertical));
            return "Scrolled element.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformScrollPercent(IUIAutomationElement element, double? horizontalPercent, double? verticalPercent)
    {
        var pattern = GetPattern<IUIAutomationScrollPattern>(element, UIA_PatternIds.UIA_ScrollPatternId)
            ?? throw new InvalidOperationException("Element does not support the Scroll pattern.");

        try
        {
            pattern.SetScrollPercent(
                horizontalPercent ?? throw new InvalidOperationException("The scroll-percent action requires horizontal and vertical percentages."),
                verticalPercent ?? throw new InvalidOperationException("The scroll-percent action requires horizontal and vertical percentages."));
            return "Scroll percentages updated.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformSetRangeValue(IUIAutomationElement element, double? value)
    {
        var pattern = GetPattern<IUIAutomationRangeValuePattern>(element, UIA_PatternIds.UIA_RangeValuePatternId)
            ?? throw new InvalidOperationException("Element does not support the RangeValue pattern.");

        try
        {
            pattern.SetValue(value ?? throw new InvalidOperationException("The set-range-value action requires a numeric value."));
            return "Range value updated.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformSetView(IUIAutomationElement element, string? view, int? viewId)
    {
        var pattern = GetPattern<IUIAutomationMultipleViewPattern>(element, UIA_PatternIds.UIA_MultipleViewPatternId)
            ?? throw new InvalidOperationException("Element does not support the MultipleView pattern.");

        try
        {
            var supportedViews = TryRead(() => pattern.GetCurrentSupportedViews(), Array.Empty<int>()) ?? Array.Empty<int>();
            var resolved = ResolveViewId(pattern, supportedViews, view, viewId);
            pattern.SetCurrentView(resolved);
            return $"View changed to {resolved} ({ReadViewName(pattern, resolved)}).";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static int ResolveViewId(
        IUIAutomationMultipleViewPattern pattern,
        int[] supportedViews,
        string? view,
        int? viewId)
    {
        var requested = viewId?.ToString(CultureInfo.InvariantCulture) ?? view;
        if (string.IsNullOrWhiteSpace(requested))
        {
            throw new InvalidOperationException("The set-view action requires a view id or view name.");
        }

        requested = requested.Trim();

        // A numeric argument is only treated as an id when the control actually offers it,
        // so controls with numeric view names stay addressable by name.
        if (int.TryParse(requested, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedId)
            && supportedViews.Contains(parsedId))
        {
            return parsedId;
        }

        foreach (var candidate in supportedViews)
        {
            if (string.Equals(ReadViewName(pattern, candidate), requested, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        var available = supportedViews.Length == 0
            ? "none"
            : string.Join(", ", supportedViews.Select(id => $"{id}='{ReadViewName(pattern, id)}'"));
        throw new InvalidOperationException($"Unsupported view '{requested}'. Available views: {available}.");
    }

    private static string PerformDock(IUIAutomationElement element, string? position)
    {
        var pattern = GetPattern<IUIAutomationDockPattern>(element, UIA_PatternIds.UIA_DockPatternId)
            ?? throw new InvalidOperationException("Element does not support the Dock pattern.");

        try
        {
            var dockPosition = ParseDockPosition(position);
            pattern.SetDockPosition(dockPosition);
            return $"Dock position changed to {dockPosition}.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static DockPosition ParseDockPosition(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "top" => DockPosition.DockPosition_Top,
        "left" => DockPosition.DockPosition_Left,
        "bottom" => DockPosition.DockPosition_Bottom,
        "right" => DockPosition.DockPosition_Right,
        "fill" => DockPosition.DockPosition_Fill,
        "none" => DockPosition.DockPosition_None,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported dock position. Use top, left, bottom, right, fill, or none.")
    };

    private static ScrollAmount ParseScrollAmount(string? value) => (value ?? "no-amount").Trim().ToLowerInvariant() switch
    {
        "large-decrement" => ScrollAmount.ScrollAmount_LargeDecrement,
        "small-decrement" => ScrollAmount.ScrollAmount_SmallDecrement,
        "no-amount" => ScrollAmount.ScrollAmount_NoAmount,
        "small-increment" => ScrollAmount.ScrollAmount_SmallIncrement,
        "large-increment" => ScrollAmount.ScrollAmount_LargeIncrement,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported scroll amount.")
    };

    private static UiAutomationRect ToRect(tagRECT rect) => new()
    {
        Left = rect.left,
        Top = rect.top,
        Right = rect.right,
        Bottom = rect.bottom
    };

    private static TValue TryRead<TValue>(Func<TValue> getter, TValue fallback)
    {
        try
        {
            return getter();
        }
        catch (COMException)
        {
            return fallback;
        }
    }

    private static void ReleaseAll(IEnumerable<object?> comObjects)
    {
        foreach (var comObject in comObjects)
        {
            FinalRelease(comObject);
        }
    }

    private static void FinalRelease(object? comObject)
    {
        if (comObject is not null && Marshal.IsComObject(comObject))
        {
            Marshal.FinalReleaseComObject(comObject);
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class FocusChangedEventHandler : IUIAutomationFocusChangedEventHandler, IDisposable
    {
        private IUIAutomationElement? sender;

        public AutoResetEvent WaitHandle { get; } = new(false);

        public void HandleFocusChangedEvent(IUIAutomationElement sender)
        {
            if (this.sender is null)
            {
                this.sender = sender;
                WaitHandle.Set();
                return;
            }

            FinalRelease(sender);
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "focus",
                    TimedOut = timedOut,
                    SourceElement = sender is null ? null : ReadElementInfo(automation, sender)
                };
            }
            finally
            {
                FinalRelease(sender);
                sender = null;
            }
        }

        public void Dispose() => WaitHandle.Dispose();
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class AutomationEventHandler : IUIAutomationEventHandler, IDisposable
    {
        private IUIAutomationElement? sender;

        public AutoResetEvent WaitHandle { get; } = new(false);

        public int? EventId { get; private set; }

        public void HandleAutomationEvent(IUIAutomationElement sender, int eventId)
        {
            if (this.sender is null)
            {
                this.sender = sender;
                EventId = eventId;
                WaitHandle.Set();
                return;
            }

            FinalRelease(sender);
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "automation",
                    TimedOut = timedOut,
                    EventId = EventId,
                    SourceElement = sender is null ? null : ReadElementInfo(automation, sender)
                };
            }
            finally
            {
                FinalRelease(sender);
                sender = null;
            }
        }

        public void Dispose() => WaitHandle.Dispose();
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class PropertyChangedEventHandler : IUIAutomationPropertyChangedEventHandler, IDisposable
    {
        private IUIAutomationElement? sender;

        public AutoResetEvent WaitHandle { get; } = new(false);

        public int? PropertyId { get; private set; }

        public object? Value { get; private set; }

        public void HandlePropertyChangedEvent(IUIAutomationElement sender, int propertyId, object newValue)
        {
            if (this.sender is null)
            {
                this.sender = sender;
                PropertyId = propertyId;
                Value = newValue;
                WaitHandle.Set();
                return;
            }

            FinalRelease(sender);
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "property",
                    TimedOut = timedOut,
                    PropertyId = PropertyId,
                    Value = Value,
                    SourceElement = sender is null ? null : ReadElementInfo(automation, sender)
                };
            }
            finally
            {
                FinalRelease(sender);
                sender = null;
            }
        }

        public void Dispose() => WaitHandle.Dispose();
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class StructureChangedEventHandler : IUIAutomationStructureChangedEventHandler, IDisposable
    {
        private IUIAutomationElement? sender;

        public AutoResetEvent WaitHandle { get; } = new(false);

        public StructureChangeType? StructureChangeType { get; private set; }

        public int[]? RuntimeId { get; private set; }

        public void HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
        {
            if (this.sender is null)
            {
                this.sender = sender;
                StructureChangeType = changeType;
                RuntimeId = runtimeId;
                WaitHandle.Set();
                return;
            }

            FinalRelease(sender);
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "structure",
                    TimedOut = timedOut,
                    StructureChangeType = (int?)StructureChangeType,
                    StructureChangeTypeName = StructureChangeType?.ToString(),
                    Value = RuntimeId,
                    SourceElement = sender is null ? null : ReadElementInfo(automation, sender)
                };
            }
            finally
            {
                FinalRelease(sender);
                sender = null;
            }
        }

        public void Dispose() => WaitHandle.Dispose();
    }
}
