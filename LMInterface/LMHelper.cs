using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMInterface.Serializables;
using LMInterface.Services;

namespace LMInterface
{
    public static class LMHelper {
        /// <summary>
        /// Returns the string without the specified tag.
        /// If there is no end-tag found, it will remove the whole string and return empty.
        /// </summary>
        //TODO search for start-tag at any position (not s.StartsWith(startTag))
        public static string RemoveTag(this string s, string tagName, out string removedSection) {
            string startTag = $"<{tagName}>";
            string endTag = $"</{tagName}>";

            if (s.StartsWith(startTag)) {
                int indexStart = s.IndexOf(startTag, StringComparison.CurrentCulture);
                int indexEnd = s.IndexOf(endTag, StringComparison.CurrentCulture);
                if (indexEnd == -1) {
                    removedSection = s.Substring(indexStart + startTag.Length);
                    return "";
                }

                removedSection = s.Substring(indexStart + startTag.Length, indexEnd - (indexStart + startTag.Length));

                return s.Substring(indexEnd + endTag.Length);
            }

            removedSection = "";
            return s;
        }

        /// <summary>
        /// Returns the content of the specified tag in a string.
        /// If there is no end-tag specified, it will return the whole string.
        /// </summary>
        public static string GetTag(this string s, string tagName) {
            string startTag = $"<{tagName}>";
            string endTag = $"</{tagName}>";

            if (s.StartsWith(startTag)) {
                int indexStart = s.IndexOf(startTag, StringComparison.CurrentCulture);
                int indexEnd = s.IndexOf(endTag, StringComparison.CurrentCulture);

                //if endTag was not found
                if (indexEnd == -1) return s.Substring(indexStart + startTag.Length);

                return s.Substring(indexStart + startTag.Length, indexEnd - (indexStart + startTag.Length));
            }

            //if tag was not found
            return "";
        }

        /// <summary>
        /// Automatically removes unnecessary sections and messages from the conversation. (Thinking sections and tool calls/results)
        /// </summary>
        public static ApiRequest MakeApiRequest(Conversation conv, bool think, List<Tool>? toolset, string toolChoice) {
            //filter conversation
            var finalConversation = conv.Messages.Where(o => !o.IsToolCall && !o.IsToolCallResult).Select(o => o.WithoutThinkSection()).ToList();

            //if last message in conversation is tool call result, then include the tool call and its result(s) in the final conversation
            if (conv.Messages.Last().IsToolCallResult) {
                int toolCallIndex = conv.Messages.IndexOf(conv.Messages.Last(o => o.IsToolCall));
                for (int i = toolCallIndex; i < conv.Messages.Count; i++) finalConversation.Add(conv.Messages[i].Clone());
            }

            //add no reasoning token
            if (!think) finalConversation[^1].Content += $" {conv.NoReasoningToken}";

            return new() {
                Model = conv.ModelId,
                MaxTokens = conv.MaxTokens,
                Messages = finalConversation,
                Tools = toolset,
                ToolChoice = toolChoice,
                Temperature = think ? 0.6 : 0.7,
                TopP = think ? 0.95 : 0.8,
                TopK = 20,
                Stream = false
            };
        }

        public static async Task<List<ApiMessage>> GetToolResults(List<ToolCall> toolCalls) {
            List<ApiMessage> results = new();

            foreach (ToolCall toolCall in toolCalls) {
                ApiMessage msg = new () { Role = "tool", Content = "", ToolCallId = toolCall.Id };

                switch (toolCall.ToolCallArguments.Name) {
                    case "WebsiteContent":
                        msg.Content = await WebTool.GetResult(toolCall.ToolCallArguments.Arguments);
                        break;
                    case "ExecutePython":
                        msg.Content = await PythonTool.GetResult(toolCall.ToolCallArguments.Arguments);
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
