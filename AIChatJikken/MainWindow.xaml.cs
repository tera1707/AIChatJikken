using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.ClientModel;
using System.Diagnostics;
using System.Windows;

namespace AIChatJikken;

public partial class MainWindow : Window
{
    List<ChatMessage> chatHistory = new();

    public MainWindow()
    {
        InitializeComponent();

        PromptBox.Text = "休日のパパはなにをしていますか？";
        Key.Text = "ここにAzure OpenAIのキーを入れてください。";
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

        // github modelsのキーを入れる
        var credential = new ApiKeyCredential(key);

        // LLMのモデルを指定
        var aiClient = new AzureOpenAIClient(new Uri("https://myendpoint.openai.azure.com/"), credential)
                            .GetChatClient("gpt-4o-mini")
                            .AsIChatClient();

        var chatClient = aiClient.AsBuilder()
                                    .UseFunctionInvocation()
                                    .Build();

        var chatmsg = new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, prompt);
        chatHistory.Add(chatmsg);

        // チャットを送信
        var res = await chatClient.GetResponseAsync(chatHistory, chatOption);

        chatHistory.Add(new ChatMessage(ChatRole.Assistant, res.Text));

        //-------------------------

        // MCPクライアントを終了（ここで、MCPサーバーが終了する）
        await mcpClient.DisposeAsync();

        return res.Text;
    }
}