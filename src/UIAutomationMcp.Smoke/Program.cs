using System.Runtime.InteropServices;
using UIAutomationMcp.Service;

internal static class Program
{
    [DllImport("user32.dll")]
    private static extern nint GetDesktopWindow();

    [STAThread]
    private static void Main()
    {
        var service = new UiAutomationService();
        var snapshot = service.CaptureSnapshot();
        var result = service.ProbeDesktop();
        var focusedElement = service.GetFocusedElement();
        var desktopByClass = service.FindFirstByClassName("#32769");
        var desktopByHandle = service.GetElementFromHandle(GetDesktopWindow());

        Console.WriteLine("UI Automation COM bootstrap succeeded.");
        Console.WriteLine($"Coclass: {result.Coclass}");
        Console.WriteLine($"Root name: {result.RootName}");
        Console.WriteLine($"Root class: {result.RootClassName}");
        Console.WriteLine($"Root control type: {result.RootControlType}");
        Console.WriteLine($"Root process id: {result.RootProcessId}");
        Console.WriteLine($"Root automation id: {snapshot.Desktop.AutomationId}");

        if (focusedElement is not null)
        {
            Console.WriteLine("Focused element:");
            Console.WriteLine($"  Name: {focusedElement.Name}");
            Console.WriteLine($"  Class: {focusedElement.ClassName}");
            Console.WriteLine($"  Control type: {focusedElement.ControlType}");
            Console.WriteLine($"  Process id: {focusedElement.ProcessId}");
            Console.WriteLine($"  Automation id: {focusedElement.AutomationId}");
        }
        else
        {
            Console.WriteLine("Focused element: <unavailable>");
        }

        if (desktopByClass is not null)
        {
            Console.WriteLine("Class-name search:");
            Console.WriteLine($"  Name: {desktopByClass.Name}");
            Console.WriteLine($"  Class: {desktopByClass.ClassName}");
            Console.WriteLine($"  Control type: {desktopByClass.ControlType}");
            Console.WriteLine($"  Process id: {desktopByClass.ProcessId}");
            Console.WriteLine($"  Automation id: {desktopByClass.AutomationId}");
        }
        else
        {
            Console.WriteLine("Class-name search: <no match>");
        }

        Console.WriteLine("Handle lookup:");
        Console.WriteLine($"  Name: {desktopByHandle.Name}");
        Console.WriteLine($"  Class: {desktopByHandle.ClassName}");
        Console.WriteLine($"  Control type: {desktopByHandle.ControlType}");
        Console.WriteLine($"  Process id: {desktopByHandle.ProcessId}");
        Console.WriteLine($"  Automation id: {desktopByHandle.AutomationId}");
    }
}
