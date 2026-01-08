namespace SoundbarStandbyHelper.StartupManagers;

internal interface IStartupManager
{
	bool IsSupported { get; }
	bool IsStartupEnabled();
	void EnableStartup();
	void DisableStartup();
	void ToggleStartup();
}
