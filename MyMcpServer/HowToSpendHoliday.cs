using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MyMcpServer;

[McpServerToolType]
internal static class HowToSpendHoliday
{
    [McpServerTool, Description("Get how to spend holiday. 休日の過ごし方を取得する。")]
    internal static string GetHowToSpendHoliday(
        [Description("A name for a family member who wants to learn how to spend their holidays. 休日の過ごし方を取得したい家族の呼称")] string target) => target switch
        {
            "father" => "Swimming in the pool.",
            "mother" => "Shopping.",
            "son" => "Runnning.",
            "daughter" => "Singing songs.",
            _ => "I don't know",
        };
}
