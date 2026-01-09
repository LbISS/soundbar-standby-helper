using System.Text.Json;
using SoundbarStandbyHelper;
using SoundbarStandbyHelper.AudioPlayers;
using SoundbarStandbyHelper.TrayManagers;
using SoundbarStandbyHelper.StartupManagers;
using System.Runtime.InteropServices;

JsonSerializerOptions jsonOptions = new()
{
	WriteIndented = true
};

var configPath = "config.json";

var config = ConfigManager.Load(configPath);
Program.LogMessage($"Config has been loaded:" + JsonSerializer.Serialize(config, jsonOptions));

IAudioPlayer audioPlayer;
try
{
	audioPlayer = AudioPlayerFactory.Create();
}
catch (PlatformNotSupportedException ex)
{
	Program.LogMessage($"Error: {ex.Message}");
	return;
}

// Create startup manager
IStartupManager startupManager = StartupManagerFactory.Create();

// Apply startup setting from config
if (config.StartWithSystem && !startupManager.IsStartupEnabled())
{
	startupManager.EnableStartup();
}
else if (!config.StartWithSystem && startupManager.IsStartupEnabled())
{
	startupManager.DisableStartup();
}

ITrayManager trayManager = config.MinimizeToTray && OperatingSystem.IsWindows()
	? new WindowsTrayManager()
	: new NullTrayManager();

// Subscribe to play sound event from tray
trayManager.OnPlaySoundRequested += () => PlaySound(config.SoundFilePath);

if (config.MinimizeToTray)
{
	if (OperatingSystem.IsWindows())
	{
		trayManager.Initialize("Soundbar Standby Helper", "Soundbar Standby Helper - Running", startupManager);
		trayManager.HideConsole();
	}
	else
	{
		Program.LogMessage("Warning: MinimizeToTray is only supported on Windows. Running in console mode.");
	}
}

Program.LogMessage($"Soundbar Standby Helper Started");
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
	if (Program.IsExiting)
		return;

	try
	{
		if (!File.Exists(soundFilePath))
		{
			Program.LogMessage($"Error: Sound file '{soundFilePath}' not found.");
			return;
		}

		Program.LogMessage($"Playing sound: {soundFilePath}");

		audioPlayer.PlaySound(soundFilePath);
	}
	catch (Exception ex)
	{
		Program.LogMessage($"Error playing sound: {ex.Message}");
	}
}

internal partial class Program
{
	private static readonly ManualResetEvent _exitEvent = new(false);
	private static volatile bool _isExiting = false;
	private static EventHandler? _handler;

	public static ManualResetEvent ExitEvent => _exitEvent;
	public static bool IsExiting => _isExiting;

	[DllImport("Kernel32")]
	private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

	private delegate bool EventHandler(CtrlType sig);

	private enum CtrlType
	{
		CTRL_C_EVENT = 0,
		CTRL_BREAK_EVENT = 1,
		CTRL_CLOSE_EVENT = 2,
		CTRL_LOGOFF_EVENT = 5,
		CTRL_SHUTDOWN_EVENT = 6
	}

	static Program()
	{
		// Handle Ctrl+C
		Console.CancelKeyPress += (sender, e) =>
		{
			e.Cancel = true;
			RequestExit();
		};

		// Handle window close button (X)
		if (OperatingSystem.IsWindows())
		{
			_handler = new EventHandler(sig =>
			{
				if (sig == CtrlType.CTRL_CLOSE_EVENT)
				{
					// Close button was clicked - do immediate cleanup on background thread
					Task.Run(() =>
					{
						RequestExit();
						// Give cleanup 500ms max
						Thread.Sleep(500);
						Environment.Exit(0);
					});
					return true;
				}

				RequestExit();
				return false;
			});

			SetConsoleCtrlHandler(_handler, true);
		}
	}

	public static void RequestExit()
	{
		if (_isExiting)
			return;

		_isExiting = true;
		_exitEvent.Set();
	}

	public static void LogMessage(string message)
	{
		Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
	}
}
