namespace SoundbarStandbyHelper.TrayManagers;

internal class NullTrayManager : ITrayManager
{
	public event Action? OnPlaySoundRequested;

	public void Initialize(string title, string tooltip)
	{
	}

	public void ShowNotification(string title, string message)
	{
	}

	public void HideConsole()
	{
	}

	public void Dispose()
	{
	}
}
