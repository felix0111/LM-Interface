using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LMInterface
{
    public static class LMHelper {

        public static string FormatRawText(Message msg) {

            //if lm uses tool
            if (msg.ToolCalls != null) {
                return $"*Used tool {msg.ToolCalls[0].Function.Name} with parameters {msg.ToolCalls[0].Function.Arguments}*";
            }

            //replace all \r with \n
            string rawText = msg.Content;
            string formattedMessage = "";

            //process <think></think> paragraph
            int indexStart = 0;
            int indexEnd = 0;

            //process think section at start
            if (rawText.StartsWith($"<think>{new string('\n', 2)}</think>")) {
                formattedMessage = rawText.Substring(17);
            } else if (rawText.StartsWith("<think>")) {
                indexStart = rawText.IndexOf("<think>", StringComparison.CurrentCulture);
                indexEnd = rawText.IndexOf("</think>", StringComparison.CurrentCulture);

                string thinkContent = rawText.Substring(indexStart + 7, indexEnd - (indexStart + 7));
                formattedMessage = $"```{thinkContent}```  \n{rawText.Substring(indexEnd + 8)}";
            } else {
                formattedMessage = rawText;
            }

            return formattedMessage;
        }

        public static Message RemoveThinkSection(Message message) {
            Message newMessage = message.Clone();

            //remove think section at start
            if (message.Role == "assistant" && newMessage.Content.StartsWith("<think>")) {
                int indexEnd = newMessage.Content.IndexOf("</think>", StringComparison.CurrentCulture);
                newMessage.Content = newMessage.Content.Substring(indexEnd + 8).TrimStart('\n');
            }

            return newMessage;
        }

        public static LMRequest MakeJsonRequest_Qwen3(List<Message> conversation, bool think) {
            var convo = conversation.Select(RemoveThinkSection).ToList();
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
