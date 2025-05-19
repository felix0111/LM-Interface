using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace LMInterface
{
    public static class PythonHelper {

        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromMinutes(1) };
        private static bool _clientInUse;

        private static readonly string FastApiUrl = "http://127.0.0.1:8000/";

        public static async Task RunServer() {
            if (await IsServerRunning()) return;

            string command = "python";
            string arguments = "-m uvicorn main:app --port 8000";

            var processInfo = new ProcessStartInfo("cmd.exe") {
                Arguments = $"/C {command} {arguments}",
                WorkingDirectory = Package.Current.InstalledLocation.Path + @"\Python",
                CreateNoWindow = true,
                UseShellExecute = true
            };

            var p = Process.Start(processInfo) ?? throw new Exception("Could not start uvicorn server!");

            //TODO find a way to reliably terminate the python process when the main process ends
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => {
                p.Kill(true);
                p.WaitForExit();
            };
        }

        public static async Task<bool> IsServerRunning() {
            try {
                await HttpClient.GetAsync(FastApiUrl).ConfigureAwait(false);
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        public static async Task<string> ExecutePythonScript(string script) {
            if (_clientInUse) return "Error: HTTPClient already in use!";
            _clientInUse = true;

            if (!await IsServerRunning()) return "Could not reach the python-server!";

            //name must be 'script'!
            var request = new { script };

            //convert request object to json
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //send content and wait for response
            HttpResponseMessage response = await HttpClient.PostAsync(FastApiUrl + "exec-script", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            //read response as json and deserialize
            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            PythonResult result = JsonConvert.DeserializeObject<PythonResult>(responseContent) ?? throw new Exception("JSON could not deserialize fastAPI response!");

            _clientInUse = false;
            return result.Output ?? "Error: " + result.Error;
        }
    }

    public class PythonResult {
        [JsonProperty("output")] public string? Output { get; set; }
        [JsonProperty("error")] public string? Error { get; set; }
    }
}
