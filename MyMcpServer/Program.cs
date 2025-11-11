using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System;
using System.IO;
using System.Reflection;

[McpServerToolType]
internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);

        var mcp = builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            //.WithResourcesFromAssembly()
            //.WithPromptsFromAssembly()
            .WithToolsFromAssembly();

        // 同じフォルダにある外部アセンブリ "McpServer1.dll" を読み込み、ツールを追加登録
        var baseDir = AppContext.BaseDirectory;
        var pluginPath = Path.Combine(baseDir, "McpServer1.dll");
        if (File.Exists(pluginPath))
        {
            var externalAssembly = Assembly.LoadFrom(pluginPath);
            mcp.WithToolsFromAssembly(externalAssembly);
        }

        var app = builder.Build();

        await app.RunAsync();
    }
}
