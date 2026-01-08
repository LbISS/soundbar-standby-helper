namespace SoundbarStandbyHelper.StartupManagers;

internal static class StartupManagerFactory
{
	public static IStartupManager Create()
	{
		if (OperatingSystem.IsWindows())
		{
			return new WindowsStartupManager();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return new MacOSStartupManager();
		}
		else if (OperatingSystem.IsLinux())
		{
			return new LinuxStartupManager();
		}

		return new NullStartupManager();
	}
}
