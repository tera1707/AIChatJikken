using ABI.Windows.ApplicationModel.Activation;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using System.ClientModel;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using Windows.Devices.Usb;

namespace AIChatJikken;

public partial class MainWindow : Window
{
    MyMcpClient mcpClient;
    MyChatClient chatClient;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<MainWindow>().Build();

        // credential endpoint mcpTools
        //-------------------------------
        mcpClient = await MyMcpClient.CreateMcpServer("My Mcp Server", @"C:\Program Files\MyMcpServer\MyMcpServer.exe", []);

        //-------------------------------
        var credential = new ApiKeyCredential(configuration["AzureOpenAI:Token"]!);
        var endpoint = new Uri(configuration["AzureOpenAI:Endpoint"]!);
        chatClient = new MyChatClient(endpoint, credential, await mcpClient.GetMcpClientTools());

        PromptBox.Text = "休日のパパはなにをしていますか？";
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        SendButton.IsEnabled = false;
        ResponseBlock.Text = "";
        var prompt = PromptBox.Text;

        if (prompt is null)
            return;

        await chatClient.GetCompletionAsync(prompt, (response =>
        {
            ResponseBlock.Text += response;
        }));

        PromptBox.Text = "";
        SendButton.IsEnabled = true;
    }

    private async void Window_Closed(object sender, EventArgs e)
    {
        // MCPクライアントを終了（ここで、MCPサーバーが終了する）
        await mcpClient!.DisposeAsync();
    }
}