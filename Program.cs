using System.Text.Json;
using SoundbarStandbyHelper;
using SoundbarStandbyHelper.AudioPlayers;
using SoundbarStandbyHelper.TrayManagers;

JsonSerializerOptions jsonOptions = new()
{
	WriteIndented = true
};

var configPath = "config.json";

var config = ConfigManager.Load(configPath);

IAudioPlayer audioPlayer;
try
{
	audioPlayer = AudioPlayerFactory.Create();
	Program.LogMessage($"Audio player initialized: {audioPlayer.GetType().Name}");
}
catch (PlatformNotSupportedException ex)
{
	Program.LogMessage($"Error: {ex.Message}");
	return;
}

ITrayManager trayManager = config.MinimizeToTray && OperatingSystem.IsWindows()
	? new WindowsTrayManager()
	: new NullTrayManager();

if (config.MinimizeToTray)
{
	if (OperatingSystem.IsWindows())
	{
		trayManager.Initialize("Sound Timer", "Sound Timer - Running");
		Program.LogMessage("Application minimized to system tray");
		trayManager.ShowNotification("Sound Timer", "Application is running in system tray. Right-click tray icon to exit.");
	}
	else
	{
		Program.LogMessage("Warning: MinimizeToTray is only supported on Windows. Running in console mode.");
	}
}

Program.LogMessage($"Sound Timer Started");
Program.LogMessage($"Sound file: {config.SoundFilePath}");
Program.LogMessage($"Sound will play every {config.DelaySeconds} seconds...");
Program.LogMessage("Press Ctrl+C to exit.");
Console.WriteLine();

PlaySound(config.SoundFilePath);

var timer = new System.Threading.Timer(
	callback: _ => PlaySound(config.SoundFilePath),
	state: null,
	dueTime: TimeSpan.FromSeconds(config.DelaySeconds),
	period: TimeSpan.FromSeconds(config.DelaySeconds));

Program.ExitEvent.WaitOne();
timer.Dispose();
trayManager.Dispose();

Program.LogMessage("Exiting...");

void PlaySound(string soundFilePath)
{
	try
	{
		if (!File.Exists(soundFilePath))
		{
			Program.LogMessage($"Error: Sound file '{soundFilePath}' not found.");
			return;
		}

		Program.LogMessage($"Playing sound: {soundFilePath}");

		audioPlayer.PlaySound(soundFilePath);

		Program.LogMessage($"Sound playback completed.");
	}
	catch (Exception ex)
	{
		Program.LogMessage($"Error playing sound: {ex.Message}");
	}
}

internal partial class Program
{
	private static readonly ManualResetEvent _exitEvent = new(false);

	public static ManualResetEvent ExitEvent => _exitEvent;

	static Program()
	{
		Console.CancelKeyPress += (sender, e) =>
		{
			e.Cancel = true;
			RequestExit();
		};
	}

	public static void RequestExit()
	{
		_exitEvent.Set();
	}

	public static void LogMessage(string message)
	{
		Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
	}
}
