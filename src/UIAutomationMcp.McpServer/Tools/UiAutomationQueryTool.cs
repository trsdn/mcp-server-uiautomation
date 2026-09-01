using ModelContextProtocol.Server;
using UIAutomationMcp.ComInterop;
using UIAutomationMcp.Service;

namespace UIAutomationMcp.McpServer.Tools;

[McpServerToolType]
public sealed class UiAutomationQueryTool(UiAutomationService service) : UiAutomationToolsBase
{
    [McpServerTool(Name = "uia_desktop")]
    public string Desktop() => Execute(service.ProbeDesktop);

    [McpServerTool(Name = "uia_snapshot")]
    public string Snapshot() => Execute(service.CaptureSnapshot);

    [McpServerTool(Name = "uia_focused")]
    public string Focused() => Execute(service.GetFocusedElement);

    [McpServerTool(Name = "uia_handle")]
    public string FromHandle(long handle) => Execute(() => service.GetElementFromHandle(new nint(handle)));

    [McpServerTool(Name = "uia_point")]
    public string FromPoint(int x, int y) => Execute(() => service.GetElementFromPoint(x, y));

    [McpServerTool(Name = "uia_find_name")]
    public string FindByName(string name) => Execute(() => service.FindFirstByName(name));

    [McpServerTool(Name = "uia_find_class")]
    public string FindByClass(string className) => Execute(() => service.FindFirstByClassName(className));

    [McpServerTool(Name = "uia_find_automation_id")]
    public string FindByAutomationId(string automationId) => Execute(() => service.FindFirstByAutomationId(automationId));

    [McpServerTool(Name = "uia_inspect")]
    public string Inspect(
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        string? notName = null,
        string? notClassName = null,
        string? notAutomationId = null,
        int? notControlType = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree",
        bool realizeVirtualized = true,
        bool tryInspect = false)
    {
        var locator = CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView, realizeVirtualized, notName, notClassName, notAutomationId, notControlType);
        return tryInspect
            ? Execute(() => service.TryInspect(locator))
            : Execute(() => service.Inspect(locator));
    }

    [McpServerTool(Name = "uia_find")]
    public string Find(
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        string? notName = null,
        string? notClassName = null,
        string? notAutomationId = null,
        int? notControlType = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree",
        int maxResults = 50) =>
        Execute(() => service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = root,
            FocusedElement = focused,
            SearchFromFocused = fromFocused,
            WindowHandle = handle,
            PointX = x,
            PointY = y,
            Name = name,
            ClassName = className,
            AutomationId = automationId,
            FrameworkId = frameworkId,
            ControlType = controlType,
            ProcessId = processId,
            NotName = notName,
            NotClassName = notClassName,
            NotAutomationId = notAutomationId,
            NotControlType = notControlType,
            Scope = scope,
            MaxResults = maxResults,
            CacheRequest = CreateCacheRequest(cache, cacheScope, cacheView)
        }));

    [McpServerTool(Name = "uia_children")]
    public string Children(
        string view = "control",
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree",
        int maxResults = 50) =>
        Execute(() => service.ListChildren(
            CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView),
            view,
            maxResults));

    [McpServerTool(Name = "uia_descendants")]
    public string Descendants(
        string view = "control",
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree",
        int maxResults = 50) =>
        Execute(() => service.ListDescendants(
            CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView),
            view,
            maxResults));

    [McpServerTool(Name = "uia_navigate")]
    public string Navigate(
        string direction,
        string view = "control",
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree") =>
        Execute(() => service.Navigate(CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView), direction, view));

    [McpServerTool(Name = "uia_text")]
    public string Text(
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree",
        string? find = null) =>
        Execute(() => service.ReadText(CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView), find));

    [McpServerTool(Name = "uia_selection")]
    public string Selection(
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree") =>
        Execute(() => service.ReadSelection(CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView)));

    [McpServerTool(Name = "uia_table")]
    public string Table(
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree",
        int maxRows = 50,
        int maxColumns = 25) =>
        Execute(() => service.ReadTable(
            CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView),
            maxRows,
            maxColumns));

    [McpServerTool(Name = "uia_wait_event")]
    public string WaitEvent(
        string eventKind,
        int timeoutMs = 5000,
        int? eventId = null,
        int? propertyId = null,
        int? changeId = null,
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        bool cache = false,
        string cacheScope = "subtree",
        string cacheView = "control",
        string scope = "subtree") =>
        Execute(() => service.WaitForEvent(new UiAutomationEventWaitRequest
        {
            EventKind = eventKind,
            TimeoutMs = timeoutMs,
            EventId = eventId,
            PropertyId = propertyId,
            ChangeId = changeId,
            Locator = CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, cache, cacheScope, cacheView),
            CacheRequest = CreateCacheRequest(cache, cacheScope, cacheView)
        }));

    [McpServerTool(Name = "uia_action")]
    public string Action(
        string action,
        string? value = null,
        string? secondValue = null,
        double? number = null,
        double? secondNumber = null,
        int? intValue = null,
        bool root = false,
        bool focused = false,
        bool fromFocused = false,
        long? handle = null,
        int? x = null,
        int? y = null,
        string? name = null,
        string? className = null,
        string? automationId = null,
        string? frameworkId = null,
        int? controlType = null,
        int? processId = null,
        string scope = "subtree",
        bool realizeVirtualized = true) =>
        Execute(() => service.PerformAction(new UiAutomationActionRequest
        {
            Action = action,
            Locator = CreateLocateRequest(root, focused, fromFocused, handle, x, y, name, className, automationId, frameworkId, controlType, processId, scope, false, "subtree", "control", realizeVirtualized),
            StringValue = value,
            SecondStringValue = secondValue,
            NumberValue = number,
            SecondNumberValue = secondNumber,
            IntValue = intValue
        }));

    private static UiAutomationLocateRequest CreateLocateRequest(
        bool root,
        bool focused,
        bool fromFocused,
        long? handle,
        int? x,
        int? y,
        string? name,
        string? className,
        string? automationId,
        string? frameworkId,
        int? controlType,
        int? processId,
        string scope,
        bool cache,
        string cacheScope,
        string cacheView,
        bool realizeVirtualized = true,
        string? notName = null,
        string? notClassName = null,
        string? notAutomationId = null,
        int? notControlType = null) =>
        new()
        {
            DesktopRoot = root,
            FocusedElement = focused,
            SearchFromFocused = fromFocused,
            WindowHandle = handle,
            PointX = x,
            PointY = y,
            Name = name,
            ClassName = className,
            AutomationId = automationId,
            FrameworkId = frameworkId,
            ControlType = controlType,
            ProcessId = processId,
            Scope = scope,
            NotName = notName,
            NotClassName = notClassName,
            NotAutomationId = notAutomationId,
            NotControlType = notControlType,
            RealizeVirtualized = realizeVirtualized,
            CacheRequest = CreateCacheRequest(cache, cacheScope, cacheView)
        };

    private static UiAutomationCacheRequestInfo? CreateCacheRequest(bool cache, string cacheScope, string cacheView) =>
        cache
            ? new UiAutomationCacheRequestInfo
            {
                UseCache = true,
                Scope = cacheScope,
                View = cacheView
            }
            : null;
}
