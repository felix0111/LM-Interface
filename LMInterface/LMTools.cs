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

        public static async Task<ToolCallResponse> GetToolResponse(string toolCallId, ToolCallArguments arguments) {
            HttpHelper.JsonUrl? jsonUrl = JsonConvert.DeserializeObject<HttpHelper.JsonUrl>(arguments.Arguments);
            if (jsonUrl == null) {
                return new ToolCallResponse() { Role = "tool", Content = $"Tool Error: Could not parse the json format!", ToolCallId = toolCallId };
            }

            string webContent = await HttpHelper.GetWebsiteContent(jsonUrl.Url, jsonUrl.Nodes);
            return new ToolCallResponse() {Role = "tool", Content = webContent, ToolCallId = toolCallId};
        }

    }
}
