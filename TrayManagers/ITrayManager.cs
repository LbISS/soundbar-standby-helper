namespace SoundbarStandbyHelper.TrayManagers;

internal interface ITrayManager : IDisposable
{
	void Initialize(string title, string tooltip);
	void ShowNotification(string title, string message);
}
