#if !NET40
#define NET40
#endif

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using TimeSnapperPluginAPI; // Correct namespace for ITimeSnapperPlugIn

namespace TimeLoggerPluginAPI
{
    public class TimeLogger : ITimeSnapperPlugIn
    {
        // Unique GUID for this plugin. Generate your own GUID to avoid conflicts.
        public Guid PluginGuid => new Guid("B5AEE497-1C29-4A34-8D1E-4F107A0B5C5D");

        // A friendly name for the plugin.
        public string FriendlyName => "Time Logger Plugin";

        // A description of what the plugin does.
        public string Description => "Logs time throughout the day and sends data to a Supabase Edge Function.";

        // Events to which this plugin subscribes.
        public string[] SubscribesTo => new[] { "FlagSaved", "ApplicationActivated", "ApplicationDeactivated" };

        // Configuration https://npsbvriuvfksuvnalrke.supabase.co
        private string _supabaseUrl = "https://npsbvriuvfksuvnalrke.supabase.co/functions/v1/chrome-sync-time-entries";
        private string _apiKey = "";
        private string _configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "TimeLogger", 
            "config.txt");

        public TimeLogger()
        {
            LoadConfiguration();
            
            // Log initialization to help with debugging
            LogToFile("TimeLogger plugin initialized");
        }

        // This method is called when one of the subscribed events occurs.
        public void HandleEvent(string eventName, object eventData)
        {
            Console.WriteLine($"Event Received: {eventName} at {DateTime.Now}");
            string data = eventData != null ? eventData.ToString() : "No Data";
            
            // Log to file for debugging
            LogToFile($"Event: {eventName}, Data: {data}");
            
            // Send the logged data asynchronously to the Edge Function.
            Task.Run(() => SendToSupabaseAsync(eventName, data));
        }

        // Sends data to the Supabase Edge Function via an HTTP POST request.
        private async Task SendToSupabaseAsync(string eventName, string eventData)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set up the Supabase Edge Function endpoint
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                    
                    var payload = new
                    {
                        eventName = eventName,
                        eventData = eventData,
                        applicationInfo = GetActiveApplication(),
                        timestamp = DateTime.UtcNow.ToString("o")
                    };

                    string json = JsonConvert.SerializeObject(payload);
                    using (var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"))
                    {
                        HttpResponseMessage response = await client.PostAsync(_supabaseUrl, content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            LogToFile($"Data sent successfully. Response: {responseContent}");
                        }
                        else
                        {
                            string errorContent = await response.Content.ReadAsStringAsync();
                            LogToFile($"Failed to send data. Status Code: {response.StatusCode}, Error: {errorContent}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Error sending data: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Get information about active application (simplified - you may want to enhance this)
        private string GetActiveApplication()
        {
            try
            {
                // Real implementation would get the active window title, etc.
                return "Active application info";
            }
            catch
            {
                return "Unknown";
            }
        }

        // Simple logging to help with debugging
        private void LogToFile(string message)
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TimeLogger");
                
                Directory.CreateDirectory(logDir);
                
                string logPath = Path.Combine(logDir, "timelogger.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] {message}\n");
            }
            catch (Exception ex)
            {
                // Enhanced error handling for logging
                Console.WriteLine($"Error logging to file: {ex.Message}");
            }
        }

        // Load configuration from a simple text file
        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string[] lines = File.ReadAllLines(_configFilePath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("SupabaseUrl="))
                        {
                            _supabaseUrl = line.Substring("SupabaseUrl=".Length);
                        }
                        else if (line.StartsWith("ApiKey="))
                        {
                            _apiKey = line.Substring("ApiKey=".Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
            }
        }

        // Save configuration to a simple text file
        private void SaveConfiguration()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath));
                File.WriteAllText(_configFilePath, 
                    $"SupabaseUrl={_supabaseUrl}\nApiKey={_apiKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        // Replace console-based configuration with a proper Windows Form
        public void Configure()
        {
            // Instantiate and display the configuration form
            using (var configForm = new ConfigurationForm(_supabaseUrl, _apiKey))
            {
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // Update configuration with values from the form
                    _supabaseUrl = configForm.SupabaseUrl;
                    _apiKey = configForm.ApiKey;
                    SaveConfiguration();
                    LogToFile("Configuration updated via configuration form");
                }
            }
        }
    }

    // Configuration form for the TimeLogger plugin
    public class ConfigurationForm : Form
    {
        private TextBox txtSupabaseUrl;
        private TextBox txtApiKey;
        private Button btnOK;
        private Button btnCancel;
        private Label lblSupabaseUrl;
        private Label lblApiKey;

        public string SupabaseUrl { get; private set; }
        public string ApiKey { get; private set; }

        public ConfigurationForm(string currentSupabaseUrl, string currentApiKey)
        {
            SupabaseUrl = currentSupabaseUrl;
            ApiKey = currentApiKey;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "TimeLogger Plugin Configuration";
            this.Size = new System.Drawing.Size(500, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Supabase URL label and textbox
            lblSupabaseUrl = new Label();
            lblSupabaseUrl.Text = "Supabase URL:";
            lblSupabaseUrl.Location = new System.Drawing.Point(20, 20);
            lblSupabaseUrl.Size = new System.Drawing.Size(100, 23);

            txtSupabaseUrl = new TextBox();
            txtSupabaseUrl.Location = new System.Drawing.Point(130, 20);
            txtSupabaseUrl.Size = new System.Drawing.Size(330, 23);
            txtSupabaseUrl.Text = SupabaseUrl;

            // API Key label and textbox
            lblApiKey = new Label();
            lblApiKey.Text = "API Key:";
            lblApiKey.Location = new System.Drawing.Point(20, 60);
            lblApiKey.Size = new System.Drawing.Size(100, 23);

            txtApiKey = new TextBox();
            txtApiKey.Location = new System.Drawing.Point(130, 60);
            txtApiKey.Size = new System.Drawing.Size(330, 23);
            txtApiKey.Text = ApiKey;
            // Add password masking for security
            txtApiKey.PasswordChar = '*';

            // OK button
            btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new System.Drawing.Point(285, 110);
            btnOK.Size = new System.Drawing.Size(75, 30);
            btnOK.Click += new EventHandler(BtnOK_Click);

            // Cancel button
            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(385, 110);
            btnCancel.Size = new System.Drawing.Size(75, 30);

            // Set form defaults
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Add controls to the form
            this.Controls.Add(lblSupabaseUrl);
            this.Controls.Add(txtSupabaseUrl);
            this.Controls.Add(lblApiKey);
            this.Controls.Add(txtApiKey);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SupabaseUrl = txtSupabaseUrl.Text;
            ApiKey = txtApiKey.Text;
            // DialogResult is set automatically by the button property
        }
    }
}