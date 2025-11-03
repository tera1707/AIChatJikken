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
using System.Windows.Controls;
using Windows.Devices.Usb;

namespace AIChatJikken;

public partial class MainWindow : Window
{
    MyMcpClient? mcpClient;
    MyChatClient? chatClient1;
    MyChatClient? chatClient2;

    private static readonly string prerequisites = "以下の内容について、コメントをしてください。提案を求められた際は、一つの案にこだわらず、出来るだけ色々な案を出すようにしてください。たまに質問をするようにしてください。\r\n--------\r\n";

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<MainWindow>().Build();

        // credential endpoint mcpTools
        //-------------------------------
        //mcpClient = await MyMcpClient.CreateMcpServer("My Mcp Server", @"C:\Program Files\MyMcpServer\MyMcpServer.exe", []);

        //-------------------------------
        var credential = new ApiKeyCredential(configuration["AzureOpenAI:Token"]!);
        var endpoint = new Uri(configuration["AzureOpenAI:Endpoint"]!);
        //chatClient = new MyChatClient(endpoint, credential, await mcpClient?.GetMcpClientTools());
        chatClient1 = new MyChatClient(endpoint, credential, null);
        chatClient2 = new MyChatClient(endpoint, credential, null);

        PromptBox.Text = "昨今の環境問題について討論をさせてください。まずごみ問題についてどう思いますか？";
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        SendButton.IsEnabled = false;
        ResponseBlock1.Text = "";
        ResponseBlock2.Text = "";
        var userPrompt = PromptBox.Text;

        if (userPrompt is null)
            return;

        await chatClient1.GetCompletionAsync("あなたはツンデレの口調で話します。", (_) => { });
        await chatClient2.GetCompletionAsync("あなたはおじいちゃんの口調で話します。", (_) => { });


        //まずは1番に、入力した問いを投げかける
        string prompt0 = prerequisites + userPrompt;
        int aite = 0;
        var name = (aite % 2) == 0 ? "ギャル" : "じいちゃん";
        GetTargetTextBlock(aite)!.Text += $"■{name}-----------\r\n";
        await GetTargetClient(aite)!.GetCompletionAsync(prompt0, (response =>
        {
            GetTargetTextBlock(aite)!.Text += response;
        }));
        GetTargetTextBlock(aite)!.Text += "\r\n\r\n";


        string aitenoResponse = "";
        aitenoResponse += GetTargetTextBlock(0)!.Text;

        // その後は、お互いのレスポンスを入力として、会話を続ける。
        for (int i = 1; i < 7; i++)
        {
            string prompt = prerequisites + aitenoResponse;
            aitenoResponse = "";

            var name2 = (i % 2) == 0 ? "ギャル" : "じいちゃん";
            GetTargetTextBlock(i)!.Text += $"■{name2}-----------\r\n";
            await GetTargetClient(i)!.GetCompletionAsync(prompt, (response =>
            {
                GetTargetTextBlock(i)!.Text += response;
                aitenoResponse += response;
            }));
            GetTargetTextBlock(i)!.Text += "\r\n-----------------\r\n";
            await Task.Delay(2000);
        }


        PromptBox.Text = "";
        SendButton.IsEnabled = true;

        MyChatClient? GetTargetClient(int count)
        {
            return ((count % 2) == 0) ? chatClient1 : chatClient2;
        }


        TextBlock? GetTargetTextBlock(int count)
        {
            return ((count % 2) == 0) ? ResponseBlock1 : ResponseBlock1;
        }
    }

    private async void Window_Closed(object sender, EventArgs e)
    {
        // MCPクライアントを終了（ここで、MCPサーバーが終了する）
        if (mcpClient is not null)
        {
            await mcpClient.DisposeAsync();
        }
    }
}