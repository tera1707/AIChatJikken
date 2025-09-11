using ModelContextProtocol.Client;
using System.Diagnostics;

namespace AIChatJikken;


internal class MyMcpClient
{
    private IMcpClient? _mcpClient;

    private MyMcpClient(IMcpClient mcpClient)
    {
        _mcpClient = mcpClient;
    }

    internal async static Task<MyMcpClient> CreateMcpServer(string name, string command, IList<string> argument)
    {
        // MCPサーバー起動時のパラメータの設定
        var clientTransport = new StdioClientTransport(new()
        {
            // MCPサーバーのexeは、@"C:\Program Files\MyMcpServer\MyMcpServer.exe"に置くことにする。引数は無し。
            Command = command,
            Arguments = argument,
            Name = name,
        });

        // MCPクライアントを作成（ここで、MCPサーバーが起動する）
        var mcpClient = await McpClientFactory.CreateAsync(clientTransport!);

        return new MyMcpClient(mcpClient);
    }

    internal async Task<IList<McpClientTool>> GetMcpClientTools()
    {
        // ツールの名前を列挙
        var mcpTools = await _mcpClient!.ListToolsAsync();
        foreach (var tool in mcpTools)
        {
            Debug.WriteLine($"MCPサーバーのツール名：{tool.Name}");
        }
        return mcpTools;
    }

    internal async Task DisposeAsync()
    {
        await _mcpClient!.DisposeAsync();
    }
}
