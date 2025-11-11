using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer1;

[McpServerToolType]
internal static class McpServer1
{
    [McpServerTool, Description("私の家族がなにを好きなのかを取得します。")]
    internal static string GetWhatDoOurFamilyLike(
        [Description("なにを好きなのかを取得したい家族の呼称")] string target) => target switch
        {
            "パパ" => "ゲーム",
            "ママ" => "ミュージカル",
            "息子" => "車",
            "娘" => "自転車",
            _ => "不明です",
        };
}
