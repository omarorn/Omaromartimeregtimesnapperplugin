@echo off
echo Searching for MSBuild...

REM Try to find MSBuild in common Visual Studio installation locations
set FOUND=0

REM Check VS2022 path
if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

REM Check VS2019 path
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

REM Check .NET Framework path
if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" (
    set MSBUILD_PATH="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" (
    set MSBUILD_PATH="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
    set FOUND=1
    goto BUILD
)

if %FOUND%==0 (
    echo MSBuild not found. Please install Visual Studio or .NET Framework.
    exit /b 1
)

:BUILD
echo Found MSBuild at %MSBUILD_PATH%
echo Building TimeLoggerPlugin...

REM Download NuGet.exe if it doesn't exist
if not exist "NuGet.exe" (
    echo Downloading NuGet.exe...
    powershell -Command "Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile NuGet.exe"
    if not exist "NuGet.exe" (
        echo Failed to download NuGet.exe. Will attempt to build without explicit restore.
    ) else (
        echo NuGet.exe downloaded successfully.
    )
)

REM Restore NuGet packages
echo Restoring NuGet packages...
if exist "NuGet.exe" (
    .\NuGet.exe restore TimeLoggerPlugin.csproj
) else (
    echo Attempting to restore packages using MSBuild...
    %MSBUILD_PATH% TimeLoggerPlugin.csproj /t:Restore /p:Configuration=Release
)

REM Build the project
echo Building project...
%MSBUILD_PATH% TimeLoggerPlugin.csproj /p:Configuration=Release /p:TargetFrameworkVersion=v4.8

echo Build complete!
if exist bin\Release\TimeLoggerPlugin.dll (
    echo TimeLoggerPlugin.dll was successfully built at bin\Release\TimeLoggerPlugin.dll
    
    echo.
    echo Would you like to copy the plugin to TimeSnapper plugins directory? (Y/N)
    choice /C YN /M "Copy to TimeSnapper:"
    
    if errorlevel 2 goto END
    if errorlevel 1 (
        echo.
        echo Copying plugin to TimeSnapper Plugins directory...
        
        if exist "C:\Program Files\TimeSnapper\Plugins\" (
            copy /Y "bin\Release\TimeLoggerPlugin.dll" "C:\Program Files\TimeSnapper\Plugins\"
            echo Plugin successfully copied.
        ) else if exist "C:\Program Files (x86)\TimeSnapper\Plugins\" (
            copy /Y "bin\Release\TimeLoggerPlugin.dll" "C:\Program Files (x86)\TimeSnapper\Plugins\"
            echo Plugin successfully copied.
        ) else (
            echo TimeSnapper Plugins directory not found.
            echo Please manually copy bin\Release\TimeLoggerPlugin.dll to your TimeSnapper Plugins directory.
        )
    )
) else (
    echo Build might have failed. Check the output for errors.
)

:END
pause