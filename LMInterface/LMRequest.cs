using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LMInterface
{
    public class ChatCompletionRequest {
        [JsonProperty("model")] public string Model { get; set; }
        [JsonProperty("messages")] public Message[] Messages { get; set; }
        [JsonProperty("max_tokens")] public long MaxTokens { get; set; }
        [JsonProperty("temperature")] public double Temperature { get; set; }
        [JsonProperty("top_p")] public double Top_P { get; set; }
        [JsonProperty("top_k")] public double Top_K { get; set; }
        //which tools the model can use
        [JsonProperty("tools")] public Tool[] Tools { get; set; }
        [JsonProperty("stream")] public bool Stream { get; set; }
    }

    public class LMRequest {
        [JsonProperty("model")] public string Model { get; set; }
        [JsonProperty("messages")] public List<Message> Messages { get; set; }
        [JsonProperty("tools")] public List<Tool> Tools { get; set; }
        [JsonProperty("tool_choice")] public string ToolChoice { get; set; } // Can be string ("auto", "none", "required")
        [JsonProperty("max_tokens")] public long MaxTokens { get; set; }
        [JsonProperty("temperature")] public double Temperature { get; set; }
        [JsonProperty("top_p")] public double Top_P { get; set; }
        [JsonProperty("top_k")] public double Top_K { get; set; }
        [JsonProperty("stream")] public bool Stream { get; set; }
    }

    public partial class Message {

        public Message Clone() {
            return JsonConvert.DeserializeObject<Message>(JsonConvert.SerializeObject(this))!;
        }

        [JsonProperty("role")] public string Role { get; set; } // "system", "user", "assistant"

        [JsonProperty("content")] public string Content { get; set; } // the actual message

        [JsonProperty("think")] public bool Think { get; set; } //if the model should think

        [JsonProperty("tool_calls")] public List<ToolCall>? ToolCalls { get; set; } // if the model wants to use tools

        [JsonProperty("tool_call_id")] public string ToolCallId { get; set; } // used when responding to tool calls
    }

    public class Tool {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("type")] public string Type { get; set; } = "function"; //must be function
        [JsonProperty("function")] public ToolFunction Function { get; set; }
    }

    public class ToolCall {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("type")] public string Type { get; set; } = "function"; //must be function

        [JsonProperty("function")] public ToolFunction Function { get; set; }
    }

    public class ToolFunction {
        [JsonProperty("name")] public string Name { get; set; } //name of tool

        [JsonProperty("description")] public string Description { get; set; } //description of tool

        [JsonProperty("parameters")] public Parameters Parameters { get; set; } //defines all parameters (comes from user)

        [JsonProperty("arguments")] public string Arguments { get; set; } //defines all arguments in Json format (comes from assistant)
    }

    public class Parameters {
        [JsonProperty("type")] public string Type { get; set; } = "object"; //must be object

        [JsonProperty("properties")] public Dictionary<string, PropertyAnnotations> Properties { get; set; }

        [JsonProperty("required")] public List<string> Required { get; set; } //defines the required properties that have to be valid
    }

    public class PropertyAnnotations {
        [JsonProperty("type")] public string Type { get; set; } //integer, string, object etc.. (can also be ["string", "null"] to allow null as option)
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)] public List<string> Enum { get; set; } //used to define possible values of the property
        //[JsonProperty("exclusiveMinimum")] public double ExclusiveMinimum { get; set; } //used to define the minimum value of the property
    }
}
