
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.UI.Xaml;

namespace LMInterface.Serializables {

    /// <summary>
    /// Holds all stuff related to the communication with API.
    /// </summary>
    public partial class ApiMessage {
        
        
        //general parameters
        [JsonProperty("role")] public required string Role { get; set; } // "system", "user", "assistant"
        [JsonProperty("content")] public string? Content { get; set; } // the actual message

        
        
        //when receiving a tool call, this will be != null
        public bool IsToolCall => ToolCalls != null;
        [JsonProperty("tool_calls")] public List<ToolCall>? ToolCalls { get; set; } //tools the model wants to use

        
        
        //when sending the tool call result, this will be != null
        public bool IsToolCallResult => ToolCallId != null;
        [JsonProperty("tool_call_id")] public string? ToolCallId { get; set; } //the id of the tool call to respond to



        //primary constructor doesn't work because the xaml backend would error, see https://github.com/microsoft/microsoft-ui-xaml/issues/8723
        [SetsRequiredMembers]
        public ApiMessage() {
            Role = "user";
        }

        public ApiMessage Clone() => JsonConvert.DeserializeObject<ApiMessage>(JsonConvert.SerializeObject(this))!;
    }



    /// <summary>
    /// Holds all properties and functions used for UI.
    /// </summary>
    public partial class ApiMessage {
        public HorizontalAlignment MessageAlignment {
            get {
                if (Role == "system") return HorizontalAlignment.Center;
                return Role == "assistant" ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            }
        }

        public Thickness MessageMargin {
            get {
                if (Role == "system") return new Thickness(60, 15, 60, 15);
                return Role == "assistant" ? new Thickness(0, 20, 50, 20) : new Thickness(50, 20, 0, 20);
            }
        }

        public Visibility ThoughtsVisible => Thoughts.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ToolCallVisible => ToolCall.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ToolCallResultVisible => ToolCallResult.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MainContentVisible => MainContent.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;

        public string ToolCall => ToolCalls != null ? ToolCalls!.Select(o => $"{o.ToolCallArguments.Name} : {o.ToolCallArguments.Arguments}").Aggregate((s, s1) => $"{s}  \n{s1}") : "";
        public string ToolCallResult => ToolCallId != null ? Content! : "";

        public string Thoughts => Content == null ? "" : Content.GetTag("think");
        public string MainContent => Content == null || ToolCallId != null ? "" : Content.RemoveTag("think", out _).RemoveTag("tool", out _);

        public ApiMessage WithoutThinkSection() {
            ApiMessage clone = Clone();
            clone.Content = clone.Content.RemoveTag("think", out _);

            return clone;
        }
    }
}
