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

			using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
			key?.SetValue(AppName, $"\"{exePath}\"");
			
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
}
