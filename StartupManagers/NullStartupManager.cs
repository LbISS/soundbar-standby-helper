namespace SoundbarStandbyHelper.StartupManagers;

internal class NullStartupManager : IStartupManager
{
	public bool IsSupported => false;

	public bool IsStartupEnabled()
	{
		return false;
	}

	public void EnableStartup()
	{
		// No-op
	}

	public void DisableStartup()
	{
		// No-op
	}

	public void ToggleStartup()
	{
		// No-op
	}
}
