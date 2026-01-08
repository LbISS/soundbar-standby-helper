namespace SoundbarStandbyHelper.StartupManagers;

internal class MacOSStartupManager : IStartupManager
{
	private const string AppName = "SoundbarStandbyHelper";
	private string LaunchAgentPath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		"Library", "LaunchAgents", $"com.{AppName}.plist"
	);

	public bool IsSupported => true;

	public bool IsStartupEnabled()
	{
		return File.Exists(LaunchAgentPath);
	}

	public void EnableStartup()
	{
		try
		{
			var exePath = Environment.ProcessPath;
			if (string.IsNullOrEmpty(exePath))
			{
				Program.LogMessage("Error: Could not determine application path");
				return;
			}

			var directory = Path.GetDirectoryName(LaunchAgentPath);
			if (directory != null && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Create Launch Agent plist file
			var plistContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.{AppName}</string>
    <key>ProgramArguments</key>
    <array>
        <string>{exePath}</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <false/>
</dict>
</plist>";

			File.WriteAllText(LaunchAgentPath, plistContent);
			Program.LogMessage("Startup enabled: Application will start on login");
		}
		catch (Exception ex)
		{
			Program.LogMessage($"Error enabling startup: {ex.Message}");
		}
	}

	public void DisableStartup()
	{
		try
		{
			if (File.Exists(LaunchAgentPath))
			{
				File.Delete(LaunchAgentPath);
				Program.LogMessage("Startup disabled: Application will not start on login");
			}
		}
		catch (Exception ex)
		{
			Program.LogMessage($"Error disabling startup: {ex.Message}");
		}
	}

	public void ToggleStartup()
	{
		if (IsStartupEnabled())
		{
			DisableStartup();
		}
		else
		{
			EnableStartup();
		}
	}
}
