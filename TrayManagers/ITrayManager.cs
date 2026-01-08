using SoundbarStandbyHelper.StartupManagers;

namespace SoundbarStandbyHelper.TrayManagers;

internal interface ITrayManager : IDisposable
{
	event Action? OnPlaySoundRequested;
	
	void Initialize(string title, string tooltip, IStartupManager startupManager);
	void ShowNotification(string title, string message);
	void HideConsole();
}
