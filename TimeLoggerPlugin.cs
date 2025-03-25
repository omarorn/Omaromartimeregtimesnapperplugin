using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeSnapper.Plugin; // Ensure this matches the namespace defined in ITimeSnapperPlugIn.dll

namespace TimeLoggerPlugin
{
    public class TimeLoggerPlugin : ITimeSnapperPlugIn
    {
        private readonly string endpointUrl = "https://npsbvriuvfksuvnalrke.supabase.co/functions/v1/timesnapper-events";
        private readonly HttpClient httpClient;

        public TimeLoggerPlugin()
        {
            httpClient = new HttpClient();
        }

        public Guid PluginGuid => new Guid("B5AEE497-1C29-4A34-8D1E-4F107A0B5C5D");

        public string FriendlyName => "Supabase Time Logger Plugin";

        public string Description => "Logs TimeSnapper events to the Supabase edge function endpoint.";

        public string[] SubscribesTo => new string[]
        {
            "SnapshotSaved",
            "ProgramStatistics",
            "TimeSpentComputing",
            "DiskSpaceUsage",
            "FlagSaved",
            "ProductivityGrades",
            "ActivityCloud"
        };

        public bool Configurable => false;

        public void Configure()
        {
            // No configuration is needed.
        }

        public void HandleEvent(string eventName, object eventData)
        {
            Console.WriteLine($"Event Received: {eventName} at {DateTime.Now}");
            Task.Run(async () => await SendToSupabaseAsync(eventName, eventData));
        }

        private async Task SendToSupabaseAsync(string eventName, object eventData)
        {
            try
            {
                var payload = new
                {
                    eventType = eventName,
                    timestamp = DateTime.UtcNow.ToString("o"),
                    data = eventData != null ? eventData.ToString() : "No Data",
                    reportType = GetReportType(eventName)
                };

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpointUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Successfully sent {eventName} event to Supabase.");
                }
                else
                {
                    Console.WriteLine($"Failed to send {eventName} event. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending {eventName} event: {ex.Message}");
            }
        }

        private string GetReportType(string eventName)
        {
            switch (eventName)
            {
                case "ProgramStatistics":
                    return "program_statistics";
                case "TimeSpentComputing":
                    return "time_spent";
                case "DiskSpaceUsage":
                    return "disk_space";
                case "FlagSaved":
                    return "flags";
                case "ProductivityGrades":
                    return "productivity";
                case "ActivityCloud":
                    return "activity";
                default:
                    return "snapshot";
            }
        }
    }
}