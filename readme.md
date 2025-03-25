# TimeSnapper Plugin Documentation

## English Instructions

### Plugin Development and Deployment Guide

1. **Project Configuration**
   - Target .NET Framework 4.8 in the project file
   - Reference ITimeSnapperPlugIn.dll correctly
   - Ensure plugin class implements required interface
   - Build using Visual Studio or C# Dev Kit

2. **Deployment Steps**
   - Build the project to generate DLL
   - Rename output to end with "Plugin.dll"
   - Copy to: C:\Program Files (x86)\TimeSnapper\Plugins\
   - Restart TimeSnapper to load plugin

3. **Testing**
   - Check TimeSnapper logs for plugin loading
   - Verify event handling (SnapshotSaved, FlagSaved)
   - Confirm data transmission to X-Function endpoint

---

## Íslenskar Leiðbeiningar

### Leiðbeiningar fyrir Þróun og Útsetningu Viðbótar

1. **Verkefnisstillingar**
   - Notaðu .NET Framework 4.8 í verkefnaskrá
   - Vísaðu rétt í ITimeSnapperPlugIn.dll
   - Tryggðu að viðbótarklasinn útfæri rétt viðmót
   - Byggðu með Visual Studio eða C# Dev Kit

2. **Útsetningarskref**
   - Byggðu verkefnið til að búa til DLL
   - Endurnefndu úttak þannig að það endi á "Plugin.dll"
   - Afritaðu í: C:\Program Files (x86)\TimeSnapper\Plugins\
   - Endurræstu TimeSnapper til að hlaða viðbót

3. **Prófun**
   - Athugaðu TimeSnapper skrár fyrir hleðslu viðbótar
   - Staðfestu meðhöndlun atburða (SnapshotSaved, FlagSaved)
   - Staðfestu gagnasendingu til X-Function endapunkts

---

## Development Notes

### Project Structure
```
TimeLoggerPlugin/
├── TimeLoggerPlugin.cs     # Main plugin implementation
├── TimeLoggerPlugin.csproj # Project configuration
└── Properties/
    └── AssemblyInfo.cs     # Assembly information
```

### Required Interface Implementation
```csharp
public class TimeLogger : ITimeSnapperPlugIn
{
    public Guid PluginGuid => new Guid("B5AEE497-1C29-4A34-8D1E-4F107A0B5C5D");
    public string FriendlyName => "Time Logger Plugin";
    public string Description => "Logs time throughout the day and sends data to a super-based X-Function.";
    public string[] SubscribesTo => new[] { "SnapshotSaved", "FlagSaved" };
    
    // Event handling implementation
    public void HandleEvent(string eventName, object eventData)
    {
        // Implementation details in TimeLoggerPlugin.cs
    }
}
```

### Configuration
1. Update .NET Framework version in .csproj:
```xml
<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
```

2. Ensure correct DLL reference:
```xml
<Reference Include="ITimeSnapperPlugIn">
    <HintPath>.\ITimeSnapperPlugIn.dll</HintPath>
</Reference>
```

### Building
```batch
dotnet build
```

### Deployment Checklist
- [ ] Built DLL exists
- [ ] DLL name ends with "Plugin.dll"
- [ ] DLL copied to TimeSnapper Plugins folder
- [ ] TimeSnapper restarted
- [ ] Plugin appears in TimeSnapper logs
- [ ] Events are being captured
- [ ] Data successfully sent to X-Function

## Troubleshooting

1. **Plugin Not Loading**
   - Verify DLL name ends with "Plugin.dll"
   - Check .NET Framework version matches
   - Confirm DLL is in correct Plugins folder

2. **Events Not Captured**
   - Verify SubscribesTo array includes correct event names
   - Check TimeSnapper logs for errors
   - Ensure HandleEvent method is properly implemented

3. **Data Not Sending**
   - Verify X-Function endpoint URL is correct
   - Check network connectivity
   - Review error logging in HandleEvent method

## Support

For issues or questions:
- TimeSnapper Documentation: [TimeSnapper Plugin Development](https://wiki.timesnapper.com/index.php?title=Plugin_Development)
- Report Issues: [GitHub Issues](https://github.com/yourusername/TimeLoggerPlugin/issues)
