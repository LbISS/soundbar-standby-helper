@echo off
setlocal enabledelayedexpansion

:: Check for --debug flag
set DEBUG_MODE=0
if /I "%1"=="--debug" set DEBUG_MODE=1

echo ============================================
echo  Soundbar Standby Helper - Release Builder
echo ============================================
echo.

:: Get version from project file - find the PropertyGroup Version tag
set VERSION=
for /f "tokens=*" %%a in ('findstr /C:"<Version>" SoundbarStandbyHelper.csproj') do (
    set "line=%%a"
    set "line=!line:*<Version>=!"
    set "line=!line:</Version>=!"
    set "VERSION=!line: =!"
)

:: Trim spaces
set VERSION=%VERSION: =%

if "%VERSION%"=="" (
    echo WARNING: Could not read version from project file, using default
    set VERSION=1.0.0
)

echo Building version: %VERSION%
echo.

:: Clean previous builds
echo Cleaning previous builds...
if exist publish rd /s /q publish
if exist releases rd /s /q releases
mkdir publish
mkdir releases

:: Skip self-contained builds in debug mode
if %DEBUG_MODE%==1 goto :SkipSelfContained

echo.
echo ============================================
echo  Building Self-Contained Versions
echo  (Includes .NET Runtime, ~30-70MB each)
echo ============================================
echo.

echo [1/4] Building Windows x64 (self-contained)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/win-x64-self-contained
if errorlevel 1 (
    echo ERROR: Windows x64 build failed!
    pause
    exit /b 1
)

echo [2/4] Building Linux x64 (self-contained)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/linux-x64-self-contained
if errorlevel 1 (
    echo ERROR: Linux x64 build failed!
    pause
    exit /b 1
)

echo [3/4] Building macOS x64 (self-contained)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/osx-x64-self-contained
if errorlevel 1 (
    echo ERROR: macOS x64 build failed!
    pause
    exit /b 1
)

echo [4/4] Building macOS ARM64 (self-contained)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/osx-arm64-self-contained
if errorlevel 1 (
    echo ERROR: macOS ARM64 build failed!
    pause
    exit /b 1
)

:SkipSelfContained

echo.
echo ============================================
echo  Building Framework-Dependent Versions
if %DEBUG_MODE%==1 (
    echo  (Debug Mode - Windows x64 only^)
) else (
    echo  (Requires .NET 8 Runtime, ~200kb each^)
)
echo ============================================
echo.

echo [1/4] Building Windows x64 (framework-dependent)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish/win-x64
if errorlevel 1 (
    echo ERROR: Windows x64 framework-dependent build failed!
    pause
    exit /b 1
)

:: Skip other platforms in debug mode
if %DEBUG_MODE%==1 goto :SkipOtherPlatforms

echo [2/4] Building Linux x64 (framework-dependent)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r linux-x64 --self-contained false -p:PublishSingleFile=true -o ./publish/linux-x64
if errorlevel 1 (
    echo ERROR: Linux x64 framework-dependent build failed!
    pause
    exit /b 1
)

echo [3/4] Building macOS x64 (framework-dependent)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-x64 --self-contained false -p:PublishSingleFile=true -o ./publish/osx-x64
if errorlevel 1 (
    echo ERROR: macOS x64 framework-dependent build failed!
    pause
    exit /b 1
)

echo [4/4] Building macOS ARM64 (framework-dependent)...
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-arm64 --self-contained false -p:PublishSingleFile=true -o ./publish/osx-arm64
if errorlevel 1 (
    echo ERROR: macOS ARM64 framework-dependent build failed!
    pause
    exit /b 1
)

:SkipOtherPlatforms

:: Skip packaging in debug mode
if %DEBUG_MODE%==1 goto :SkipPackaging

echo.
echo ============================================
echo  Creating Release Archives
echo ============================================
echo.

echo Creating ZIP archives...

:: Self-contained versions
echo Packaging win-x64-self-contained...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\win-x64-self-contained\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-win-x64-self-contained.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-win-x64-self-contained.zip

echo Packaging linux-x64-self-contained...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\linux-x64-self-contained\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-linux-x64-self-contained.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-linux-x64-self-contained.zip

echo Packaging osx-x64-self-contained...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\osx-x64-self-contained\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-osx-x64-self-contained.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-osx-x64-self-contained.zip

echo Packaging osx-arm64-self-contained...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\osx-arm64-self-contained\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-osx-arm64-self-contained.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-osx-arm64-self-contained.zip

:: Framework-dependent versions
echo Packaging win-x64...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\win-x64\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-win-x64.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-win-x64.zip

echo Packaging linux-x64...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\linux-x64\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-linux-x64.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-linux-x64.zip

echo Packaging osx-x64...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\osx-x64\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-osx-x64.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-osx-x64.zip

echo Packaging osx-arm64...
mkdir ".\publish\SoundbarStandbyHelper-v%VERSION%" 2>nul
xcopy /E /I /Y ".\publish\osx-arm64\*" ".\publish\SoundbarStandbyHelper-v%VERSION%\" >nul
powershell -Command "Get-ChildItem -Path './publish/SoundbarStandbyHelper-v%VERSION%' -Filter *.pdb -Recurse | Remove-Item -Force"
powershell -Command "Compress-Archive -Path './publish/SoundbarStandbyHelper-v%VERSION%' -DestinationPath './releases/SoundbarStandbyHelper-v%VERSION%-osx-arm64.zip' -Force"
rd /s /q ".\publish\SoundbarStandbyHelper-v%VERSION%"
echo Created: SoundbarStandbyHelper-v%VERSION%-osx-arm64.zip

echo.
echo Cleaning up temporary build files...
rd /s /q publish
echo Deleted publish folder

:SkipPackaging

echo.
echo ============================================
echo  Build Summary
echo ============================================
echo.
echo Version: %VERSION%
echo.
if %DEBUG_MODE%==1 (
    echo Debug Build:
    echo   - Output directory: publish\win-x64
    echo.
    echo Files in output:
    dir /B publish\win-x64
) else (
    echo Self-Contained Builds (includes .NET runtime^):
    echo   - SoundbarStandbyHelper-v%VERSION%-win-x64-self-contained.zip
    echo   - SoundbarStandbyHelper-v%VERSION%-linux-x64-self-contained.zip
    echo   - SoundbarStandbyHelper-v%VERSION%-osx-x64-self-contained.zip
    echo   - SoundbarStandbyHelper-v%VERSION%-osx-arm64-self-contained.zip
    echo.
    echo Framework-Dependent Builds (requires .NET 8 runtime^):
    echo   - SoundbarStandbyHelper-v%VERSION%-win-x64.zip
    echo   - SoundbarStandbyHelper-v%VERSION%-linux-x64.zip
    echo   - SoundbarStandbyHelper-v%VERSION%-osx-x64.zip
    echo   - SoundbarStandbyHelper-v%VERSION%-osx-arm64.zip
    echo.
    echo All release files are in the 'releases' folder.
    echo.
    echo ============================================
    echo  Next Steps
    echo ============================================
    echo.
    echo 1. Test the executables on target platforms
    echo 2. Go to: https://github.com/LbISS/soundbar-standby-helper/releases
    echo 3. Click "Draft a new release"
    echo 4. Create tag: v%VERSION%
    echo 5. Upload files from the 'releases' folder
    echo 6. Add release notes and publish
)
echo.
echo ============================================

pause
