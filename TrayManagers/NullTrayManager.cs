namespace SoundbarStandbyHelper.TrayManagers;

internal class NullTrayManager : ITrayManager
{
#pragma warning disable CS0067 // Event is never used - this is a null implementation
	public event Action? OnPlaySoundRequested;
#pragma warning restore CS0067

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
