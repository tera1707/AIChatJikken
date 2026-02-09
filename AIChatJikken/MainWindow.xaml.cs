using Microsoft.Extensions.Configuration;
using System.ClientModel;
using System.Windows;

namespace AIChatJikken;

public partial class MainWindow : Window
{
    private MyChatClient? chatClient1;
    private MyChatClient? chatClient2;

    private static readonly string prerequisites = "以下の内容について、コメントをしてください。提案を求められた際は、一つの案にこだわらず、出来るだけ色々な案を出すようにしてください。たまに質問をするようにしてください。\r\n--------\r\n";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<MainWindow>().Build();

        var credential = new ApiKeyCredential(configuration["AzureOpenAI:Token"]!);
        var endpoint = new Uri(configuration["AzureOpenAI:Endpoint"]!);

        chatClient1 = new MyChatClient(endpoint, credential, null);
        chatClient2 = new MyChatClient(endpoint, credential, null);

        // 最初の話題提供のためのプロンプト
        PromptBox.Text = "昨今の環境問題について討論をさせてください。まずごみ問題についてどう思いますか？";
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(PromptBox.Text))
            return;

        if (chatClient1 is null || chatClient2 is null)
            return;

        // 画面で設定したものを取得
        var userPrompt = PromptBox.Text;
        int maxCoversation = int.Parse(MaxConversationCount.Text);
        string[] characterSetting = { Character1Name.Text, Character2Name.Text };

        // チャットの出力欄をリセット
        SendButton.IsEnabled = false;
        ResponseBlock1.Text = "";

        // チャットクライアント作成
        await chatClient1.GetCompletionAsync($"あなたは{characterSetting[0]}です。その口調で話してください。話し相手は{characterSetting[1]}です。そのつもりで話してください。", (_) => { });
        await chatClient2.GetCompletionAsync($"あなたは{characterSetting[1]}です。その口調で話してください。話し相手は{characterSetting[0]}です。そのつもりで話してください。", (_) => { });

        var aitenoResponse = "";

        // チャット処理
        for (int i = 0; i < (maxCoversation * 2); i++)
        {
            // 最初の一回は、ユーザーが入れたプロンプトをもとに回答し、それ以降は相手の回答をもとに回答する。
            var prompt = prerequisites + ((aitenoResponse == "") ? userPrompt : aitenoResponse);
            // 相手の回答をもとにこの回のinputを作ったら、前回の相手の回答はリセットする
            aitenoResponse = "";
            var name = characterSetting[i % 2];
            ResponseBlock1.Text += $"■{name} の発言({(i / 2) + 1})\r\n";

            // 回答を生成
            await GetTargetClient(i)!.GetCompletionAsync(prompt, (response =>
            {
                // 一文字ずつ画面に表示
                ResponseBlock1.Text += response;
                aitenoResponse += response;
            }));

            // 最後に改行を入れる
            ResponseBlock1.Text += "\r\n\r\n";

            await Task.Delay(1000);
        }

        // 指定の回数の会話が終わったら、送信ボタンを有効化する
        SendButton.IsEnabled = true;

        // 一回の会話が終わったら、クライアントを破棄する
        chatClient1?.Dispose();
        chatClient2?.Dispose();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        chatClient1?.Dispose();
        chatClient2?.Dispose();
    }

    private MyChatClient? GetTargetClient(int count)
    {
        return ((count % 2) == 0) ? chatClient1 : chatClient2;
    }
}