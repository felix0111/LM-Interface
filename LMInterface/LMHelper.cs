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

                //if endTag was not found
                if (indexEnd == -1) return "";

                return s.Substring(indexStart + startTag.Length, indexEnd - (indexStart + startTag.Length));
            }

            //if tag was not found
            return "";
        }

        /// <summary>
        /// Automatically removes unnecessary sections and messages from the conversation. (Thinking sections and tool calls/results)
        /// </summary>
        public static LMRequest MakeJsonRequest_Qwen3(List<Message> conversation, bool think, List<Tool>? toolset, string toolChoice) {
            var convo = conversation.Where(o => !o.IsToolCall && !o.IsToolCallResult).Select(o => o.WithoutThinkSection()).ToList();
            if (!think) convo[^1].Content += " /no_think";

            return new() {
                Model = "unsloth/qwen3-30b-a3b",
                MaxTokens = 4096,
                Messages = convo,
                Tools = toolset,
                ToolChoice = toolChoice,
                Temperature = think ? 0.6 : 0.7,
                TopP = think ? 0.95 : 0.8,
                TopK = 20,
                Stream = false
            };
        }

        public static async Task<List<Message>> GetToolResults(List<ToolCall> toolCalls) {
            List<Message> results = new();

            foreach (ToolCall toolCall in toolCalls) {
                Message msg = new () { Role = "tool", Content = "", ToolCallId = toolCall.Id };

                switch (toolCall.ToolCallArguments.Name) {
                    case "WebsiteContent":
                        msg.Content = await WebTool.GetResult(toolCall.ToolCallArguments.Arguments);
                        break;
                    default:
                        msg.Content = $"Error: Could not find tool with name: {toolCall.ToolCallArguments.Name}!";
                        break;
                }
                results.Add(msg);
            }

            return results;
        }

    }
}
