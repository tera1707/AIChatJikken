using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMcpServer;

[McpServerToolType]
internal static class HowToSpendHoliday
{
    [McpServerTool, Description("Get how to spend holiday. 休日の過ごし方を取得する。")]
    internal static string GetHowToSpendHoliday(
        [Description("休日の過ごし方を取得したい家族の呼称")] string target) => target switch
        {
            "パパ" => "ごろごろしています。",
            "ママ" => "ショッピングしています。",
            "息子" => "走り回っています。",
            "娘" => "歌を歌っています。",
            _ => "どうしてるんでしょう？",
        };
}
