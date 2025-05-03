using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LMInterface
{
    public static class LMHelper {
        /// <summary>
        /// Returns the string without the specified tag.
        /// </summary>
        public static string RemoveTag(this string s, string tagName, out string removedSection) {
            string startTag = $"<{tagName}>";
            string endTag = $"</{tagName}>";


            if (s.StartsWith(startTag)) {
                int indexStart = s.IndexOf(startTag, StringComparison.CurrentCulture);
                int indexEnd = s.IndexOf(endTag, StringComparison.CurrentCulture);

                removedSection = s.Substring(indexStart + startTag.Length, indexEnd - (indexStart + startTag.Length));

                return s.Substring(indexEnd + endTag.Length);
            }

            removedSection = "";
            return s;
        }

        /// <summary>
        /// Returns the content of the specified tag in a string.
        /// </summary>
        public static string GetTag(this string s, string tagName) {
            string startTag = $"<{tagName}>";
            string endTag = $"</{tagName}>";

            if (s.StartsWith(startTag)) {
                int indexStart = s.IndexOf(startTag, StringComparison.CurrentCulture);
                int indexEnd = s.IndexOf(endTag, StringComparison.CurrentCulture);

                return s.Substring(indexStart + startTag.Length, indexEnd - (indexStart + startTag.Length));
            }

            return "";
        }

        public static LMRequest MakeJsonRequest_Qwen3(List<Message> conversation, bool think) {
            var convo = conversation.Select(o => o.WithoutThinkSection()).ToList();
            if (!think) convo[^1].Content += " /no_think";

            return new() {
                Model = "qwen3-30b-a3b",
                MaxTokens = 4096,
                Messages = convo,
                Tools = new() {
                    new() {
                        Function = new() {
                            Name = "WebsiteContent",
                            Description = "Used to fetch the content of a website in a markdown format.",
                            Parameters = new() {
                                Type = "object",
                                Properties = new() {
                                    { "url", new() { Description = "The URL of the website.", Type = "string" } },
                                    { "nodes", new () {Description = "The XPath expression used as a filter. (optional)", Type = "string"}}
                                },
                                Required = new() { "url" }
                            }
                        }
                    }
                },
                ToolChoice = "auto",
                Temperature = think ? 0.6 : 0.7,
                Top_P = think ? 0.95 : 0.8,
                Top_K = 20,
                Stream = false
            };
        }

        public static async Task<List<Message>> GetToolResults(List<ToolCall> toolCalls) {
            List<Message> results = new();

            foreach (ToolCall toolCall in toolCalls) {

                switch (toolCall.Function.Name) {
                    case "WebsiteContent":
                        string webContent = "";
                        string url = JsonConvert.DeserializeObject<HttpHelper.JsonUrl>(toolCall.Function.Arguments)!.Url;
                        string? xpath = JsonConvert.DeserializeObject<HttpHelper.JsonUrl>(toolCall.Function.Arguments)!.Nodes;
                        await HttpHelper.GetWebsiteContent(url, xpath, result => webContent = result);

                        results.Add(new() {Role = "tool", Content = webContent});
                        break;
                    default:
                        results.Add(new Message() {Role = "tool", Content = $"Error: Could not find tool with name: {toolCall.Function.Name}!"});
                        break;
                }

            }

            return results;
        }

    }
}
