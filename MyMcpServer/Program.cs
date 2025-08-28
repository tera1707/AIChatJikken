using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.Net.Http.Headers;

[McpServerToolType]
internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);

        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            //.WithResourcesFromAssembly()
            //.WithPromptsFromAssembly()
            .WithToolsFromAssembly();

        var app = builder.Build();

        await app.RunAsync();
    }
}
