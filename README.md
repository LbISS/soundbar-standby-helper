# Soundbar Standby Helper

A cross-platform .NET application that prevents soundbars from entering standby mode by periodically playing a sound file.

## The Problem

Many soundbars, especially those sold in the EU market, automatically enter standby mode after a few minutes of inactivity due to EU energy regulations (ErP Directive). While this is environmentally friendly, it can be frustrating when:

- You need to manually wake up your soundbar before every online meeting
- You're using the soundbar in a non-EU country where this behavior is unnecessary
- You frequently switch between different audio sources
- You want your soundbar to remain ready for notifications or alerts

**SoundbarStandbyHelper** solves this by playing a sound file at regular intervals, keeping your soundbar active and preventing it from entering standby mode.

## Features

- ✅ **Cross-platform**: Runs on Windows, Linux, and macOS
- ✅ **Configurable**: Customize sound file, playback interval, and behavior via JSON config
- ✅ **System Tray Support**: Minimize to tray on Windows (optional)

## Requirements

- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (or build as self-contained)
- A `.wav` sound file (a quiet low-frequency and short sound file is recommended)

## Installation

### Option 1: Download Pre-built Release
**Note**: You may download normal binaries if you have .net runtime installed (typical case) or self-contained binaries. Self-contained builds include the .NET runtime, resulting in larger executables (~70MB) but require no .Net runtime installation on the target machine.
1. Download the latest release for your platform from the [Releases](../../releases) page.
3. Extract the archive to a folder of your choice
4. Run the executable

### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/LbISS/soundbar-standby-helper.git
cd soundbar-standby-helper

# Publish the application in mainstream way
# Windows x64
dotnet publish SoundbarStandbyHelper.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish/win-x64

# Linux x64
dotnet publish SoundbarStandbyHelper.csproj -c Release -r linux-x64 --self-contained false -p:PublishSingleFile=true -o ./publish/linux-x64

# macOS x64 (Intel)
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-x64 --self-contained false -p:PublishSingleFile=true -o ./publish/osx-x64

# macOS ARM64 (Apple Silicon M1/M2/M3)
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-arm64 --self-contained false -p:PublishSingleFile=true -o ./publish/osx-arm64

# Or publish as self-contained (no .NET runtime required)
# Windows x64
dotnet publish SoundbarStandbyHelper.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/win-x64-self-contained

# Linux x64
dotnet publish SoundbarStandbyHelper.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/linux-x64-self-contained

# macOS x64 (Intel)
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/osx-x64-self-contained

# macOS ARM64 (Apple Silicon M1/M2/M3)
dotnet publish SoundbarStandbyHelper.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish/osx-arm64-self-contained
```

The compiled output will be in: `publish/`

## Configuration

The application uses a `config.json` file for configuration. If the file doesn't exist, it will be created automatically with default values on first run.

### config.json

```json
{
  "SoundFilePath": "sound.wav",
  "MinimizeToTray": true,
  "DelaySeconds": 540,
  "StartWithSystem": false
}
```

### Configuration Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SoundFilePath` | string | `"sound.wav"` | Relative or absolute path to the `.wav` file to play. The file should be in the same folder as the executable by default. |
| `MinimizeToTray` | boolean | `true` | **Windows only**: When `true`, the application minimizes to the system tray. On Linux/macOS, this setting is ignored and the app runs in console mode. |
| `DelaySeconds` | integer | `540` | Number of seconds between sound playbacks. Default is 540 seconds (9 minutes), which is suitable for most EU soundbars that enter standby after 10 minutes. |
| `StartWithSystem` | boolean | `false` | Start the application automatically when you log in (startup). |


## Running as a Service/Startup Application

### Windows (Task Scheduler)
1. Open Task Scheduler
2. Create Basic Task
3. Set trigger to "When I log on"
4. Set action to start the program: `C:\path\to\SoundbarStandbyHelper.exe`
5. Check "Run whether user is logged on or not" if desired

### Linux (systemd)
Create a service file at `~/.config/systemd/user/soundbar-helper.service`:
```ini
[Unit]
Description=Soundbar Standby Helper

[Service]
ExecStart=/path/to/SoundbarStandbyHelper
WorkingDirectory=/path/to/
Restart=always

[Install]
WantedBy=default.target
```

Enable and start:
```bash
systemctl --user enable soundbar-helper.service
systemctl --user start soundbar-helper.service
```

### macOS (LaunchAgent)
Create `~/Library/LaunchAgents/com.soundbar.helper.plist`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.soundbar.helper</string>
    <key>ProgramArguments</key>
    <array>
        <string>/path/to/SoundbarStandbyHelper</string>
    </array>
    <key>WorkingDirectory</key>
    <string>/path/to/</string>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
</dict>
</plist>
```

Load the agent:
```bash
launchctl load ~/Library/LaunchAgents/com.soundbar.helper.plist
```

## Platform-Specific Notes

- **Windows**: Uses `SoundPlayer` API for audio playback. System tray functionality available.
- **Linux**: Uses `aplay` command-line tool (requires ALSA). No system tray support.
- **macOS**: Uses `afplay` command-line tool (built-in). No system tray support.

## Troubleshooting

### Sound file not found
- Ensure the `.wav` file exists in the specified location
- Use an absolute path in `config.json` if the relative path doesn't work
- Check file permissions

### Linux: "aplay: command not found"
Install ALSA utilities:
```bash
sudo apt-get install alsa-utils
```

### Application doesn't prevent standby
- Reduce `DelaySeconds` value (try 300 seconds / 5 minutes)
- Ensure your audio output device is set correctly
- Make .wav sound a little bit louder
- Verify the sound is actually playing (check logs)

## License

This project is open source. Please check the LICENSE file for details.
