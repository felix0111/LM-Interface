using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LMInterface
{
    public class LMStudioInterface {

        public static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromHours(1) };
        private static bool _clientInUse;

        private static readonly string _sendUrl = "http://localhost:1234/v1/chat/completions";
        private static readonly string _modelsUrl = "http://localhost:1234/v1/models";

        public static JsonSerializerSettings JsonSettings => new() { NullValueHandling = NullValueHandling.Ignore};

        public async Task ChatCompletion(List<Message> conversation, bool think, bool allowTools, Action<LMResponse> responseHandling) {
            if (_clientInUse) return;
            _clientInUse = true;

            //make request object
            LMRequest request = LMHelper.MakeJsonRequest_Qwen3(conversation, think, allowTools ? new List<Tool>() { new WebTool() } : null, allowTools ? "auto" : "none");

            //convert request object to json
            var json = JsonConvert.SerializeObject(request, JsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //send content and wait for response
            HttpResponseMessage response = await HttpClient.PostAsync(_sendUrl, content);
            response.EnsureSuccessStatusCode();

            //read response as json and convert to response object
            string responseContent = await response.Content.ReadAsStringAsync();
            LMResponse modelResponse = JsonConvert.DeserializeObject<LMResponse>(responseContent, JsonSettings) ?? throw new Exception("JSON could not deserialize response!");

            _clientInUse = false;
            responseHandling.Invoke(modelResponse);
        }

        public static async Task SendToolResults(List<Message> conversation, bool think, Action<List<Message>>toolResultsMessages, Action<LMResponse> actualResponse) {
            if (_clientInUse || !conversation[^1].IsToolCall) return; //shouldnt ever happen
            _clientInUse = true;

            //do not give tools in toolcall response to avoid repeating tool calls
            LMRequest toolCallReponse = LMHelper.MakeJsonRequest_Qwen3(conversation, think, null, "none");
            //re-add the last toolcall message because it's normally sorted out
            toolCallReponse.Messages.Add(conversation[^1]);
            
            //TODO might need to add /no_think to the end of each response when !think
            //get all the tool results
            List<Message> toolResults = await LMHelper.GetToolResults(conversation[^1].ToolCalls!);

            //send the results so the UI can already show them
            toolResultsMessages.Invoke(toolResults);

            //add results of the called tools to the conversation history
            toolCallReponse.Messages.AddRange(toolResults);

            //convert json
            var json = JsonConvert.SerializeObject(toolCallReponse, JsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //send content and wait for response
            HttpResponseMessage response = await HttpClient.PostAsync(_sendUrl, content);
            response.EnsureSuccessStatusCode();

            //read response as json and convert to response object
            string responseContent = await response.Content.ReadAsStringAsync();
            LMResponse modelResponse = JsonConvert.DeserializeObject<LMResponse>(responseContent, JsonSettings) ?? throw new Exception("JSON could not deserialize response!");

            _clientInUse = false;
            actualResponse.Invoke(modelResponse);
        }
    }
}