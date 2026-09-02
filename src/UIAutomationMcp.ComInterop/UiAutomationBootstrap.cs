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

    public static UiAutomationTextInfo? ReadText(UiAutomationLocateRequest locator, string? findText = null, bool matchCase = false, bool searchBackward = false) => RunInSta(() =>
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
                // The element may still be an inline child of a text container, which is
                // exactly the case TextChild exists for.
                var childOnly = ReadTextChild(element!);
                return childOnly is null
                    ? null
                    : new UiAutomationTextInfo { TextChild = childOnly };
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

            var textPattern2 = GetPattern<IUIAutomationTextPattern2>(element!, UIA_PatternIds.UIA_TextPattern2Id);
            var textEditPattern = GetPattern<IUIAutomationTextEditPattern>(element!, UIA_PatternIds.UIA_TextEditPatternId);

            try
            {
                return new UiAutomationTextInfo
                {
                    Text = documentRange?.GetText(-1) ?? string.Empty,
                    SupportedTextSelection = (int)textPattern.SupportedTextSelection,
                    SupportedTextSelectionName = textPattern.SupportedTextSelection.ToString(),
                    SelectedTexts = selectedTexts,
                    HasTextPattern2 = textPattern2 is not null,
                    HasTextEditPattern = textEditPattern is not null,
                    Caret = ReadCaret(textPattern2, documentRange),
                    Annotations = ReadAnnotations(documentRange),
                    TextChild = ReadTextChild(element!),
                    TextEdit = ReadTextEdit(textEditPattern),
                    Find = string.IsNullOrEmpty(findText) ? null : FindTextRun(documentRange, findText!, matchCase, searchBackward)
                };
            }
            finally
            {
                FinalRelease(textEditPattern);
                FinalRelease(textPattern2);
            }
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
                ItemCount = selectionPattern2?.CurrentItemCount,
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
        TextEditEventHandler? textEditHandler = null;
        NotificationEventHandler? notificationHandler = null;
        ChangesEventHandler? changesHandler = null;
        ActiveTextPositionEventHandler? activeTextHandler = null;

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

                case "text-edit":
                    origin = ResolveEventOrigin(automation, request, cacheRequest);
                    textEditHandler = new TextEditEventHandler();
                    var textEditAutomation = automation as IUIAutomation3
                        ?? throw new InvalidOperationException("Text-edit events require UI Automation 3 or later.");
                    foreach (var changeType in TextEditChangeTypes)
                    {
                        textEditAutomation.AddTextEditTextChangedEventHandler(
                            origin,
                            ParseTreeScope(request.Locator.Scope),
                            changeType,
                            cacheRequest,
                            textEditHandler);
                    }

                    break;

                case "notification":
                    origin = ResolveEventOrigin(automation, request, cacheRequest);
                    notificationHandler = new NotificationEventHandler();
                    var notificationAutomation = automation as IUIAutomation5
                        ?? throw new InvalidOperationException("Notification events require UI Automation 5 or later.");
                    notificationAutomation.AddNotificationEventHandler(
                        origin,
                        ParseTreeScope(request.Locator.Scope),
                        cacheRequest,
                        notificationHandler);
                    break;

                case "changes":
                    origin = ResolveEventOrigin(automation, request, cacheRequest);
                    changesHandler = new ChangesEventHandler();
                    var changesAutomation = automation as IUIAutomation4
                        ?? throw new InvalidOperationException("Changes events require UI Automation 4 or later.");
                    // The registration takes a ref to the first element of a change-type
                    // array plus a count, not an array parameter.
                    var changeTypes = new[] { request.ChangeId ?? UIA_ChangeIds.UIA_SummaryChangeId };
                    changesAutomation.AddChangesEventHandler(
                        origin,
                        ParseTreeScope(request.Locator.Scope),
                        ref changeTypes[0],
                        changeTypes.Length,
                        cacheRequest,
                        changesHandler);
                    break;

                case "active-text-position":
                    origin = ResolveEventOrigin(automation, request, cacheRequest);
                    activeTextHandler = new ActiveTextPositionEventHandler();
                    var activeTextAutomation = automation as IUIAutomation6
                        ?? throw new InvalidOperationException("Active-text-position events require UI Automation 6 or later.");
                    activeTextAutomation.AddActiveTextPositionChangedEventHandler(
                        origin,
                        ParseTreeScope(request.Locator.Scope),
                        cacheRequest,
                        activeTextHandler);
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
                "text-edit" => textEditHandler!.WaitHandle.WaitOne(timeoutMs),
                "notification" => notificationHandler!.WaitHandle.WaitOne(timeoutMs),
                "changes" => changesHandler!.WaitHandle.WaitOne(timeoutMs),
                "active-text-position" => activeTextHandler!.WaitHandle.WaitOne(timeoutMs),
                _ => false
            };

            return eventKind switch
            {
                "focus" => focusHandler!.ToResult(automation, !signaled),
                "automation" => automationHandler!.ToResult(automation, !signaled),
                "property" => propertyHandler!.ToResult(automation, !signaled),
                "structure" => structureHandler!.ToResult(automation, !signaled),
                "text-edit" => textEditHandler!.ToResult(automation, !signaled),
                "notification" => notificationHandler!.ToResult(automation, !signaled),
                "changes" => changesHandler!.ToResult(automation, !signaled),
                "active-text-position" => activeTextHandler!.ToResult(automation, !signaled),
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

            if (textEditHandler is not null && origin is not null && automation is IUIAutomation3 automation3)
            {
                automation3.RemoveTextEditTextChangedEventHandler(origin, textEditHandler);
            }

            if (notificationHandler is not null && origin is not null && automation is IUIAutomation5 automation5)
            {
                automation5.RemoveNotificationEventHandler(origin, notificationHandler);
            }

            if (changesHandler is not null && origin is not null && automation is IUIAutomation4 automation4)
            {
                automation4.RemoveChangesEventHandler(origin, changesHandler);
            }

            if (activeTextHandler is not null && origin is not null && automation is IUIAutomation6 automation6)
            {
                automation6.RemoveActiveTextPositionChangedEventHandler(origin, activeTextHandler);
            }

            focusHandler?.Dispose();
            automationHandler?.Dispose();
            propertyHandler?.Dispose();
            structureHandler?.Dispose();
            textEditHandler?.Dispose();
            notificationHandler?.Dispose();
            changesHandler?.Dispose();
            activeTextHandler?.Dispose();
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
                    ? []
                    : ReadElementReferenceArray(() => tablePattern.GetCurrentRowHeaders()),
                ColumnHeaders = tablePattern is null
                    ? []
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
                "rotate" => PerformRotate(element!, request.NumberValue),
                "scroll" => PerformScroll(element!, request.StringValue, request.SecondStringValue),
                "scroll-percent" => PerformScrollPercent(element!, request.NumberValue, request.SecondNumberValue),
                "set-range-value" => PerformSetRangeValue(element!, request.NumberValue),
                "set-view" => PerformSetView(element!, request.StringValue, request.IntValue),
                "dock" => PerformDock(element!, request.StringValue),
                "realize" => PerformRealize(element!),
                "scroll-into-view" => PerformScrollIntoView(element!),
                "select-text" => InvokeTextRangeOperation(() => PerformSelectText(element!, request.StringValue, request.IntValue, request.NumberValue, request.MatchCase, request.SearchBackward)),
                "move-caret" => InvokeTextRangeOperation(() => PerformMoveCaret(element!, request.StringValue, request.IntValue, request.MatchCase, request.SearchBackward)),
                "scroll-text-into-view" => InvokeTextRangeOperation(() => PerformScrollTextIntoView(element!, request.StringValue, request.IntValue, request.NumberValue, request.MatchCase, request.SearchBackward)),
                "default-action" => PerformDefaultAction(element!),
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

            // A virtualized list only materializes the rows currently in view, so a
            // miss here does not mean the item is absent. Ask the provider directly
            // before giving up. This only runs on the failure path, so searches that
            // already succeed are unaffected.
            if (match is null && request.RealizeVirtualized)
            {
                match = TryFindVirtualizedItem(automation, origin, request);
            }

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

    /// <summary>
    /// Last-resort lookup for items a virtualizing container knows about but has not
    /// materialized. Walks the origin and any ItemContainer descendants, asks the
    /// provider via <c>FindItemByProperty</c>, and realizes the result so callers get
    /// a live element rather than a placeholder.
    /// </summary>
    private static IUIAutomationElement? TryFindVirtualizedItem(
        IUIAutomation automation,
        IUIAutomationElement origin,
        UiAutomationLocateRequest request)
    {
        var (propertyId, value) = SelectContainerSearchProperty(request);
        if (propertyId == 0)
        {
            return null;
        }

        foreach (var container in EnumerateItemContainers(automation, origin))
        {
            IUIAutomationItemContainerPattern? pattern = null;
            try
            {
                pattern = GetPattern<IUIAutomationItemContainerPattern>(container, UIA_PatternIds.UIA_ItemContainerPatternId);
                if (pattern is null)
                {
                    continue;
                }

                IUIAutomationElement? found;
                try
                {
                    found = pattern.FindItemByProperty(null!, propertyId, value);
                }
                catch (Exception ex) when (ex is COMException or ArgumentException)
                {
                    // Providers that advertise ItemContainer but do not implement the
                    // requested property fail rather than returning null. Interop maps
                    // E_INVALIDARG to ArgumentException, so both types must be handled.
                    continue;
                }

                if (found is null)
                {
                    continue;
                }

                RealizeVirtualizedItem(found);
                if (MatchesRemainingCriteria(found, request, propertyId))
                {
                    return found;
                }

                FinalRelease(found);
            }
            finally
            {
                FinalRelease(pattern);
                if (!ReferenceEquals(container, origin))
                {
                    FinalRelease(container);
                }
            }
        }

        return null;
    }

    private static (int PropertyId, object? Value) SelectContainerSearchProperty(UiAutomationLocateRequest request)
    {
        // FindItemByProperty takes exactly one property, so pick the most selective
        // criterion available and verify the rest on the result.
        if (!string.IsNullOrWhiteSpace(request.AutomationId))
        {
            return (UIA_PropertyIds.UIA_AutomationIdPropertyId, request.AutomationId);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            return (UIA_PropertyIds.UIA_NamePropertyId, request.Name);
        }

        if (!string.IsNullOrWhiteSpace(request.ClassName))
        {
            return (UIA_PropertyIds.UIA_ClassNamePropertyId, request.ClassName);
        }

        if (request.ControlType.HasValue)
        {
            return (UIA_PropertyIds.UIA_ControlTypePropertyId, request.ControlType.Value);
        }

        return (0, null);
    }

    private static bool MatchesRemainingCriteria(IUIAutomationElement element, UiAutomationLocateRequest request, int usedPropertyId)
    {
        if (usedPropertyId != UIA_PropertyIds.UIA_AutomationIdPropertyId
            && !string.IsNullOrWhiteSpace(request.AutomationId)
            && !string.Equals(TryRead(() => element.CurrentAutomationId, null), request.AutomationId, StringComparison.Ordinal))
        {
            return false;
        }

        if (usedPropertyId != UIA_PropertyIds.UIA_NamePropertyId
            && !string.IsNullOrWhiteSpace(request.Name)
            && !string.Equals(TryRead(() => element.CurrentName, null), request.Name, StringComparison.Ordinal))
        {
            return false;
        }

        if (usedPropertyId != UIA_PropertyIds.UIA_ClassNamePropertyId
            && !string.IsNullOrWhiteSpace(request.ClassName)
            && !string.Equals(TryRead(() => element.CurrentClassName, null), request.ClassName, StringComparison.Ordinal))
        {
            return false;
        }

        if (usedPropertyId != UIA_PropertyIds.UIA_ControlTypePropertyId
            && request.ControlType.HasValue
            && TryRead(() => element.CurrentControlType, 0) != request.ControlType.Value)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.FrameworkId)
            && !string.Equals(TryRead(() => element.CurrentFrameworkId, null), request.FrameworkId, StringComparison.Ordinal))
        {
            return false;
        }

        return !request.ProcessId.HasValue || TryRead(() => element.CurrentProcessId, 0) == request.ProcessId.Value;
    }

    private static IEnumerable<IUIAutomationElement> EnumerateItemContainers(IUIAutomation automation, IUIAutomationElement origin)
    {
        if (GetPattern<IUIAutomationItemContainerPattern>(origin, UIA_PatternIds.UIA_ItemContainerPatternId) is { } originPattern)
        {
            FinalRelease(originPattern);
            yield return origin;
        }

        IUIAutomationCondition? condition = null;
        IUIAutomationElementArray? containers = null;
        try
        {
            condition = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_IsItemContainerPatternAvailablePropertyId, true);
            containers = origin.FindAll(TreeScope.TreeScope_Descendants, condition);
        }
        catch (Exception ex) when (ex is COMException or ArgumentException)
        {
            containers = null;
        }
        finally
        {
            FinalRelease(condition);
        }

        if (containers is null)
        {
            yield break;
        }

        try
        {
            for (var i = 0; i < containers.Length; i++)
            {
                IUIAutomationElement? container;
                try
                {
                    container = containers.GetElement(i);
                }
                catch (Exception ex) when (ex is COMException or ArgumentException)
                {
                    continue;
                }

                if (container is not null)
                {
                    yield return container;
                }
            }
        }
        finally
        {
            FinalRelease(containers);
        }
    }

    private static void RealizeVirtualizedItem(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationVirtualizedItemPattern>(element, UIA_PatternIds.UIA_VirtualizedItemPatternId);
        if (pattern is null)
        {
            return;
        }

        try
        {
            pattern.Realize();
        }
        catch (Exception ex) when (ex is COMException or ArgumentException)
        {
            // The provider may realize lazily on first access instead; the element is
            // still usable, so a failure here must not fail the lookup.
        }
        finally
        {
            FinalRelease(pattern);
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
        || request.ProcessId.HasValue
        // A request carrying only negative criteria is still a request. Treating it
        // as "no locator" would silently resolve to the search origin instead.
        || !string.IsNullOrWhiteSpace(request.NotName)
        || !string.IsNullOrWhiteSpace(request.NotClassName)
        || !string.IsNullOrWhiteSpace(request.NotAutomationId)
        || request.NotControlType.HasValue;

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

            AddNegatedCondition(automation, conditions, UIA_PropertyIds.UIA_NamePropertyId, request.NotName);
            AddNegatedCondition(automation, conditions, UIA_PropertyIds.UIA_ClassNamePropertyId, request.NotClassName);
            AddNegatedCondition(automation, conditions, UIA_PropertyIds.UIA_AutomationIdPropertyId, request.NotAutomationId);
            if (request.NotControlType.HasValue)
            {
                AddNegatedCondition(automation, conditions, UIA_PropertyIds.UIA_ControlTypePropertyId, request.NotControlType.Value);
            }

            if (conditions.Count == 0)
            {
                return automation.CreateTrueCondition();
            }

            if (conditions.Count == 1)
            {
                return automation.CreateAndConditionFromArray([.. conditions]);
            }

            return automation.CreateAndConditionFromArray([.. conditions]);
        }
        finally
        {
            ReleaseAll(conditions);
        }
    }

    /// <summary>
    /// Wraps a property condition in <c>CreateNotCondition</c> and adds it to the
    /// AND-composed list. The inner condition is released immediately: the Not
    /// wrapper holds its own reference, and the caller only ever releases what is
    /// in <paramref name="conditions"/>.
    /// </summary>
    private static void AddNegatedCondition(
        IUIAutomation automation,
        List<IUIAutomationCondition> conditions,
        int propertyId,
        object? value)
    {
        if (value is null || (value is string text && string.IsNullOrWhiteSpace(text)))
        {
            return;
        }

        IUIAutomationCondition? inner = null;
        try
        {
            inner = automation.CreatePropertyCondition(propertyId, value);
            conditions.Add(automation.CreateNotCondition(inner));
        }
        finally
        {
            FinalRelease(inner);
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
                NotName = request.NotName,
                NotClassName = request.NotClassName,
                NotAutomationId = request.NotAutomationId,
                NotControlType = request.NotControlType,
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

    /// <summary>
    /// Projects an element into a stable snapshot.
    /// </summary>
    /// <remarks>
    /// Every read is failure-tolerant. An element reaching here may already be
    /// gone - an event handler receives a sender that can be destroyed before the
    /// handler runs, and a tree walk can outlive the window it is walking - so a
    /// dead element must degrade to empty values rather than throw and take the
    /// whole enumeration or event result with it.
    /// </remarks>
    private static UiAutomationElementInfo ReadElementInfo(IUIAutomation automation, IUIAutomationElement element)
    {
        var runtimeId = ReadRuntimeId(element);
        var supportedPatterns = ReadSupportedPatterns(automation, element);
        var bounds = TryRead(() => element.CurrentBoundingRectangle, default);
        var legacy = ReadLegacyAccessiblePattern(element);

        var name = TryRead(() => element.CurrentName, string.Empty) ?? string.Empty;
        var nameSource = "uia";
        if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(legacy?.Name))
        {
            name = legacy!.Name;
            nameSource = "legacy";
        }

        // A labelling element is the last resort, and only when neither the native
        // name nor the MSAA bridge produced one. Win32 and WinForms inputs routinely
        // carry no name of their own and are identifiable only through their label.
        var labeledBy = ReadElementReference(() => TryRead(() => element.CurrentLabeledBy, null));
        if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(labeledBy?.Name))
        {
            name = labeledBy!.Name;
            nameSource = "labeledBy";
        }

        var extended = ReadExtendedElementInfo(element);

        var localizedControlType = TryRead(() => element.CurrentLocalizedControlType, string.Empty) ?? string.Empty;
        var localizedControlTypeSource = "uia";
        if (string.IsNullOrEmpty(localizedControlType) && !string.IsNullOrEmpty(legacy?.RoleName))
        {
            localizedControlType = legacy!.RoleName;
            localizedControlTypeSource = "legacy";
        }

        return new UiAutomationElementInfo
        {
            Name = name,
            ClassName = TryRead(() => element.CurrentClassName, string.Empty) ?? string.Empty,
            ControlType = TryRead(() => element.CurrentControlType, 0),
            LocalizedControlType = localizedControlType,
            ProcessId = TryRead(() => element.CurrentProcessId, 0),
            AutomationId = TryRead(() => element.CurrentAutomationId, string.Empty) ?? string.Empty,
            FrameworkId = TryRead(() => element.CurrentFrameworkId, string.Empty) ?? string.Empty,
            BoundingRectangle = ToRect(bounds),
            AcceleratorKey = TryRead(() => element.CurrentAcceleratorKey, string.Empty) ?? string.Empty,
            AccessKey = TryRead(() => element.CurrentAccessKey, string.Empty) ?? string.Empty,
            AriaProperties = TryRead(() => element.CurrentAriaProperties, string.Empty),
            AriaRole = TryRead(() => element.CurrentAriaRole, string.Empty),
            Culture = TryRead(() => element.CurrentCulture, 0),
            HasKeyboardFocus = TryRead(() => element.CurrentHasKeyboardFocus != 0, false),
            HelpText = TryRead(() => element.CurrentHelpText, string.Empty) ?? string.Empty,
            IsContentElement = TryRead(() => element.CurrentIsContentElement != 0, false),
            IsControlElement = TryRead(() => element.CurrentIsControlElement != 0, false),
            IsDataValidForForm = TryRead(() => element.CurrentIsDataValidForForm != 0, false),
            IsEnabled = TryRead(() => element.CurrentIsEnabled != 0, false),
            IsKeyboardFocusable = TryRead(() => element.CurrentIsKeyboardFocusable != 0, false),
            IsOffscreen = TryRead(() => element.CurrentIsOffscreen != 0, false),
            IsPassword = TryRead(() => element.CurrentIsPassword != 0, false),
            IsRequiredForForm = TryRead(() => element.CurrentIsRequiredForForm != 0, false),
            ItemStatus = TryRead(() => element.CurrentItemStatus, string.Empty) ?? string.Empty,
            ItemType = TryRead(() => element.CurrentItemType, string.Empty) ?? string.Empty,
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
            TransformPattern = ReadTransformPattern(element),
            DockPattern = ReadDockPattern(element),
            GridPattern = ReadGridPattern(element),
            GridItemPattern = ReadGridItemPattern(element),
            TablePattern = ReadTablePattern(element),
            TableItemPattern = ReadTableItemPattern(element),
            Virtualization = ReadVirtualization(supportedPatterns),
            DragPattern = ReadDragPattern(element),
            DropTargetPattern = ReadDropTargetPattern(element),
            LegacyAccessiblePattern = legacy,
            LabeledBy = labeledBy,
            ControllerFor = ReadElementReferenceArray(() => TryRead(() => element.CurrentControllerFor, null)),
            DescribedBy = ReadElementReferenceArray(() => TryRead(() => element.CurrentDescribedBy, null)),
            FlowsTo = ReadElementReferenceArray(() => TryRead(() => element.CurrentFlowsTo, null)),
            FlowsFrom = extended.FlowsFrom,
            FullDescription = extended.FullDescription,
            PositionInSet = extended.PositionInSet,
            SizeOfSet = extended.SizeOfSet,
            Level = extended.Level,
            AnnotationTypes = extended.AnnotationTypes,
            LandmarkType = extended.LandmarkType,
            LocalizedLandmarkType = extended.LocalizedLandmarkType,
            HeadingLevel = extended.HeadingLevel,
            IsDialog = extended.IsDialog,
            IsPeripheral = extended.IsPeripheral,
            LiveSetting = extended.LiveSetting,
            LiveSettingName = extended.LiveSettingName,
            OptimizeForVisualContent = extended.OptimizeForVisualContent,
            NameSource = nameSource,
            LocalizedControlTypeSource = localizedControlTypeSource
        };
    }

    private static int[] ReadRuntimeId(IUIAutomationElement element)
    {
        try
        {
            return element.GetRuntimeId() as int[] ?? [];
        }
        catch (COMException)
        {
            return [];
        }
    }

    /// <summary>
    /// Reads the properties introduced by <c>IUIAutomationElement2</c> through
    /// <c>IUIAutomationElement9</c>.
    /// </summary>
    /// <remarks>
    /// Each interface level is cast independently and failures degrade to null,
    /// so a Windows build that exposes Element4 but not Element9 still yields
    /// everything it has. This mirrors how <see cref="WaitForEvent"/> soft-casts
    /// to <c>IUIAutomation3</c> rather than demanding a minimum OS version.
    ///
    /// The element is not a separate COM object at any level - these are QueryInterface
    /// views onto the proxy the caller already owns - so nothing here is released.
    /// </remarks>
    private static UiAutomationExtendedElementInfo ReadExtendedElementInfo(IUIAutomationElement element)
    {
        var info = new UiAutomationExtendedElementInfo();

        if (element is IUIAutomationElement2 e2)
        {
            info.LiveSetting = TryRead<int?>(() => (int)e2.CurrentLiveSetting, null);
            info.LiveSettingName = TryRead(() => e2.CurrentLiveSetting.ToString(), null);
            info.OptimizeForVisualContent = TryRead<bool?>(() => e2.CurrentOptimizeForVisualContent != 0, null);
            info.FlowsFrom = ReadElementReferenceArray(() => TryRead(() => e2.CurrentFlowsFrom, null));
        }

        if (element is IUIAutomationElement3 e3)
        {
            info.IsPeripheral = TryRead<bool?>(() => e3.CurrentIsPeripheral != 0, null);
        }

        if (element is IUIAutomationElement4 e4)
        {
            info.PositionInSet = TryRead<int?>(() => e4.CurrentPositionInSet, null);
            info.SizeOfSet = TryRead<int?>(() => e4.CurrentSizeOfSet, null);
            info.Level = TryRead<int?>(() => e4.CurrentLevel, null);
            // Element-level annotation types, distinct from the text-range annotation
            // walk in ReadAnnotations: these describe the element, not a run of text.
            info.AnnotationTypes = TryRead<IReadOnlyList<int>?>(
                () => (e4.CurrentAnnotationTypes as int[])?.ToArray(),
                null);
        }

        if (element is IUIAutomationElement5 e5)
        {
            info.LandmarkType = TryRead<int?>(() => e5.CurrentLandmarkType, null);
            info.LocalizedLandmarkType = TryRead(() => e5.CurrentLocalizedLandmarkType, null);
        }

        if (element is IUIAutomationElement6 e6)
        {
            info.FullDescription = TryRead(() => e6.CurrentFullDescription, null);
        }

        if (element is IUIAutomationElement8 e8)
        {
            // UIA reports headings as 80051..80059 and non-headings as
            // HeadingLevel_None (80050), so a raw passthrough would put a
            // meaningless five-digit constant on every element in a tree. Project
            // the ordinary 1..9 a caller expects, and null for "not a heading".
            info.HeadingLevel = TryRead<int?>(
                () =>
                {
                    var raw = (int)e8.CurrentHeadingLevel;
                    return raw > UIA_HeadingLevelIds.HeadingLevel_None && raw <= UIA_HeadingLevelIds.HeadingLevel9
                        ? raw - UIA_HeadingLevelIds.HeadingLevel_None
                        : null;
                },
                null);
        }

        if (element is IUIAutomationElement9 e9)
        {
            info.IsDialog = TryRead<bool?>(() => e9.CurrentIsDialog != 0, null);
        }

        return info;
    }

    private sealed class UiAutomationExtendedElementInfo
    {
        public string? FullDescription { get; set; }
        public int? PositionInSet { get; set; }
        public int? SizeOfSet { get; set; }
        public int? Level { get; set; }
        public IReadOnlyList<int>? AnnotationTypes { get; set; }
        public int? LandmarkType { get; set; }
        public string? LocalizedLandmarkType { get; set; }
        public int? HeadingLevel { get; set; }
        public bool? IsDialog { get; set; }
        public bool? IsPeripheral { get; set; }
        public int? LiveSetting { get; set; }
        public string? LiveSettingName { get; set; }
        public bool? OptimizeForVisualContent { get; set; }
        public IReadOnlyList<UiAutomationElementReference> FlowsFrom { get; set; } = [];
    }

    private static UiAutomationPatternInfo[] ReadSupportedPatterns(IUIAutomation automation, IUIAutomationElement element)
    {
        try
        {
            automation.PollForPotentialSupportedPatterns(element, out var patternIds, out _);
            var ids = patternIds as int[] ?? [];
            return [.. ids
                // Providers occasionally report a 0 id, which is not a pattern.
                // Passing it through would emit a meaningless "Pattern:0" entry.
                .Where(id => id > 0)
                .Select(id => new UiAutomationPatternInfo
                {
                    Id = id,
                    ProgrammaticName = PatternNames.TryGetValue(id, out var name) ? name : $"Pattern:{id}"
                })];
        }
        catch (COMException)
        {
            return [];
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
            var supportedViews = TryRead(() => pattern.GetCurrentSupportedViews(), []) ?? [];

            return new UiAutomationMultipleViewPatternState
            {
                CurrentView = currentView,
                CurrentViewName = ReadViewName(pattern, currentView),
                SupportedViews = [.. supportedViews.Select(id => new UiAutomationViewInfo { Id = id, Name = ReadViewName(pattern, id) })]
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string ReadViewName(IUIAutomationMultipleViewPattern pattern, int viewId) =>
        TryRead(() => pattern.GetViewName(viewId), string.Empty) ?? string.Empty;

    private static UiAutomationDragPatternState? ReadDragPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationDragPattern>(element, UIA_PatternIds.UIA_DragPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationDragPatternState
            {
                IsGrabbed = pattern.CurrentIsGrabbed != 0,
                DropEffect = pattern.CurrentDropEffect ?? string.Empty,
                DropEffects = pattern.CurrentDropEffects ?? [],
                GrabbedItems = ReadElementReferenceArray(() => pattern.GetCurrentGrabbedItems())
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationDropTargetPatternState? ReadDropTargetPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationDropTargetPattern>(element, UIA_PatternIds.UIA_DropTargetPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationDropTargetPatternState
            {
                DropTargetEffect = pattern.CurrentDropTargetEffect ?? string.Empty,
                DropTargetEffects = pattern.CurrentDropTargetEffects ?? []
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static UiAutomationTransformPatternState? ReadTransformPattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationTransformPattern>(element, UIA_PatternIds.UIA_TransformPatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            return new UiAutomationTransformPatternState
            {
                CanMove = pattern.CurrentCanMove != 0,
                CanResize = pattern.CurrentCanResize != 0,
                CanRotate = pattern.CurrentCanRotate != 0
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

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

    private static readonly string[] MsaaRoleNames =
    [
        "TitleBar", "MenuBar", "ScrollBar", "Grip", "Sound", "Cursor", "Caret", "Alert",
        "Window", "Client", "MenuPopup", "MenuItem", "ToolTip", "Application", "Document",
        "Pane", "Chart", "Dialog", "Border", "Grouping", "Separator", "ToolBar", "StatusBar",
        "Table", "ColumnHeader", "RowHeader", "Column", "Row", "Cell", "Link", "HelpBalloon",
        "Character", "List", "ListItem", "Outline", "OutlineItem", "PageTab", "PropertyPage",
        "Indicator", "Graphic", "StaticText", "Text", "PushButton", "CheckButton",
        "RadioButton", "ComboBox", "DropList", "ProgressBar", "Dial", "HotkeyField", "Slider",
        "SpinButton", "Diagram", "Animation", "Equation", "ButtonDropDown", "ButtonMenu",
        "ButtonDropDownGrid", "Whitespace", "PageTabList", "Clock", "SplitButton",
        "IpAddress", "OutlineButton"
    ];

    private static readonly (uint Flag, string Name)[] MsaaStateFlags =
    [
        (0x00000001u, "Unavailable"), (0x00000002u, "Selected"), (0x00000004u, "Focused"),
        (0x00000008u, "Pressed"), (0x00000010u, "Checked"), (0x00000020u, "Mixed"),
        (0x00000040u, "ReadOnly"), (0x00000080u, "HotTracked"), (0x00000100u, "Default"),
        (0x00000200u, "Expanded"), (0x00000400u, "Collapsed"), (0x00000800u, "Busy"),
        (0x00001000u, "Floating"), (0x00002000u, "Marqueed"), (0x00004000u, "Animated"),
        (0x00008000u, "Invisible"), (0x00010000u, "Offscreen"), (0x00020000u, "Sizeable"),
        (0x00040000u, "Moveable"), (0x00080000u, "SelfVoicing"), (0x00100000u, "Focusable"),
        (0x00200000u, "Selectable"), (0x00400000u, "Linked"), (0x00800000u, "Traversed"),
        (0x01000000u, "MultiSelectable"), (0x02000000u, "ExtSelectable"),
        (0x04000000u, "AlertLow"), (0x08000000u, "AlertMedium"), (0x10000000u, "AlertHigh"),
        (0x20000000u, "Protected"), (0x40000000u, "HasPopup")
    ];

    private static string MsaaRoleName(uint role) =>
        role < (uint)MsaaRoleNames.Length ? MsaaRoleNames[role] : role.ToString(CultureInfo.InvariantCulture);

    private static string[] MsaaStateNames(uint state)
    {
        var names = new List<string>();
        foreach (var (flag, name) in MsaaStateFlags)
        {
            if ((state & flag) != 0)
            {
                names.Add(name);
            }
        }

        return [.. names];
    }

    /// <summary>
    /// Reads the MSAA state a provider exposes through the LegacyIAccessible bridge.
    /// This is the only structured data many Win32, MFC, and installer windows offer.
    /// </summary>
    private static UiAutomationLegacyAccessiblePatternState? ReadLegacyAccessiblePattern(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationLegacyIAccessiblePattern>(element, UIA_PatternIds.UIA_LegacyIAccessiblePatternId);
        if (pattern is null)
        {
            return null;
        }

        try
        {
            var role = TryRead(() => pattern.CurrentRole, 0u);
            var state = TryRead(() => pattern.CurrentState, 0u);

            return new UiAutomationLegacyAccessiblePatternState
            {
                ChildId = TryRead(() => pattern.CurrentChildId, 0),
                Name = TryRead(() => pattern.CurrentName, string.Empty) ?? string.Empty,
                Value = TryRead(() => pattern.CurrentValue, string.Empty) ?? string.Empty,
                Description = TryRead(() => pattern.CurrentDescription, string.Empty) ?? string.Empty,
                Role = role,
                RoleName = MsaaRoleName(role),
                State = state,
                StateNames = MsaaStateNames(state),
                Help = TryRead(() => pattern.CurrentHelp, string.Empty) ?? string.Empty,
                KeyboardShortcut = TryRead(() => pattern.CurrentKeyboardShortcut, string.Empty) ?? string.Empty,
                DefaultAction = TryRead(() => pattern.CurrentDefaultAction, string.Empty) ?? string.Empty
            };
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static readonly Dictionary<int, string> AnnotationTypeNames = new()
    {
        [60000] = "Unknown",
        [60001] = "SpellingError",
        [60002] = "GrammarError",
        [60003] = "Comment",
        [60004] = "FormulaError",
        [60005] = "TrackChanges",
        [60006] = "Header",
        [60007] = "Footer",
        [60008] = "Highlighted",
        [60009] = "Endnote",
        [60010] = "Footnote",
        [60011] = "InsertionChange",
        [60012] = "DeletionChange",
        [60013] = "MoveChange",
        [60014] = "FormatChange",
        [60015] = "UnsyncedChange",
        [60016] = "EditingLockedChange",
        [60017] = "ExternalChange",
        [60018] = "ConflictingChange",
        [60019] = "Author",
        [60020] = "AdvancedProofingIssue",
        [60021] = "DataValidationError",
        [60022] = "CircularReferenceError",
        [60023] = "Mathematics",
        [60024] = "Sensitive"
    };


    /// <summary>
    /// UI Automation event ids resolved to their programmatic names, so an observed
    /// event is self-describing instead of a bare number.
    /// </summary>
    private static readonly Dictionary<int, string> UiaEventNames = new()
    {
        [UIA_EventIds.UIA_ToolTipOpenedEventId] = "ToolTipOpened",
        [UIA_EventIds.UIA_ToolTipClosedEventId] = "ToolTipClosed",
        [UIA_EventIds.UIA_StructureChangedEventId] = "StructureChanged",
        [UIA_EventIds.UIA_MenuOpenedEventId] = "MenuOpened",
        [UIA_EventIds.UIA_AutomationPropertyChangedEventId] = "AutomationPropertyChanged",
        [UIA_EventIds.UIA_AutomationFocusChangedEventId] = "AutomationFocusChanged",
        [UIA_EventIds.UIA_AsyncContentLoadedEventId] = "AsyncContentLoaded",
        [UIA_EventIds.UIA_MenuClosedEventId] = "MenuClosed",
        [UIA_EventIds.UIA_LayoutInvalidatedEventId] = "LayoutInvalidated",
        [UIA_EventIds.UIA_Invoke_InvokedEventId] = "Invoke_Invoked",
        [UIA_EventIds.UIA_SelectionItem_ElementAddedToSelectionEventId] = "SelectionItem_ElementAddedToSelection",
        [UIA_EventIds.UIA_SelectionItem_ElementRemovedFromSelectionEventId] = "SelectionItem_ElementRemovedFromSelection",
        [UIA_EventIds.UIA_SelectionItem_ElementSelectedEventId] = "SelectionItem_ElementSelected",
        [UIA_EventIds.UIA_Selection_InvalidatedEventId] = "Selection_Invalidated",
        [UIA_EventIds.UIA_Text_TextSelectionChangedEventId] = "Text_TextSelectionChanged",
        [UIA_EventIds.UIA_Text_TextChangedEventId] = "Text_TextChanged",
        [UIA_EventIds.UIA_Window_WindowOpenedEventId] = "Window_WindowOpened",
        [UIA_EventIds.UIA_Window_WindowClosedEventId] = "Window_WindowClosed",
        [UIA_EventIds.UIA_MenuModeStartEventId] = "MenuModeStart",
        [UIA_EventIds.UIA_MenuModeEndEventId] = "MenuModeEnd",
        [UIA_EventIds.UIA_InputReachedTargetEventId] = "InputReachedTarget",
        [UIA_EventIds.UIA_InputReachedOtherElementEventId] = "InputReachedOtherElement",
        [UIA_EventIds.UIA_InputDiscardedEventId] = "InputDiscarded",
        [UIA_EventIds.UIA_SystemAlertEventId] = "SystemAlert",
        [UIA_EventIds.UIA_LiveRegionChangedEventId] = "LiveRegionChanged",
        [UIA_EventIds.UIA_HostedFragmentRootsInvalidatedEventId] = "HostedFragmentRootsInvalidated",
        [UIA_EventIds.UIA_Drag_DragStartEventId] = "Drag_DragStart",
        [UIA_EventIds.UIA_Drag_DragCancelEventId] = "Drag_DragCancel",
        [UIA_EventIds.UIA_Drag_DragCompleteEventId] = "Drag_DragComplete",
        [UIA_EventIds.UIA_DropTarget_DragEnterEventId] = "DropTarget_DragEnter",
        [UIA_EventIds.UIA_DropTarget_DragLeaveEventId] = "DropTarget_DragLeave",
        [UIA_EventIds.UIA_DropTarget_DroppedEventId] = "DropTarget_Dropped",
        [UIA_EventIds.UIA_TextEdit_TextChangedEventId] = "TextEdit_TextChanged",
        [UIA_EventIds.UIA_TextEdit_ConversionTargetChangedEventId] = "TextEdit_ConversionTargetChanged",
        [UIA_EventIds.UIA_ChangesEventId] = "Changes",
        [UIA_EventIds.UIA_NotificationEventId] = "Notification",
        [UIA_EventIds.UIA_ActiveTextPositionChangedEventId] = "ActiveTextPositionChanged"
    };
    private static string UiaEventName(int eventId) =>
        UiaEventNames.TryGetValue(eventId, out var name) ? name : eventId.ToString(CultureInfo.InvariantCulture);

    private const int UiaAnnotationTypesAttributeId = 40031;
    /// <summary>
    /// Upper bound on the number of format runs walked when collecting annotations.
    /// Documents can be arbitrarily long and each step is a cross-process COM call, so
    /// the walk is capped rather than allowed to run to completion.
    /// </summary>
    private const int AnnotationRunLimit = 400;

    /// <summary>
    /// UI Automation subscribes text-edit handlers per change type and offers no "any" value,
    /// so a caller waiting for text edits is registered against every concrete type.
    /// </summary>
    private static readonly TextEditChangeType[] TextEditChangeTypes =
    [
        TextEditChangeType.TextEditChangeType_AutoCorrect,
        TextEditChangeType.TextEditChangeType_Composition,
        TextEditChangeType.TextEditChangeType_CompositionFinalized,
        TextEditChangeType.TextEditChangeType_AutoComplete
    ];

    /// <summary>
    /// Character offset of <paramref name="target"/>'s start within
    /// <paramref name="documentRange"/>. UI Automation exposes no offset API, so the
    /// distance is measured by cloning the document range, pulling its end back to the
    /// target's start, and counting the resulting text.
    /// </summary>
    private static int ComputeOffset(IUIAutomationTextRange? documentRange, IUIAutomationTextRange? target)
    {
        if (documentRange is null || target is null)
        {
            return -1;
        }

        IUIAutomationTextRange? probe = null;
        try
        {
            probe = documentRange.Clone();
            probe.MoveEndpointByRange(
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
                target,
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);
            return probe.GetText(-1)?.Length ?? -1;
        }
        catch (Exception ex) when (ex is COMException or ArgumentException)
        {
            return -1;
        }
        finally
        {
            FinalRelease(probe);
        }
    }

    private static UiAutomationTextCaretInfo? ReadCaret(IUIAutomationTextPattern2? pattern, IUIAutomationTextRange? documentRange)
    {
        if (pattern is null)
        {
            return null;
        }

        IUIAutomationTextRange? caret = null;
        IUIAutomationTextRange? line = null;
        try
        {
            caret = pattern.GetCaretRange(out int isActive);
            if (caret is null)
            {
                return null;
            }

            var offset = ComputeOffset(documentRange, caret);

            var lineText = string.Empty;
            try
            {
                line = caret.Clone();
                line.ExpandToEnclosingUnit(TextUnit.TextUnit_Line);
                lineText = line.GetText(-1) ?? string.Empty;
            }
            catch (Exception ex) when (ex is COMException or ArgumentException)
            {
                lineText = string.Empty;
            }

            return new UiAutomationTextCaretInfo
            {
                IsActive = isActive != 0,
                Offset = offset,
                LineText = lineText
            };
        }
        catch (Exception ex) when (ex is COMException or ArgumentException)
        {
            return null;
        }
        finally
        {
            FinalRelease(line);
            FinalRelease(caret);
        }
    }

    /// <summary>
    /// Collects annotated runs by walking the document one format unit at a time.
    /// Annotations follow formatting boundaries in every provider that reports them, and
    /// UI Automation offers no way to enumerate them directly.
    /// </summary>
    private static UiAutomationTextAnnotation[] ReadAnnotations(IUIAutomationTextRange? documentRange)
    {
        if (documentRange is null)
        {
            return [];
        }

        var annotations = new List<UiAutomationTextAnnotation>();
        IUIAutomationTextRange? cursor = null;

        try
        {
            cursor = documentRange.Clone();
            cursor.MoveEndpointByRange(
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
                documentRange,
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);

            for (var step = 0; step < AnnotationRunLimit; step++)
            {
                cursor.ExpandToEnclosingUnit(TextUnit.TextUnit_Format);

                if (cursor.CompareEndpoints(
                        TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start,
                        documentRange,
                        TextPatternRangeEndpoint.TextPatternRangeEndpoint_End) >= 0)
                {
                    break;
                }

                var text = cursor.GetText(-1) ?? string.Empty;
                foreach (var typeId in ReadAnnotationTypeIds(cursor))
                {
                    annotations.Add(new UiAutomationTextAnnotation
                    {
                        TypeId = typeId,
                        TypeName = AnnotationTypeNames.TryGetValue(typeId, out var name)
                            ? name
                            : typeId.ToString(CultureInfo.InvariantCulture),
                        StartOffset = ComputeOffset(documentRange, cursor),
                        Length = text.Length,
                        Text = text
                    });
                }

                // Collapse to the end of this run so the next expansion picks up the next one.
                var moved = cursor.MoveEndpointByUnit(
                    TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start,
                    TextUnit.TextUnit_Format,
                    1);
                if (moved == 0)
                {
                    break;
                }

                cursor.MoveEndpointByRange(
                    TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
                    cursor,
                    TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);
            }
        }
        catch (Exception ex) when (ex is COMException or ArgumentException)
        {
            // A provider that cannot walk format units simply reports no annotations.
        }
        finally
        {
            FinalRelease(cursor);
        }

        return [.. annotations];
    }

    private static int[] ReadAnnotationTypeIds(IUIAutomationTextRange range)
    {
        object? value;
        try
        {
            value = range.GetAttributeValue(UiaAnnotationTypesAttributeId);
        }
        catch (Exception ex) when (ex is COMException or ArgumentException)
        {
            return [];
        }

        // Mixed or unsupported attributes come back as a sentinel object rather than an array.
        if (value is not Array array)
        {
            return [];
        }

        var ids = new List<int>();
        foreach (var entry in array)
        {
            if (entry is null)
            {
                continue;
            }

            try
            {
                var id = Convert.ToInt32(entry, CultureInfo.InvariantCulture);

                // AnnotationType_Unknown carries no information and shows up on plain runs.
                if (id != 60000 && !ids.Contains(id))
                {
                    ids.Add(id);
                }
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
            {
                // Ignore entries that are not annotation ids.
            }
        }

        return [.. ids];
    }

    private static UiAutomationTextChildInfo? ReadTextChild(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationTextChildPattern>(element, UIA_PatternIds.UIA_TextChildPatternId);
        if (pattern is null)
        {
            return null;
        }

        IUIAutomationElement? container = null;
        IUIAutomationTextRange? range = null;
        IUIAutomationTextPattern? containerText = null;
        IUIAutomationTextRange? containerRange = null;

        try
        {
            container = TryRead(() => pattern.TextContainer, null);
            range = TryRead(() => pattern.TextRange, null);

            var offset = -1;
            if (container is not null && range is not null)
            {
                containerText = GetPattern<IUIAutomationTextPattern>(container, UIA_PatternIds.UIA_TextPatternId);
                if (containerText is not null)
                {
                    containerRange = TryRead(() => containerText.DocumentRange, null);
                    offset = ComputeOffset(containerRange, range);
                }
            }

            return new UiAutomationTextChildInfo
            {
                Container = container is null ? null : ToElementReference(container),
                RangeText = range is null ? string.Empty : TryRead(() => range.GetText(-1), string.Empty) ?? string.Empty,
                StartOffset = offset
            };
        }
        finally
        {
            FinalRelease(containerRange);
            FinalRelease(containerText);
            FinalRelease(range);
            FinalRelease(container);
            FinalRelease(pattern);
        }
    }

    /// <summary>
    /// Searches the document for <paramref name="needle"/> and reports the match by
    /// offset, so a later independent call can act on it.
    /// </summary>
    /// <remarks>
    /// Text ranges are live COM objects that cannot survive between CLI or MCP
    /// invocations, which is why nothing here returns one. Offsets are the portable
    /// address, and every verb that acts on text rebuilds its range from them.
    /// </remarks>
    private static UiAutomationTextFindResult FindTextRun(
        IUIAutomationTextRange? documentRange,
        string needle,
        bool matchCase = false,
        bool backward = false)
    {
        if (documentRange is null || string.IsNullOrEmpty(needle))
        {
            return new UiAutomationTextFindResult { Found = false, Needle = needle };
        }

        IUIAutomationTextRange? match = null;
        try
        {
            // FindText takes (text, backward, ignoreCase) as ints. Case-insensitive
            // forward is the default because a caller is usually locating text they
            // read off a screen and does not care about case; both are overridable
            // for the cases where it matters, such as telling ERROR from Error, or
            // wanting the last occurrence rather than the first.
            match = documentRange.FindText(needle, backward ? 1 : 0, matchCase ? 0 : 1);
            if (match is null)
            {
                return new UiAutomationTextFindResult { Found = false, Needle = needle };
            }

            var text = TryRead(() => match.GetText(-1), null);
            return new UiAutomationTextFindResult
            {
                Found = true,
                Needle = needle,
                StartOffset = ComputeOffset(documentRange, match),
                Length = text?.Length,
                Text = text,
                BoundingRectangles = ReadBoundingRectangles(match)
            };
        }
        catch (Exception ex) when (ex is COMException or NotSupportedException)
        {
            // Advertising the Text pattern does not oblige a provider to implement
            // FindText; several bridged and custom providers raise E_NOTIMPL, which
            // surfaces here as NotSupportedException rather than COMException.
            // A search that cannot run is reported as a miss, not as a crash that
            // takes the whole text read with it.
            return new UiAutomationTextFindResult { Found = false, Needle = needle };
        }
        finally
        {
            FinalRelease(match);
        }
    }

    /// <summary>
    /// Reads a range's screen rectangles. A range that wraps across lines reports
    /// one rectangle per line; an off-screen range reports none.
    /// </summary>
    private static List<UiAutomationRect> ReadBoundingRectangles(IUIAutomationTextRange range)
    {
        try
        {
            if (range.GetBoundingRectangles() is not double[] values || values.Length < 4)
            {
                return [];
            }

            // The provider returns a flat [left, top, width, height, ...] array.
            var results = new List<UiAutomationRect>(values.Length / 4);
            for (var i = 0; i + 3 < values.Length; i += 4)
            {
                results.Add(new UiAutomationRect
                {
                    Left = (int)values[i],
                    Top = (int)values[i + 1],
                    Right = (int)(values[i] + values[i + 2]),
                    Bottom = (int)(values[i + 1] + values[i + 3])
                });
            }

            return results;
        }
        catch (COMException)
        {
            return [];
        }
    }

    /// <summary>
    /// Rebuilds a text range from an offset and length against the document range.
    /// The caller owns the returned range and must release it.
    /// </summary>
    private static IUIAutomationTextRange? BuildRangeFromOffset(IUIAutomationTextRange documentRange, int startOffset, int length)
    {
        IUIAutomationTextRange? range = null;
        try
        {
            range = documentRange.Clone();
            if (range is null)
            {
                return null;
            }

            // Collapse to the document start, then walk both endpoints forward.
            // Endpoint moves clamp at the document boundary, so an offset or length
            // past the end yields an empty or truncated range rather than an error -
            // which is the right behaviour for a caller who cannot see the document.
            range.MoveEndpointByRange(
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
                documentRange,
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);

            if (startOffset > 0)
            {
                range.MoveEndpointByUnit(TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start, TextUnit.TextUnit_Character, startOffset);
                range.MoveEndpointByRange(
                    TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
                    range,
                    TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);
            }

            if (length > 0)
            {
                range.MoveEndpointByUnit(TextPatternRangeEndpoint.TextPatternRangeEndpoint_End, TextUnit.TextUnit_Character, length);
            }

            var result = range;
            range = null;
            return result;
        }
        catch (Exception ex) when (ex is COMException or NotSupportedException)
        {
            return null;
        }
        finally
        {
            FinalRelease(range);
        }
    }

    /// <summary>
    /// Runs a text-range operation, translating a provider that does not implement
    /// it into an explanation rather than a raw COM message.
    /// </summary>
    /// <remarks>
    /// Supporting the Text pattern obliges a provider to return text; it does not
    /// oblige it to implement <c>Clone</c>, the endpoint moves, <c>Select</c> or
    /// <c>ScrollIntoView</c>. Several bridged and read-only providers raise
    /// E_NOTIMPL for those, which reaches managed code as
    /// <see cref="NotSupportedException"/> and would otherwise surface to the user
    /// as the unactionable "Specified method is not supported".
    /// </remarks>
    private static string InvokeTextRangeOperation(Func<string> operation)
    {
        try
        {
            return operation();
        }
        catch (NotSupportedException)
        {
            throw new InvalidOperationException(
                "This provider exposes text but does not support manipulating text ranges. "
                + "Reading with the 'text' command still works.");
        }
    }

    /// <summary>
    /// Resolves the range a text verb should act on, from either a search string or
    /// an explicit offset and length. The caller owns the returned range.
    /// </summary>
    private static IUIAutomationTextRange ResolveTextRange(
        IUIAutomationElement element,
        string? needle,
        int? startOffset,
        double? length,
        bool matchCase = false,
        bool backward = false)
    {
        IUIAutomationTextPattern? pattern = null;
        IUIAutomationTextRange? documentRange = null;
        try
        {
            pattern = GetPattern<IUIAutomationTextPattern>(element, UIA_PatternIds.UIA_TextPatternId)
                ?? throw new InvalidOperationException("Element does not support the Text pattern.");
            documentRange = pattern.DocumentRange
                ?? throw new InvalidOperationException("The Text provider returned no document range.");

            if (!string.IsNullOrEmpty(needle))
            {
                IUIAutomationTextRange? match;
                try
                {
                    match = documentRange.FindText(needle, backward ? 1 : 0, matchCase ? 0 : 1);
                }
                catch (Exception ex) when (ex is COMException or NotSupportedException)
                {
                    // Advertising Text does not oblige a provider to implement
                    // FindText. Say so, rather than letting E_NOTIMPL surface as
                    // "Specified method is not supported".
                    throw new InvalidOperationException(
                        "This provider supports the Text pattern but not text search. "
                        + "Address the range with --int <startOffset> and --number <length> instead.");
                }

                return match ?? throw new InvalidOperationException(
                    matchCase
                        ? $"The text \"{needle}\" was not found in this element (case-sensitive search)."
                        : $"The text \"{needle}\" was not found in this element.");
            }

            if (startOffset is null)
            {
                throw new InvalidOperationException(
                    "A text range is required. Pass the text to act on, or --int <startOffset> with an optional --number <length>.");
            }

            return BuildRangeFromOffset(documentRange, Math.Max(0, startOffset.Value), (int)Math.Max(0, length ?? 0))
                ?? throw new InvalidOperationException("Could not build a text range at that offset.");
        }
        finally
        {
            FinalRelease(documentRange);
            FinalRelease(pattern);
        }
    }

    private static string PerformSelectText(IUIAutomationElement element, string? needle, int? startOffset, double? length, bool matchCase, bool backward)
    {
        IUIAutomationTextRange? range = null;
        try
        {
            range = ResolveTextRange(element, needle, startOffset, length, matchCase, backward);
            range.Select();
            return needle is null
                ? FormattableString.Invariant($"Selected text at offset {startOffset}.")
                : $"Selected \"{needle}\".";
        }
        finally
        {
            FinalRelease(range);
        }
    }

    private static string PerformMoveCaret(IUIAutomationElement element, string? needle, int? startOffset, bool matchCase, bool backward)
    {
        IUIAutomationTextRange? range = null;
        try
        {
            // A degenerate range - start and end at the same point - is how UIA
            // expresses a caret position; selecting it moves the caret without
            // selecting anything.
            range = ResolveTextRange(element, needle, startOffset, 0, matchCase, backward);
            range.MoveEndpointByRange(
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
                range,
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);
            range.Select();
            return needle is null
                ? FormattableString.Invariant($"Moved caret to offset {startOffset}.")
                : $"Moved caret to \"{needle}\".";
        }
        finally
        {
            FinalRelease(range);
        }
    }

    private static string PerformScrollTextIntoView(IUIAutomationElement element, string? needle, int? startOffset, double? length, bool matchCase, bool backward)
    {
        IUIAutomationTextRange? range = null;
        try
        {
            range = ResolveTextRange(element, needle, startOffset, length, matchCase, backward);
            range.ScrollIntoView(1);
            return needle is null
                ? FormattableString.Invariant($"Scrolled offset {startOffset} into view.")
                : $"Scrolled \"{needle}\" into view.";
        }
        finally
        {
            FinalRelease(range);
        }
    }

    private static UiAutomationTextEditInfo? ReadTextEdit(IUIAutomationTextEditPattern? pattern)
    {
        if (pattern is null)
        {
            return null;
        }

        IUIAutomationTextRange? composition = null;
        IUIAutomationTextRange? conversion = null;
        try
        {
            composition = TryRead(() => pattern.GetActiveComposition(), null);
            conversion = TryRead(() => pattern.GetConversionTarget(), null);

            return new UiAutomationTextEditInfo
            {
                ActiveComposition = composition is null ? string.Empty : TryRead(() => composition.GetText(-1), string.Empty) ?? string.Empty,
                ConversionTarget = conversion is null ? string.Empty : TryRead(() => conversion.GetText(-1), string.Empty) ?? string.Empty
            };
        }
        finally
        {
            FinalRelease(conversion);
            FinalRelease(composition);
        }
    }

    private static UiAutomationVirtualizationInfo? ReadVirtualization(UiAutomationPatternInfo[] supportedPatterns)
    {
        var isItemContainer = false;
        var isVirtualizedItem = false;

        foreach (var pattern in supportedPatterns)
        {
            if (pattern.Id == UIA_PatternIds.UIA_ItemContainerPatternId)
            {
                isItemContainer = true;
            }
            else if (pattern.Id == UIA_PatternIds.UIA_VirtualizedItemPatternId)
            {
                isVirtualizedItem = true;
            }
        }

        return isItemContainer || isVirtualizedItem
            ? new UiAutomationVirtualizationInfo { IsItemContainer = isItemContainer, IsVirtualizedItem = isVirtualizedItem }
            : null;
    }

    private static List<UiAutomationElementInfo> ReadElementArray(IUIAutomation automation, IUIAutomationElementArray? elements)
    {
        if (elements is null)
        {
            return [];
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

    private static List<UiAutomationElementReference> ReadElementReferenceArray(Func<IUIAutomationElementArray?> getter)
    {
        IUIAutomationElementArray? array = null;
        try
        {
            array = getter();
            if (array is null)
            {
                return [];
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
            return [];
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
        BoundingRectangle = TryRead(() => ToRect(element.CurrentBoundingRectangle), null)
    };

    private static TPattern? GetPattern<TPattern>(IUIAutomationElement element, int patternId)
        where TPattern : class
    {
        try
        {
            return element.GetCurrentPattern(patternId) as TPattern;
        }
        catch (Exception ex) when (ex is COMException or ArgumentException)
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
        var pattern = GetPattern<IUIAutomationInvokePattern>(element, UIA_PatternIds.UIA_InvokePatternId);
        if (pattern is null)
        {
            var hasLegacy = GetPattern<IUIAutomationLegacyIAccessiblePattern>(element, UIA_PatternIds.UIA_LegacyIAccessiblePatternId);
            if (hasLegacy is not null)
            {
                FinalRelease(hasLegacy);
                throw new InvalidOperationException(
                    "Element does not support the Invoke pattern. It is MSAA-bridged, so try the 'default-action' action.");
            }

            throw new InvalidOperationException("Element does not support the Invoke pattern.");
        }

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

    private static string PerformDefaultAction(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationLegacyIAccessiblePattern>(element, UIA_PatternIds.UIA_LegacyIAccessiblePatternId)
            ?? throw new InvalidOperationException("Element does not support the LegacyIAccessible pattern.");

        try
        {
            var description = TryRead(() => pattern.CurrentDefaultAction, string.Empty);
            pattern.DoDefaultAction();
            return string.IsNullOrEmpty(description)
                ? "Performed the default action."
                : $"Performed the default action '{description}'.";
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private static string PerformSetValue(IUIAutomationElement element, string? value)
    {
        var pattern = GetPattern<IUIAutomationValuePattern>(element, UIA_PatternIds.UIA_ValuePatternId);
        if (pattern is null)
        {
            // MSAA-bridged controls often expose no Value pattern but can still be
            // written through the legacy interface.
            return PerformLegacySetValue(element, value);
        }

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

    private static string PerformLegacySetValue(IUIAutomationElement element, string? value)
    {
        var legacy = GetPattern<IUIAutomationLegacyIAccessiblePattern>(element, UIA_PatternIds.UIA_LegacyIAccessiblePatternId)
            ?? throw new InvalidOperationException("Element does not support the Value pattern.");

        try
        {
            legacy.SetValue(value ?? string.Empty);
            return "Value updated through the LegacyIAccessible pattern.";
        }
        finally
        {
            FinalRelease(legacy);
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
            AssertTransformCapability(pattern, TransformCapability.Move);
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
            AssertTransformCapability(pattern, TransformCapability.Resize);
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

    private static string PerformRotate(IUIAutomationElement element, double? degrees)
    {
        var pattern = GetPattern<IUIAutomationTransformPattern>(element, UIA_PatternIds.UIA_TransformPatternId)
            ?? throw new InvalidOperationException("Element does not support the Transform pattern.");

        try
        {
            AssertTransformCapability(pattern, TransformCapability.Rotate);
            pattern.Rotate(degrees ?? throw new InvalidOperationException("The rotate action requires a number of degrees."));
            return FormattableString.Invariant($"Rotated element by {degrees} degrees.");
        }
        finally
        {
            FinalRelease(pattern);
        }
    }

    private enum TransformCapability
    {
        Move,
        Resize,
        Rotate
    }

    /// <summary>
    /// Fails with the specific capability that is false rather than letting the
    /// call reach COM and come back as an opaque provider error. A window that
    /// cannot be resized advertises Transform all the same.
    /// </summary>
    private static void AssertTransformCapability(IUIAutomationTransformPattern pattern, TransformCapability capability)
    {
        var (supported, verb) = capability switch
        {
            TransformCapability.Move => (TryRead(() => pattern.CurrentCanMove != 0, true), "moved"),
            TransformCapability.Resize => (TryRead(() => pattern.CurrentCanResize != 0, true), "resized"),
            TransformCapability.Rotate => (TryRead(() => pattern.CurrentCanRotate != 0, true), "rotated"),
            _ => (true, string.Empty)
        };

        if (!supported)
        {
            throw new InvalidOperationException(
                $"The element supports the Transform pattern but reports that it cannot be {verb}. " +
                "Check transformPattern on the element before retrying.");
        }
    }

    private static string PerformScrollIntoView(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationScrollItemPattern>(element, UIA_PatternIds.UIA_ScrollItemPatternId)
            ?? throw new InvalidOperationException(
                "Element does not support the ScrollItem pattern. If the item is virtualized, try 'realize' first.");

        try
        {
            pattern.ScrollIntoView();
            return "Scrolled element into view.";
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
            var supportedViews = TryRead(() => pattern.GetCurrentSupportedViews(), []) ?? [];
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

    private static string PerformRealize(IUIAutomationElement element)
    {
        var pattern = GetPattern<IUIAutomationVirtualizedItemPattern>(element, UIA_PatternIds.UIA_VirtualizedItemPatternId)
            ?? throw new InvalidOperationException("Element does not support the VirtualizedItem pattern.");

        try
        {
            pattern.Realize();
            return "Element realized.";
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

    /// <summary>
    /// Reads an optional property, falling back rather than throwing.
    /// </summary>
    /// <remarks>
    /// The catch list is deliberately wider than <see cref="COMException"/>.
    /// A provider that does not implement a property raises E_NOTIMPL, which the
    /// runtime callable wrapper surfaces as <see cref="NotSupportedException"/>
    /// rather than as a COM exception; an element destroyed between acquisition
    /// and read can produce <see cref="InvalidCastException"/> from the marshaller;
    /// and a released proxy produces
    /// <see cref="InvalidComObjectException"/>.
    ///
    /// All three mean the same thing to a caller - this value is unavailable -
    /// and none should abort the surrounding read. An event handler in particular
    /// receives a sender that may already be gone by the time it runs.
    /// </remarks>
    private static TValue TryRead<TValue>(Func<TValue> getter, TValue fallback)
    {
        try
        {
            return getter();
        }
        catch (Exception ex) when (ex is COMException
                                      or NotSupportedException
                                      or InvalidComObjectException
                                      or InvalidCastException
                                      or UnauthorizedAccessException)
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
    private sealed class FocusChangedEventHandler : OneShotEventHandler, IUIAutomationFocusChangedEventHandler
    {

        public void HandleFocusChangedEvent(IUIAutomationElement sender)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            WaitHandle.Set();
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "focus",
                    TimedOut = timedOut,
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class AutomationEventHandler : OneShotEventHandler, IUIAutomationEventHandler
    {

        public int? EventId { get; private set; }

        public void HandleAutomationEvent(IUIAutomationElement sender, int eventId)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            EventId = eventId;
            WaitHandle.Set();
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "automation",
                    TimedOut = timedOut,
                    EventId = EventId,
                    EventName = EventId is null ? null : UiaEventName(EventId.Value),
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class TextEditEventHandler : OneShotEventHandler, IUIAutomationTextEditTextChangedEventHandler
    {

        public TextEditChangeType ChangeType { get; private set; }

        public string[] EventStrings { get; private set; } = [];

        public void HandleTextEditTextChangedEvent(IUIAutomationElement sender, TextEditChangeType textEditChangeType, string[] eventStrings)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            ChangeType = textEditChangeType;
            EventStrings = eventStrings ?? [];
            WaitHandle.Set();
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "text-edit",
                    TimedOut = timedOut,
                    TextEditChangeType = (int)ChangeType,
                    TextEditChangeTypeName = ChangeType.ToString(),
                    EventStrings = EventStrings,
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class NotificationEventHandler : OneShotEventHandler, IUIAutomationNotificationEventHandler
    {

        public NotificationKind Kind { get; private set; }

        public NotificationProcessing Processing { get; private set; }

        public string DisplayString { get; private set; } = string.Empty;

        public string ActivityId { get; private set; } = string.Empty;

        public void HandleNotificationEvent(
            IUIAutomationElement sender,
            NotificationKind notificationKind,
            NotificationProcessing notificationProcessing,
            string displayString,
            string activityId)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            Kind = notificationKind;
            Processing = notificationProcessing;
            DisplayString = displayString ?? string.Empty;
            ActivityId = activityId ?? string.Empty;
            WaitHandle.Set();
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                // Nothing arrived, so report no payload rather than the zero values
                // of the enums - NotificationKind 0 is ItemAdded, which would read as
                // a real notification that never happened.
                if (captured is null)
                {
                    return new UiAutomationEventResult
                    {
                        EventKind = "notification",
                        TimedOut = timedOut
                    };
                }

                return new UiAutomationEventResult
                {
                    EventKind = "notification",
                    TimedOut = timedOut,
                    NotificationKind = (int)Kind,
                    NotificationKindName = Kind.ToString(),
                    NotificationProcessing = (int)Processing,
                    NotificationProcessingName = Processing.ToString(),
                    DisplayString = DisplayString,
                    ActivityId = ActivityId,
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class ChangesEventHandler : OneShotEventHandler, IUIAutomationChangesEventHandler
    {

        public int ChangeId { get; private set; }

        public string? Payload { get; private set; }

        public int ChangeCount { get; private set; }

        public void HandleChangesEvent(IUIAutomationElement sender, ref UiaChangeInfo uiaChanges, int changesCount)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            // The interop signature is `ref UiaChangeInfo` plus a count rather than
            // an array, so only the first entry is reachable without unsafe pointer
            // arithmetic. changesCount is reported so a caller can tell that more
            // changes were coalesced into the same notification.
            ChangeId = uiaChanges.uiaId;
            Payload = uiaChanges.payload?.ToString();
            ChangeCount = changesCount;
            WaitHandle.Set();
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                if (captured is null)
                {
                    return new UiAutomationEventResult { EventKind = "changes", TimedOut = timedOut };
                }

                return new UiAutomationEventResult
                {
                    EventKind = "changes",
                    TimedOut = timedOut,
                    ChangeId = ChangeId,
                    ChangePayload = Payload,
                    ChangeCount = ChangeCount,
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class ActiveTextPositionEventHandler : OneShotEventHandler, IUIAutomationActiveTextPositionChangedEventHandler
    {

        public string? RangeText { get; private set; }

        public int? RangeOffset { get; private set; }

        public void HandleActiveTextPositionChangedEvent(IUIAutomationElement sender, IUIAutomationTextRange range)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            try
            {
                RangeText = TryRead(() => range?.GetText(-1), null);
                RangeOffset = ComputeRangeOffset(sender, range);
            }
            finally
            {
                FinalRelease(range);
            }

            WaitHandle.Set();
            FinalRelease(range);
        }

        /// <summary>
        /// Derives a document offset for the reported range, reusing the same
        /// clone-and-move technique the text reader uses. IUIAutomationTextRange
        /// exposes no offset property, so this is the only way to get one.
        /// </summary>
        private static int? ComputeRangeOffset(IUIAutomationElement element, IUIAutomationTextRange? range)
        {
            if (range is null)
            {
                return null;
            }

            IUIAutomationTextPattern? textPattern = null;
            IUIAutomationTextRange? documentRange = null;
            try
            {
                textPattern = GetPattern<IUIAutomationTextPattern>(element, UIA_PatternIds.UIA_TextPatternId);
                documentRange = textPattern?.DocumentRange;
                return documentRange is null ? null : ComputeOffset(documentRange, range);
            }
            catch (COMException)
            {
                return null;
            }
            finally
            {
                FinalRelease(documentRange);
                FinalRelease(textPattern);
            }
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                if (captured is null)
                {
                    return new UiAutomationEventResult { EventKind = "active-text-position", TimedOut = timedOut };
                }

                return new UiAutomationEventResult
                {
                    EventKind = "active-text-position",
                    TimedOut = timedOut,
                    TextRangeText = RangeText,
                    TextRangeOffset = RangeOffset,
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class PropertyChangedEventHandler : OneShotEventHandler, IUIAutomationPropertyChangedEventHandler
    {

        public int? PropertyId { get; private set; }

        public object? Value { get; private set; }

        public void HandlePropertyChangedEvent(IUIAutomationElement sender, int propertyId, object newValue)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            PropertyId = propertyId;
            Value = newValue;
            WaitHandle.Set();
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "property",
                    TimedOut = timedOut,
                    PropertyId = PropertyId,
                    Value = Value,
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }

    /// <summary>
    /// Shared state for the one-shot event handlers.
    /// </summary>
    /// <remarks>
    /// The handler callback runs on a UI Automation thread while
    /// <c>ToResult</c> runs on this call's STA thread, so the captured sender is
    /// touched by two threads. Guarding it matters: structure-changed events on a
    /// busy desktop arrive in bursts, and an unsynchronised
    /// check-then-use produced an intermittent NullReferenceException that took
    /// the whole wait down.
    ///
    /// The exchange is lock-free and "first event wins": a later callback loses
    /// the race and releases its own sender rather than overwriting the captured
    /// one, and <c>TakeSender</c> hands ownership to the reader exactly once.
    /// </remarks>
    private abstract class OneShotEventHandler : IDisposable
    {
        private IUIAutomationElement? sender;

        /// <summary>Signalled once the first matching event has been captured.</summary>
        public AutoResetEvent WaitHandle { get; } = new(false);

        /// <summary>
        /// Records the first sender seen. Returns true when the caller won and
        /// should populate the payload; false when it must release its sender.
        /// </summary>
        protected bool TryCapture(IUIAutomationElement candidate)
        {
            if (Interlocked.CompareExchange(ref sender, candidate, null) is null)
            {
                return true;
            }

            FinalRelease(candidate);
            return false;
        }

        /// <summary>Takes ownership of the captured sender, leaving none behind.</summary>
        protected IUIAutomationElement? TakeSender() => Interlocked.Exchange(ref sender, null);

        /// <summary>
        /// Projects an event's sender, degrading to null rather than failing.
        /// </summary>
        /// <remarks>
        /// The sender arrives on a UI Automation callback thread and is read on
        /// this call's STA thread, so it crosses an apartment boundary and may
        /// refer to an element that has already been destroyed - especially after
        /// a timeout, where a late callback can land while the result is being
        /// built. Neither is a defect in the caller's request.
        ///
        /// An unreadable sender is reported as no sender, which is accurate and
        /// leaves the rest of the event payload intact. Letting it propagate would
        /// fail the whole wait over a detail, and did: structure-changed events on
        /// a busy desktop produced an intermittent NullReferenceException here.
        /// </remarks>
        protected static UiAutomationElementInfo? ReadSenderSafely(IUIAutomation automation, IUIAutomationElement? element)
        {
            if (element is null)
            {
                return null;
            }

            try
            {
                return ReadElementInfo(automation, element);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            FinalRelease(TakeSender());
            WaitHandle.Dispose();
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class StructureChangedEventHandler : OneShotEventHandler, IUIAutomationStructureChangedEventHandler
    {

        public StructureChangeType? StructureChangeType { get; private set; }

        public int[]? RuntimeId { get; private set; }

        public void HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
        {
            if (!TryCapture(sender))
            {
                return;
            }

            StructureChangeType = changeType;
            RuntimeId = runtimeId;
            WaitHandle.Set();
        }

        public UiAutomationEventResult ToResult(IUIAutomation automation, bool timedOut)
        {
            var captured = TakeSender();
            try
            {
                return new UiAutomationEventResult
                {
                    EventKind = "structure",
                    TimedOut = timedOut,
                    StructureChangeType = (int?)StructureChangeType,
                    StructureChangeTypeName = StructureChangeType?.ToString(),
                    Value = RuntimeId,
                    SourceElement = ReadSenderSafely(automation, captured)
                };
            }
            finally
            {
                FinalRelease(captured);
            }
        }

    }
}
