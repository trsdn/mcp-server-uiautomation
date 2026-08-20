using System.Text.Json;
using System.Globalization;
using UIAutomationMcp.ComInterop;
using UIAutomationMcp.Service;

var service = new UiAutomationService();
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };

try
{
    if (args.Length == 0)
    {
        WriteHelp();
        return;
    }

    var command = args[0].Trim().ToLowerInvariant();

    if (command is "--version" or "-v" or "version")
    {
        Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0");
        return;
    }

    var input = new CliInput(args.Skip(1).ToArray());

    object? result = command switch
    {
        "desktop" => service.ProbeDesktop(),
        "snapshot" => service.CaptureSnapshot(),
        "focused" => service.GetFocusedElement(),
        "handle" => service.GetElementFromHandle(ParseHandleOption(input)),
        "find-name" => service.FindFirstByName(input.RequireOption("--name")),
        "find-class" => service.FindFirstByClassName(input.RequireOption("--class")),
        "find-automation-id" => service.FindFirstByAutomationId(input.RequireOption("--automation-id")),
        "inspect" => service.Inspect(BuildLocateRequest(input, requireExplicit: false)),
        "find" => service.FindAll(BuildSearchRequest(input)),
        "children" => service.ListChildren(BuildLocateRequest(input, requireExplicit: true), input.GetOption("--view") ?? "control", ParseOptionalInt(input, "--max-results") ?? 50),
        "descendants" => service.ListDescendants(BuildLocateRequest(input, requireExplicit: true), input.GetOption("--view") ?? "control", ParseOptionalInt(input, "--max-results") ?? 50),
        "point" => service.GetElementFromPoint(ParseInt(input, "--x"), ParseInt(input, "--y")),
        "navigate" => service.Navigate(BuildLocateRequest(input, requireExplicit: true), input.RequireOption("--direction"), input.GetOption("--view") ?? "control"),
        "text" => service.ReadText(BuildLocateRequest(input, requireExplicit: true)),
        "selection" => service.ReadSelection(BuildLocateRequest(input, requireExplicit: true)),
        "table" => service.ReadTable(
            BuildLocateRequest(input, requireExplicit: true),
            ParseOptionalInt(input, "--max-rows") ?? 50,
            ParseOptionalInt(input, "--max-columns") ?? 25),
        "action" => service.PerformAction(BuildActionRequest(input)),
        "wait-event" => service.WaitForEvent(BuildEventWaitRequest(input)),
        "help" or "--help" or "-h" => null,
        _ => throw new ArgumentException($"Unknown command '{args[0]}'.")
    };

    if (command is "help" or "--help" or "-h")
    {
        WriteHelp();
        return;
    }

    Console.WriteLine(JsonSerializer.Serialize(result, options));
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}

static UiAutomationLocateRequest BuildLocateRequest(CliInput input, bool requireExplicit)
{
    var request = new UiAutomationLocateRequest
    {
        DesktopRoot = input.HasFlag("--root"),
        FocusedElement = input.HasFlag("--focused"),
        SearchFromFocused = input.HasFlag("--from-focused"),
        Name = input.GetOption("--name"),
        ClassName = input.GetOption("--class"),
        AutomationId = input.GetOption("--automation-id"),
        FrameworkId = input.GetOption("--framework-id"),
        Scope = input.GetOption("--scope") ?? "subtree",
        RealizeVirtualized = !input.HasFlag("--no-virtualized"),
        CacheRequest = BuildCacheRequest(input)
    };

    if (input.TryGetOption("--control-type", out var controlTypeText))
    {
        request.ControlType = int.Parse(controlTypeText, CultureInfo.InvariantCulture);
    }

    if (input.TryGetOption("--process-id", out var processIdText))
    {
        request.ProcessId = int.Parse(processIdText, CultureInfo.InvariantCulture);
    }

    if (input.TryGetOption("--handle", out var handleText))
    {
        request.WindowHandle = ParseHandleValue(handleText).ToInt64();
    }

    if (input.TryGetOption("--x", out var xText) && input.TryGetOption("--y", out var yText))
    {
        request.PointX = int.Parse(xText, CultureInfo.InvariantCulture);
        request.PointY = int.Parse(yText, CultureInfo.InvariantCulture);
    }

    var hasLocator = request.DesktopRoot
        || request.FocusedElement
        || request.WindowHandle.HasValue
        || (request.PointX.HasValue && request.PointY.HasValue)
        || !string.IsNullOrWhiteSpace(request.Name)
        || !string.IsNullOrWhiteSpace(request.ClassName)
        || !string.IsNullOrWhiteSpace(request.AutomationId)
        || !string.IsNullOrWhiteSpace(request.FrameworkId)
        || request.ControlType.HasValue
        || request.ProcessId.HasValue;

    if (requireExplicit && !hasLocator)
    {
        throw new ArgumentException("A locator is required. Use flags like --focused, --root, --handle, --x/--y, --name, --class, or --automation-id.");
    }

    return request;
}

