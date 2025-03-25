using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        private readonly HttpClient httpClient;

        public TimeLoggerPlugin()
        {
            httpClient = new HttpClient();
        }

        public Guid PluginGuid
        {
            get { return new Guid("B5AEE497-1C29-4A34-8D1E-4F107A0B5C5D"); }
        }

        public string FriendlyName
        {
            get { return "Supabase Time Logger Plugin"; }
        }

        public string Description
        {
            get { return "Logs TimeSnapper events to Supabase edge function endpoint"; }
        }

        public string[] SubscribesTo
        {
            get
            {
                return new[] { 
                    "SnapshotSaved",
                    "ProgramStatistics",
                    "TimeSpentComputing",
                    "DiskSpaceUsage",
                    "FlagSaved",
                    "ProductivityGrades",
                    "ActivityCloud"
                };
            }
        }

        public void HandleEvent(string eventName, object eventData)
        {
            Console.WriteLine(string.Format("Event Received: {0} at {1}", eventName, DateTime.Now));
            Task.Run(() => SendToSupabaseAsync(eventName, eventData));
        }

        private async Task SendToSupabaseAsync(string eventName, object eventData)
        {
            try
            {
                var payload = new Dictionary<string, object>();
                payload.Add("eventType", eventName);
                payload.Add("timestamp", DateTime.UtcNow.ToString("o"));
                payload.Add("data", eventData != null ? eventData.ToString() : "No Data");

                switch (eventName)
                {
                    case "ProgramStatistics":
                        payload.Add("reportType", "program_statistics");
                        break;
                    case "TimeSpentComputing":
                        payload.Add("reportType", "time_spent");
                        break;
                    case "DiskSpaceUsage":
                        payload.Add("reportType", "disk_space");
                        break;
                    case "FlagSaved":
                        payload.Add("reportType", "flags");
                        break;
                    case "ProductivityGrades":
                        payload.Add("reportType", "productivity");
                        break;
                    case "ActivityCloud":
                        payload.Add("reportType", "activity");
                        break;
                    default:
                        payload.Add("reportType", "snapshot");
                        break;
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(ENDPOINT_URL, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(string.Format("Successfully sent {0} event to Supabase", eventName));
                }
                else
                {
                    Console.WriteLine(string.Format("Failed to send {0} event. Status: {1}", eventName, response.StatusCode));
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(string.Format("Error details: {0}", errorContent));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error sending {0} event: {1}", eventName, ex.Message));
                if (ex.InnerException != null)
                {
                    Console.WriteLine(string.Format("Inner exception: {0}", ex.InnerException.Message));
                }
            }
        }

        public bool Configurable
        {
            get { return false; }
        }

        public void Configure()
        {
            Console.WriteLine(string.Format("TimeSnapper Supabase endpoint: {0}", ENDPOINT_URL));
        }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            }
        }
    }
}
