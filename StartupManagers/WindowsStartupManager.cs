using Microsoft.Win32;

namespace SoundbarStandbyHelper.StartupManagers;

internal class WindowsStartupManager : IStartupManager
{
	private const string AppName = "SoundbarStandbyHelper";
	private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

	public bool IsSupported => true;

	public bool IsStartupEnabled()
	{
		if (!OperatingSystem.IsWindows())
			return false;

		try
		{
			using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
			var value = key?.GetValue(AppName) as string;
			return !string.IsNullOrEmpty(value);
		}
		catch
		{
			return false;
		}
	}

	public void EnableStartup()
	{
		if (!OperatingSystem.IsWindows())
			return;

		try
		{
			var exePath = Environment.ProcessPath;
			if (string.IsNullOrEmpty(exePath))
				return;

			var exeDir = Path.GetDirectoryName(exePath);
			if (string.IsNullOrEmpty(exeDir))
				return;

			// Use cmd /c to set working directory before starting the app
			// This ensures config.json and other files are found in the app directory
			var command = $"cmd /c cd /d \"{exeDir}\" && \"{exePath}\"";

			using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
			key?.SetValue(AppName, command);

			// Save to config
			SaveStartupStateToConfig(true);

			Program.LogMessage("Startup enabled: Application will start with Windows");
		}
		catch (Exception ex)
		{
			Program.LogMessage($"Error enabling startup: {ex.Message}");
		}
	}

	public void DisableStartup()
	{
		if (!OperatingSystem.IsWindows())
			return;

		try
		{
			using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
			key?.DeleteValue(AppName, false);

			// Save to config
			SaveStartupStateToConfig(false);

			Program.LogMessage("Startup disabled: Application will not start with Windows");
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

	private void SaveStartupStateToConfig(bool enabled)
	{
		try
		{
			var config = ConfigManager.Load("config.json");
			config.StartWithSystem = enabled;
			ConfigManager.Save("config.json", config);
		}
		catch (Exception ex)
		{
			Program.LogMessage($"Warning: Could not save startup state to config: {ex.Message}");
		}
	}
}
