namespace SoundbarStandbyHelper.TrayManagers;

internal class NullTrayManager : ITrayManager
{
	public void Initialize(string title, string tooltip)
	{
	}

	public void ShowNotification(string title, string message)
	{
	}

	public void Dispose()
	{
	}
}
