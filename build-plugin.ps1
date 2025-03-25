# Function to ensure we're running as administrator
function Ensure-AdminPrivileges {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    $adminRole = [Security.Principal.WindowsBuiltInRole]::Administrator
    
    if (-not $principal.IsInRole($adminRole)) {
        Write-Host "Requesting administrator privileges..."
        $argList = "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`""
        Start-Process powershell -Verb RunAs -ArgumentList $argList -Wait
        exit
    }
}

# Ensure we have admin rights
Ensure-AdminPrivileges

# Set UTF-8 encoding without BOM
$PSDefaultParameterValues['*:Encoding'] = 'utf8'

# Function to ensure directory exists and is empty
function Ensure-CleanDirectory {
    param([string]$path)
    if (Test-Path $path) {
        Remove-Item -Path (Join-Path $path '*') -Recurse -Force -ErrorAction SilentlyContinue
    } else {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

Write-Host "Cleaning build directories..."
Ensure-CleanDirectory ".\TimeLoggerPluginAPI\bin"
Ensure-CleanDirectory ".\TimeLoggerPluginAPI\obj"
Ensure-CleanDirectory ".\packages"

# Download nuget.exe if it doesn't exist
if (-not (Test-Path ".\nuget.exe")) {
    Write-Host "Downloading nuget.exe..."
    try {
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile ".\nuget.exe" -ErrorAction Stop
    }
    catch {
        Write-Host "Error downloading nuget.exe: $_" -ForegroundColor Red
        exit 1
    }
}

# Restore NuGet packages with explicit solution directory and packages directory
Write-Host "Restoring NuGet packages..."
try {
    $nugetOutput = .\nuget.exe restore TimeLoggerPluginAPI.sln -SolutionDirectory . -PackagesDirectory packages -Verbosity detailed 2>&1
    Write-Host $nugetOutput
    if ($LASTEXITCODE -ne 0) {
        Write-Host "NuGet restore failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
catch {
    Write-Host "Exception during NuGet restore:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Find MSBuild
$msBuildPath = $null
$vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

if (Test-Path $vsWhere) {
    $installationPath = & $vsWhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    if ($installationPath) {
        $msBuildPath = Join-Path $installationPath 'MSBuild\Current\Bin\MSBuild.exe'
    }
}

if (-not (Test-Path $msBuildPath)) {
    $msBuildPath = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
}

if (-not (Test-Path $msBuildPath)) {
    Write-Host "Could not find MSBuild. Please install Visual Studio or .NET Framework SDK." -ForegroundColor Red
    exit 1
}

Write-Host "Using MSBuild from: $msBuildPath"

# Build the solution
Write-Host "Building solution..."
try {
    $msbuildOutput = & $msBuildPath TimeLoggerPluginAPI.sln /p:Configuration=Release /v:m 2>&1
    Write-Host $msbuildOutput
    if ($LASTEXITCODE -ne 0) {
        Write-Host "MSBuild failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
catch {
    Write-Host "Exception during MSBuild:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Green
    
    # Copy to TimeSnapper Plugins directory
    $pluginPath = "${env:ProgramFiles(x86)}\TimeSnapper\Plugins\TimeLoggerPlugin.dll"
    $pluginsDir = Split-Path $pluginPath -Parent
    
    # Create Plugins directory if it doesn't exist
    if (-not (Test-Path $pluginsDir)) {
        Write-Host "Creating Plugins directory..."
        try {
            New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null
        }
        catch {
            Write-Host "Error creating plugins directory: $_" -ForegroundColor Red
            exit 1
        }
    }
    
    # Copy the file
    Write-Host "Copying plugin to TimeSnapper Plugins directory..."
    $sourcePath = "TimeLoggerPluginAPI\bin\Release\TimeLoggerPlugin.dll"
    
    try {
        Copy-Item $sourcePath $pluginPath -Force -ErrorAction Stop
        if (Test-Path $pluginPath) {
            Write-Host "Plugin deployed successfully to: $pluginPath" -ForegroundColor Green
            Get-Item $pluginPath | Select-Object Name, Length, LastWriteTime | Format-Table
        }
        else {
            Write-Host "Failed to verify plugin deployment at $pluginPath" -ForegroundColor Red
            exit 1
        }
    }
    catch {
        Write-Host "Error copying plugin: $_" -ForegroundColor Red
        Write-Host "Please ensure you have administrator privileges." -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Build process completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")