static UiAutomationSearchRequest BuildSearchRequest(CliInput input)
{
    var locate = BuildLocateRequest(input, requireExplicit: false);
    var maxResults = input.TryGetOption("--max-results", out var maxText) ? int.Parse(maxText, CultureInfo.InvariantCulture) : 50;

    return new UiAutomationSearchRequest
    {
        DesktopRoot = locate.DesktopRoot,
        FocusedElement = locate.FocusedElement,
        WindowHandle = locate.WindowHandle,
        PointX = locate.PointX,
        PointY = locate.PointY,
        Name = locate.Name,
        ClassName = locate.ClassName,
        AutomationId = locate.AutomationId,
        FrameworkId = locate.FrameworkId,
        ControlType = locate.ControlType,
        ProcessId = locate.ProcessId,
        SearchFromFocused = locate.SearchFromFocused,
        Scope = locate.Scope,
        MaxResults = maxResults,
        CacheRequest = locate.CacheRequest
    };
}

static UiAutomationActionRequest BuildActionRequest(CliInput input)
{
    var positionals = input.GetPositionals();
    if (positionals.Count == 0)
    {
        throw new ArgumentException("The action command requires an action name as the first positional argument.");
    }

    var action = positionals[0];
    var firstValue = positionals.Count > 1 ? positionals[1] : null;
    var secondValue = positionals.Count > 2 ? positionals[2] : null;

    return new UiAutomationActionRequest
    {
        Action = action,
        Locator = BuildLocateRequest(input, requireExplicit: true),
        StringValue = firstValue ?? input.GetOption("--value"),
        SecondStringValue = secondValue ?? input.GetOption("--second-value"),
        NumberValue = TryParseDouble(firstValue ?? input.GetOption("--number")),
        SecondNumberValue = TryParseDouble(secondValue ?? input.GetOption("--second-number")),
        IntValue = TryParseInt(input.GetOption("--int"))
    };
}

static UiAutomationEventWaitRequest BuildEventWaitRequest(CliInput input) => new()
{
    EventKind = input.RequireOption("--event-kind"),
    Locator = BuildLocateRequest(input, requireExplicit: false),
    CacheRequest = BuildCacheRequest(input),
    TimeoutMs = input.TryGetOption("--timeout-ms", out var timeoutText) ? int.Parse(timeoutText, CultureInfo.InvariantCulture) : 5000,
    EventId = input.TryGetOption("--event-id", out var eventIdText) ? int.Parse(eventIdText, CultureInfo.InvariantCulture) : null,
    PropertyId = input.TryGetOption("--property-id", out var propertyIdText) ? int.Parse(propertyIdText, CultureInfo.InvariantCulture) : null
};

static UiAutomationCacheRequestInfo? BuildCacheRequest(CliInput input)
{
    if (!input.HasFlag("--cache"))
    {
        return null;
    }

    return new UiAutomationCacheRequestInfo
    {
        UseCache = true,
        Scope = input.GetOption("--cache-scope") ?? input.GetOption("--scope") ?? "subtree",
        View = input.GetOption("--cache-view") ?? "control"
    };
}

static nint ParseHandleOption(CliInput input) => ParseHandleValue(input.RequireOption("--handle"));

static nint ParseHandleValue(string text)
{
    if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
    {
        return new nint(Convert.ToInt64(text, 16));
    }

    return new nint(long.Parse(text, CultureInfo.InvariantCulture));
}

static int ParseInt(CliInput input, string option) => int.Parse(input.RequireOption(option), CultureInfo.InvariantCulture);

static int? ParseOptionalInt(CliInput input, string option) => input.TryGetOption(option, out var value)
    ? int.Parse(value, CultureInfo.InvariantCulture)
    : null;

