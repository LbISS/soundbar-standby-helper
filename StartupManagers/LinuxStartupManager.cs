namespace SoundbarStandbyHelper.StartupManagers;

internal class LinuxStartupManager : IStartupManager
{
	private const string AppName = "soundbar-standby-helper";
	private string AutostartPath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		".config", "autostart", $"{AppName}.desktop"
	);

	public bool IsSupported => true;

	public bool IsStartupEnabled()
	{
		return File.Exists(AutostartPath);
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

			var directory = Path.GetDirectoryName(AutostartPath);
			if (directory != null && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Create .desktop file for XDG autostart
			var desktopContent = $@"[Desktop Entry]
Type=Application
Name=Soundbar Standby Helper
Comment=Prevents soundbars from entering standby mode
Exec={exePath}
Icon=audio-headphones
Terminal=false
Categories=Utility;
X-GNOME-Autostart-enabled=true";

			File.WriteAllText(AutostartPath, desktopContent);
			
			// Make it executable
			if (OperatingSystem.IsLinux())
			{
				System.Diagnostics.Process.Start("chmod", $"+x \"{AutostartPath}\"");
			}
			
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
			if (File.Exists(AutostartPath))
			{
				File.Delete(AutostartPath);
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
