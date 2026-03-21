using System.Text.Json;
using System.Text.Json.Serialization;

namespace UIAutomationMcp.McpServer.Tools;

public abstract class UiAutomationToolsBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected string Execute(Func<object?> operation)
    {
        try
        {
            return JsonSerializer.Serialize(new
            {
                success = true,
                item = operation()
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                errorMessage = ex.Message,
                isError = true
            }, JsonOptions);
        }
    }
}
