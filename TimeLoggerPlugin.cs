using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using TimeSnapper.Plugin; // Update this to match the actual namespace from ITimeSnapperPlugIn.dll

namespace TimeLoggerPlugin
{
    public class TimeLogger : ITimeSnapperPlugIn // Make sure this matches the interface name from the DLL
    {
        private readonly string ENDPOINT_URL = "https://npsbvriuvfksuvnalrke.supabase.co/functions/v1/timesnapper-events";
        private readonly HttpClient httpClient;

        public TimeLogger()
        {
            httpClient = new HttpClient();
        }

        // Required interface implementations
        public Guid PluginGuid => new Guid("B5AEE497-1C29-4A34-8D1E-4F107A0B5C5D");
        public string FriendlyName => "Supabase Time Logger Plugin";
        public string Description => "Logs TimeSnapper events to Supabase edge function endpoint";

        // Subscribe to all relevant TimeSnapper events
        public string[] SubscribesTo => new[] { 
            "SnapshotSaved",
            "ProgramStatistics",
            "TimeSpentComputing",
            "DiskSpaceUsage",
            "FlagSaved",
            "ProductivityGrades",
            "ActivityCloud"
        };

        public void HandleEvent(string eventName, object eventData)
        {
            Console.WriteLine($"Event Received: {eventName} at {DateTime.Now}");
            Task.Run(() => SendToSupabaseAsync(eventName, eventData));
        }

        private async Task SendToSupabaseAsync(string eventName, object eventData)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "eventType", eventName },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "data", eventData?.ToString() ?? "No Data" }
                };

                // Add specific data based on event type
                switch (eventName)
                {
                    case "ProgramStatistics":
                        payload["reportType"] = "program_statistics";
                        break;
                    case "TimeSpentComputing":
                        payload["reportType"] = "time_spent";
                        break;
                    case "DiskSpaceUsage":
                        payload["reportType"] = "disk_space";
                        break;
                    case "FlagSaved":
                        payload["reportType"] = "flags";
                        break;
                    case "ProductivityGrades":
                        payload["reportType"] = "productivity";
                        break;
                    case "ActivityCloud":
                        payload["reportType"] = "activity";
                        break;
                    default:
                        payload["reportType"] = "snapshot";
                        break;
                }

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(ENDPOINT_URL, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Successfully sent {eventName} event to Supabase");
                }
                else
                {
                    Console.WriteLine($"Failed to send {eventName} event. Status: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error details: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending {eventName} event: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        // Debug method to send test events
        public async Task SendTestEventAsync(string eventType, string testData)
        {
            await SendToSupabaseAsync(eventType, testData);
        }

        // Optional configuration
        public bool Configurable => false;
        public void Configure()
        {
            Console.WriteLine($"TimeSnapper Supabase endpoint: {ENDPOINT_URL}");
        }

        // Cleanup
        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}