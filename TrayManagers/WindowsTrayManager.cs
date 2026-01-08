#if WINDOWS
using System.Drawing;
using System.Windows.Forms;
#endif

namespace SoundbarStandbyHelper.TrayManagers;

internal class WindowsTrayManager : ITrayManager
{
#if WINDOWS
	private NotifyIcon? _notifyIcon;
#endif
	private bool _disposed;

	public void Initialize(string title, string tooltip)
	{
		if (!OperatingSystem.IsWindows())
		{
			throw new PlatformNotSupportedException("System tray is only supported on Windows.");
		}

#if WINDOWS
		_notifyIcon = new NotifyIcon
		{
			Icon = SystemIcons.Application,
			Text = tooltip,
			Visible = true
		};

		var contextMenu = new ContextMenuStrip();
		contextMenu.Items.Add("Exit", null, (s, e) =>
		{
			Program.RequestExit();
		});

		_notifyIcon.ContextMenuStrip = contextMenu;
		_notifyIcon.DoubleClick += (s, e) =>
		{
			ShowNotification(title, "Application is running in system tray");
		};
#endif
	}

	public void ShowNotification(string title, string message)
	{
#if WINDOWS
		if (_notifyIcon == null)
			return;

		_notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
#endif
	}

	public void Dispose()
	{
		if (_disposed)
			return;

#if WINDOWS
		if (_notifyIcon != null)
		{
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();
		}
#endif

		_disposed = true;
	}
}
