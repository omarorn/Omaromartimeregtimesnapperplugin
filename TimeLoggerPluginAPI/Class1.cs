using System;

namespace TimeLoggerPluginAPI
{
    public interface ITimeSnapperPlugIn
    {
        Guid PluginGuid { get; }
        string FriendlyName { get; }
        string Description { get; }
        string[] SubscribesTo { get; }
        void HandleEvent(string eventName, object eventData);
        bool Configurable { get; }
        void Configure();
    }

    public class TimeLoggerPlugin : ITimeSnapperPlugIn
    {
        private readonly string ENDPOINT_URL = "https://npsbvriuvfksuvnalrke.supabase.co/functions/v1/timesnapper-events";
        private readonly System.Net.Http.HttpClient httpClient;

        public TimeLoggerPlugin()
        {
            httpClient = new System.Net.Http.HttpClient();
        }

        public Guid PluginGuid => new Guid("B5AEE497-1C29-4A34-8D1E-4F107A0B5C5D");
        public string FriendlyName => "Supabase Time Logger Plugin";
        public string Description => "Logs TimeSnapper events to Supabase edge function endpoint";
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
            System.Threading.Tasks.Task.Run(() => SendToSupabaseAsync(eventName, eventData));
        }

        private async System.Threading.Tasks.Task SendToSupabaseAsync(string eventName, object eventData)
        {
            try
            {
                var payload = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "eventType", eventName },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "data", eventData?.ToString() ?? "No Data" }
                };

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

                string json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

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

        public bool Configurable => false;
        public void Configure()
        {
            Console.WriteLine($"TimeSnapper Supabase endpoint: {ENDPOINT_URL}");
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
