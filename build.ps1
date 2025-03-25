# Download nuget.exe if it doesn't exist
$nugetPath = ".\nuget.exe"
if (-not (Test-Path $nugetPath)) {
    Write-Host "Downloading nuget.exe..."
    Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetPath
}

# Create packages directory if it doesn't exist
$packagesDir = ".\packages"
if (-not (Test-Path $packagesDir)) {
    New-Item -ItemType Directory -Path $packagesDir
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..."
& $nugetPath restore "TimeLoggerPluginAPI.sln" -PackagesDirectory $packagesDir

# Find MSBuild
$vsPath = ""
$msBuildPath = ""

$possibleVSPaths = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)

foreach ($path in $possibleVSPaths) {
    if (Test-Path $path) {
        $msBuildPath = $path
        break
    }
}

if (-not $msBuildPath) {
    Write-Host "Could not find MSBuild. Please install Visual Studio or .NET Framework SDK."
    exit 1
}

Write-Host "Using MSBuild from: $msBuildPath"

# Build the project
Write-Host "Building project..."
& $msBuildPath "TimeLoggerPluginAPI.sln" /p:Configuration=Release /v:minimal

if ($LASTEXITCODE -eq 0) {
    $pluginPath = "C:\Program Files (x86)\TimeSnapper\Plugins\TimeLoggerPlugin.dll"
    
    # Ensure the Plugins directory exists
    $pluginsDir = Split-Path $pluginPath -Parent
    if (-not (Test-Path $pluginsDir)) {
        New-Item -ItemType Directory -Path $pluginsDir -Force
    }
    
    # Copy the built DLL
    Copy-Item "TimeLoggerPluginAPI\bin\Release\TimeLoggerPlugin.dll" $pluginPath -Force
    Write-Host "Plugin deployed to: $pluginPath"
    
    # Verify the file exists and show its details
    if (Test-Path $pluginPath) {
        Write-Host "Plugin file details:"
        Get-Item $pluginPath | Select-Object Name, Length, LastWriteTime
    } else {
        Write-Host "Warning: Plugin file was not found at destination!"
    }
} else {
    Write-Host "Build failed with exit code: $LASTEXITCODE"
    exit 1
}