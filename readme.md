# TimeSnapper Supabase Plugin

A TimeSnapper plugin that logs activity data to a Supabase edge function endpoint. This plugin captures various TimeSnapper events and sends them to a configured Supabase endpoint for analysis and tracking.

## Features

- Captures multiple TimeSnapper events:
  - SnapshotSaved
  - ProgramStatistics
  - TimeSpentComputing
  - DiskSpaceUsage
  - FlagSaved
  - ProductivityGrades
  - ActivityCloud
- Sends event data to Supabase edge function
- Real-time activity logging
- Configurable endpoint

## Build Requirements

- Visual Studio 2019+ or Visual Studio Build Tools 2019+
- .NET Framework 4.8 SDK
- PowerShell 5.1 or higher
- Administrator privileges (for deployment)

## Building and Deploying

1. **Clone the Repository**
   ```
   git clone https://github.com/yourusername/timesnapper-supabase-plugin.git
   cd timesnapper-supabase-plugin
   ```

2. **Build and Deploy**
   - Run the build script (requires administrator privileges):
     ```
     powershell -ExecutionPolicy Bypass -File build-plugin.ps1
     ```
   - The script will:
     - Clean build directories
     - Restore NuGet packages
     - Build the solution
     - Deploy to TimeSnapper Plugins directory

3. **Verify Installation**
   - Restart TimeSnapper
   - The plugin should be loaded automatically
   - Check TimeSnapper logs for plugin initialization

## Configuration

The plugin sends data to the following Supabase endpoint:
```
https://npsbvriuvfksuvnalrke.supabase.co/functions/v1/timesnapper-events
```

## Testing

1. **Use the Debug Dashboard**
   - Open `dashboard.html` in a web browser
   - Use the "Test Events" tab to send test events
   - View recent events in the "Recent Events" tab

2. **Check Event Logging**
   - Events are logged to the Supabase database
   - Check the TimeSnapper console for real-time logging
   - Use the Supabase dashboard to view stored events

## Troubleshooting

1. **Plugin Not Loading**
   - Verify the DLL is in the correct location
   - Check TimeSnapper logs for errors
   - Ensure proper permissions on Plugins folder

2. **Events Not Sending**
   - Check network connectivity
   - Verify Supabase endpoint is accessible
   - Review plugin logs for errors

3. **Build Issues**
   - Run build script as administrator
   - Ensure .NET Framework 4.8 SDK is installed
   - Clear the build directories manually if needed

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues or questions:
- Open an issue on GitHub
- Contact TimeSnapper support for plugin-related questions
- Check Supabase documentation for endpoint issues
