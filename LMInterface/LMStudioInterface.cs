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

        public readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromHours(1) };
        private bool _clientInUse;

        private readonly string _sendUrl = "http://localhost:1234/v1/chat/completions";
        private readonly string _modelsUrl = "http://localhost:1234/v1/models";

        public JsonSerializerSettings JsonSettings => new() { NullValueHandling = NullValueHandling.Ignore };

        public async Task ChatCompletion(List<Message> conversation, bool think, Action<LMResponse> responseHandling) {
            if (_clientInUse) return;
            _clientInUse = true;

            LMRequest request = LMHelper.MakeJsonRequest_Qwen3(conversation, think);

            //convert request object to json
            var json = JsonConvert.SerializeObject(request, JsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //send content and wait for response
            HttpResponseMessage response = await HttpClient.PostAsync(_sendUrl, content);
            response.EnsureSuccessStatusCode();

            //read response as json and convert to response object
            string responseContent = await response.Content.ReadAsStringAsync();
            LMResponse modelResponse = JsonConvert.DeserializeObject<LMResponse>(responseContent, JsonSettings) ?? throw new Exception("JSON could not deserialize response!");

            //if model calls tools
            if (modelResponse.IsToolCall) {
                await SupplyToolResult(conversation, think, modelResponse, lmResponse => {
                    _clientInUse = false;
                    responseHandling.Invoke(lmResponse);
                });
            } else {
                _clientInUse = false;
                responseHandling.Invoke(modelResponse);
            }
        }

        //current last message should be the user supplying the tools
        private async Task SupplyToolResult(List<Message> conversation, bool think, LMResponse toolRequest, Action<LMResponse> actualResponse) {
            LMRequest toolCallReponse = LMHelper.MakeJsonRequest_Qwen3(conversation, think);

            //do not give tools in toolcall response to avoid repeating tool calls
            toolCallReponse.ToolChoice = "none";
            toolCallReponse.Tools = null;

            //add the "tool call"-response to the conversation history
            toolCallReponse.Messages.Add(toolRequest.Choices[0].Message);

            //add results of all called tools and add them to the conversation
            List<Message> toolResults = await LMHelper.GetToolResults(toolRequest.Choices[0].Message.ToolCalls);
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

            actualResponse.Invoke(modelResponse);
        }
    }
}