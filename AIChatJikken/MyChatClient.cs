using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.ClientModel;

namespace AIChatJikken;

internal class MyChatClient :IDisposable
{
    IChatClient? chatClient;
    List<ChatMessage> chatHistory = new();

    IList<McpClientTool> _mcpTools;

    public MyChatClient(Uri endpoint, ApiKeyCredential credential, IList<McpClientTool> mcpTools)
    {
        // LLMのモデルを指定
        var aiClient = new AzureOpenAIClient(endpoint!, credential)
                            .GetChatClient("gpt-4o-mini")
                            .AsIChatClient();

        chatClient = aiClient.AsBuilder()
                                    .UseFunctionInvocation()
                                    .Build();
        _mcpTools = mcpTools;
    }

    public async Task GetCompletionAsync(string prompt, Action<string> onResponse)
    {
        var chatmsg = new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, prompt);
        chatHistory.Add(chatmsg);

        var chatOption = new ChatOptions
        {
            ToolMode = ChatToolMode.Auto,
            Tools = [.. _mcpTools]
        };

        // チャットを送信
        string res = "";
        await foreach (var update in chatClient!.GetStreamingResponseAsync(chatHistory, chatOption))
        {
            onResponse(update.Text);
            res += update.Text;
        }

        chatHistory.Add(new ChatMessage(ChatRole.Assistant, res));
    }

    public void Dispose()
    {
        chatClient?.Dispose();
    }


}
