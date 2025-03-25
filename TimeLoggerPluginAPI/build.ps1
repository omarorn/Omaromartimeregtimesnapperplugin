# Install required packages
nuget install TimeLoggerPluginAPI/packages.config -OutputDirectory packages

# Build the project
$msBuildPath = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msBuildPath)) {
    $msBuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
}
if (-not (Test-Path $msBuildPath)) {
    $msBuildPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
}

Write-Host "Using MSBuild from: $msBuildPath"

& $msBuildPath TimeLoggerPluginAPI/TimeLoggerPluginAPI.csproj /p:Configuration=Release /v:minimal

# Copy to TimeSnapper Plugins directory if build was successful
if ($LASTEXITCODE -eq 0) {
    $pluginPath = "C:\Program Files (x86)\TimeSnapper\Plugins\TimeLoggerPlugin.dll"
    Copy-Item "TimeLoggerPluginAPI\bin\Release\TimeLoggerPluginAPI.dll" $pluginPath -Force
    Write-Host "Plugin deployed to: $pluginPath"
} else {
    Write-Host "Build failed with exit code: $LASTEXITCODE"
    exit 1
}