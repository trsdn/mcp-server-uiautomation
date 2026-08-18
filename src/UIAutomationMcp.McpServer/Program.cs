using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMcp.Service;

namespace UIAutomationMcp.McpServer;

internal static class Program
{
    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0] is "--version" or "-v" or "version")
        {
            Console.WriteLine(typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "1.0.0");
            return 0;
        }

        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Warning;
        });
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services.AddSingleton<UiAutomationService>();

        builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new()
                {
                    Name = "uia-mcp",
                    Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
                };
                options.ServerInstructions = """
                    UIAutomationMcp exposes Windows UI Automation queries.

                    Use the snapshot and focused-element tools for inspection.
                    Use the search tools for lookup by name, class name, automation id, or window handle.
                    This server runs on Windows and requires an interactive desktop session.
                    """;
            })
            .WithToolsFromAssembly()
            .WithStdioServerTransport();

        using var host = builder.Build();
        await host.RunAsync();
        return 0;
    }
}
