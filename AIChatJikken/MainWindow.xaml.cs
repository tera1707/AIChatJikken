using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using System.ClientModel;
using System.Diagnostics;
using System.Windows;

namespace AIChatJikken;

public partial class MainWindow : Window
{
    IChatClient? aiClient;
    IChatClient? chatClient;
    ChatOptions? chatOption;
    List<ChatMessage> chatHistory = new();

    IMcpClient? mcpClient;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<MainWindow>().Build();
        
        // MCPサーバー起動時のパラメータの設定
        var clientTransport = new StdioClientTransport(new()
        {
            // MCPサーバーのexeは、@"C:\Program Files\MyMcpServer\MyMcpServer.exe"に置くことにする。引数は無し。
            Command = @"C:\Program Files\MyMcpServer\MyMcpServer.exe",
            Arguments = [],
            Name = "My Mcp Server",
        });

        // MCPクライアントを作成（ここで、MCPサーバーが起動する）
        mcpClient = await McpClientFactory.CreateAsync(clientTransport!);

        // ツールの名前を列挙
        var mcpTools = await mcpClient.ListToolsAsync();
        foreach (var tool in mcpTools)
        {
            Debug.WriteLine($"MCPサーバーのツール名：{tool.Name}");
        }

        //-------------------------------

        chatOption = new ChatOptions
        {
            ToolMode = ChatToolMode.Auto,
            Tools = [.. mcpTools]
        };

        // github modelsのキーを入れる
        var credential = new ApiKeyCredential(configuration["AzureOpenAI:Token"]!);

        // LLMのモデルを指定
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        aiClient = new AzureOpenAIClient(new Uri(endpoint!), credential)
                            .GetChatClient("gpt-4o-mini")
                            .AsIChatClient();

        chatClient = aiClient.AsBuilder()
                                    .UseFunctionInvocation()
                                    .Build();

        PromptBox.Text = "休日のパパはなにをしていますか？";
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        SendButton.IsEnabled = false;
        ResponseBlock.Text = "";
        var prompt = PromptBox.Text;

        if (prompt is null)
            return;

        await GetCompletionAsync(prompt, (response =>
        {
            ResponseBlock.Text += response;
        }));

        SendButton.IsEnabled = true;
    }

    public async Task GetCompletionAsync(string prompt, Action<string> onResponse)
    {
        var chatmsg = new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, prompt);
        chatHistory.Add(chatmsg);

        // チャットを送信
        string res = "";
        await foreach (var update in chatClient!.GetStreamingResponseAsync(chatHistory, chatOption))
        {
            onResponse(update.Text);
            res += update.Text;
        }

        chatHistory.Add(new ChatMessage(ChatRole.Assistant, res));
    }

    private async void Window_Closed(object sender, EventArgs e)
    {
        // MCPクライアントを終了（ここで、MCPサーバーが終了する）
        await mcpClient!.DisposeAsync();
    }
}