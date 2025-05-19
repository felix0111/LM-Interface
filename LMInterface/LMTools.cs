using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LMInterface {

    public class WebTool : Tool {

        [SetsRequiredMembers]
        public WebTool() {
            Function = new() {
                Name = "WebsiteContent",
                Description = "Used to fetch the content of a website in a markdown format.",
                Parameters = new() {
                    Properties = new() {
                        { "url", new() { Description = "The URL of the website.", Type = "string" } },
                        { "nodes", new() { Description = "The XPath expression used as a filter. (optional)", Type = "string" } }
                    },
                    Required = new() { "url" }
                }
            };
        }

        public static async Task<string> GetResult(string arguments) {
            WebToolArguments? jsonUrl = JsonConvert.DeserializeObject<WebToolArguments>(arguments);
            if (jsonUrl == null) return "Tool Error: Could not parse the json format!";

            return await HttpHelper.GetWebsiteContent(jsonUrl.Url, jsonUrl.Nodes);
        }

    }

    public class WebToolArguments {
        [JsonProperty("url")] public required string Url { get; set; }
        [JsonProperty("nodes")] public string? Nodes { get; set; } //the xpath expression used for search
    }

    public class PythonTool : Tool {

        [SetsRequiredMembers]
        public PythonTool() {
            Function = new() {
                Name = "ExecutePython",
                Description = "Used to execute a python script, returns the standard output stream. You may import numpy.",
                Parameters = new() {
                    Properties = new() {
                        { "script", new() { Description = "The python script.", Type = "string" } }
                    },
                    Required = new() { "script" }
                }
            };
        }

        public static async Task<string> GetResult(string arguments) {
            PythonToolArguments? args = JsonConvert.DeserializeObject<PythonToolArguments>(arguments);
            if (args == null) return "Tool Error: Could not parse the json format!";

            return await PythonHelper.ExecutePythonScript(args.Script);
        }
    }

    public class PythonToolArguments {
        [JsonProperty("script")] public required string Script { get; set; }
    }
}