static double? TryParseDouble(string? value) => double.TryParse(value, out var parsed) ? parsed : null;

static int? TryParseInt(string? value) => int.TryParse(value, out var parsed) ? parsed : null;

static void WriteHelp()
{
    Console.WriteLine("UIAutomationMcp CLI");
    Console.WriteLine("Commands:");
    Console.WriteLine("  desktop");
    Console.WriteLine("  snapshot");
    Console.WriteLine("  focused");
    Console.WriteLine("  handle --handle <hwnd>");
    Console.WriteLine("  point --x <x> --y <y>");
    Console.WriteLine("  find-name --name <text>");
    Console.WriteLine("  find-class --class <class>");
    Console.WriteLine("  find-automation-id --automation-id <id>");
    Console.WriteLine("  inspect [locator flags]");
    Console.WriteLine("  find [locator flags] [--max-results <n>]");
    Console.WriteLine("  children [locator flags] [--view raw|control|content] [--max-results <n>]");
    Console.WriteLine("  descendants [locator flags] [--view raw|control|content] [--max-results <n>]");
    Console.WriteLine("  navigate [locator flags] --direction <parent|first-child|last-child|next-sibling|previous-sibling|normalize> [--view raw|control|content]");
    Console.WriteLine("  text [locator flags]");
    Console.WriteLine("  selection [locator flags]");
    Console.WriteLine("  table [locator flags] [--max-rows <n>] [--max-columns <n>]");
    Console.WriteLine("    reads a Grid/Table control as a cell matrix (defaults: 50 rows, 25 columns)");
    Console.WriteLine("  action <focus|invoke|set-value|expand|collapse|toggle|select|add-to-selection|remove-from-selection|maximize|minimize|restore|close|move|resize|scroll|scroll-percent|set-range-value|set-view|dock|realize|default-action> [values] [locator flags]");
    Console.WriteLine("    set-view <view-id|view-name>   switches a MultipleView control (see multipleViewPattern.supportedViews)");
    Console.WriteLine("    dock <top|left|bottom|right|fill|none>");
    Console.WriteLine("    realize                        realizes a virtualized item so it can be read or acted on");
    Console.WriteLine("    default-action                 runs the MSAA default action (LegacyIAccessible) for controls with no modern actionable pattern");
    Console.WriteLine("  wait-event --event-kind <focus|automation|property|structure> [--timeout-ms <ms>] [--event-id <id>] [--property-id <id>] [locator flags]");
    Console.WriteLine();
    Console.WriteLine("Locator flags:");
    Console.WriteLine("  --root");
    Console.WriteLine("  --focused");
    Console.WriteLine("  --from-focused");
    Console.WriteLine("  --handle <hwnd>");
    Console.WriteLine("  --x <x> --y <y>");
    Console.WriteLine("  --name <text>");
    Console.WriteLine("  --class <class>");
    Console.WriteLine("  --automation-id <id>");
    Console.WriteLine("  --framework-id <id>");
    Console.WriteLine("  --control-type <id>");
    Console.WriteLine("  --process-id <pid>");
    Console.WriteLine("  --scope <element|children|descendants|subtree>");
    Console.WriteLine("  --no-virtualized                 do not ask ItemContainer providers for items missing from the live tree");
    Console.WriteLine("  --cache");
    Console.WriteLine("  --cache-scope <element|children|descendants|subtree>");
    Console.WriteLine("  --cache-view <raw|control|content>");
}

internal sealed class CliInput
{
    private readonly Dictionary<string, string> options = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> flags = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> positionals = new();

    public CliInput(string[] args)
    {
        for (var index = 0; index < args.Length; index++)
        {
            var current = args[index];
            if (!current.StartsWith("--", StringComparison.Ordinal))
            {
                positionals.Add(current);
                continue;
            }

            if (index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                options[current] = args[index + 1];
                index++;
                continue;
            }

            flags.Add(current);
        }
    }

    public bool HasFlag(string name) => flags.Contains(name);

    public string? GetOption(string name) => options.TryGetValue(name, out var value) ? value : null;

    public bool TryGetOption(string name, out string value)
    {
        if (options.TryGetValue(name, out var found))
        {
            value = found;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string RequireOption(string name) => GetOption(name) ?? throw new ArgumentException($"Missing required option {name}.");

    public IReadOnlyList<string> GetPositionals() => positionals;
}
