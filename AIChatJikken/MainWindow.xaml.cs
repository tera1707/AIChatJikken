using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;
using System.ClientModel;
using System.Diagnostics;
using System.Net;
using System.Windows;

namespace AIChatJikken;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        PromptBox.Text = "休日のパパはなにをしていますか？";
        Key.Text = "ここにgithub modelsのキーを入れてください";
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        SendButton.IsEnabled = false;
        var prompt = PromptBox.Text;

        if (prompt is null)
            return;

        ResponseBlock.Text = await GetCompletionAsync(prompt, Key.Text);
        SendButton.IsEnabled = true;
    }

    public async Task<string> GetCompletionAsync(string prompt, string key)
    {
        // MCPサーバー起動時のパラメータの設定
        var clientTransport = new StdioClientTransport(new()
        {
            // MCPサーバーのexeは、@"C:\Program Files\MyMcpServer\MyMcpServer.exe"に置くことにする。引数は無し。
            Command = @"C:\Program Files\MyMcpServer\MyMcpServer.exe",
            Arguments = [],
            Name = "My Mcp Server",
        });

        // MCPクライアントを作成（ここで、MCPサーバーが起動する）
        var mcpClient = await McpClientFactory.CreateAsync(clientTransport!);

        // ツールの名前を列挙
        var mcpTools = await mcpClient.ListToolsAsync();
        foreach (var tool in mcpTools)
        {
            Debug.WriteLine($"MCPサーバーのツール名：{tool.Name}");
        }

        //-------------------------------

        var chatOption = new ChatOptions
        {
            ToolMode = ChatToolMode.Auto,
            Tools = [.. mcpTools]
        };

        var aiClient = new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2");

        var chatClient = aiClient.AsBuilder()
                                    .UseFunctionInvocation()
                                    .Build();

        var chatmsg = new ChatMessage(ChatRole.User, prompt);

        Thread.Sleep(5000);
        // チャットを送信
        var res = await chatClient.GetResponseAsync([chatmsg], chatOption);

        //-------------------------

        // MCPクライアントを終了（ここで、MCPサーバーが終了する）
        await mcpClient.DisposeAsync();

        return res.Text;
    }
}