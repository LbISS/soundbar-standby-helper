using System.Runtime.InteropServices;

namespace SoundbarStandbyHelper.TrayManagers;

internal class WindowsTrayManager : ITrayManager
{
	private object? _notifyIcon;
	private IntPtr _consoleWindow;
	private bool _disposed;
	private Thread? _messageLoopThread;
	private object? _applicationContext;
	private Thread? _windowWatchThread;
	private volatile bool _shouldMonitorWindow;

	public void Initialize(string title, string tooltip)
	{
		if (!OperatingSystem.IsWindows())
		{
			throw new PlatformNotSupportedException("System tray is only supported on Windows.");
		}

		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return;
		}

		_consoleWindow = GetConsoleWindow();

		// Start a message loop thread for Windows Forms
		_messageLoopThread = new Thread(() =>
		{
			var applicationType = Type.GetType("System.Windows.Forms.Application, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			var applicationContextType = Type.GetType("System.Windows.Forms.ApplicationContext, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

			if (applicationType == null || applicationContextType == null)
			{
				Program.LogMessage("Warning: Windows Forms types not available.");
				return;
			}

			// Create NotifyIcon on the UI thread
			var notifyIconType = Type.GetType("System.Windows.Forms.NotifyIcon, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			if (notifyIconType == null)
			{
				Program.LogMessage("Warning: System.Windows.Forms.NotifyIcon not available.");
				return;
			}

			_notifyIcon = Activator.CreateInstance(notifyIconType);

			// Try to load custom icon, fall back to system icon if not found
			object? icon = null;
			var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "standby-helper.ico");

			if (File.Exists(iconPath))
			{
				try
				{
					var iconType = Type.GetType("System.Drawing.Icon, System.Drawing.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");
					if (iconType != null)
					{
						var iconConstructor = iconType.GetConstructor(new[] { typeof(string) });
						icon = iconConstructor?.Invoke(new object[] { iconPath });
					}
				}
				catch (Exception ex)
				{
					Program.LogMessage($"Warning: Could not load custom icon: {ex.Message}");
				}
			}
			else
			{
				Program.LogMessage($"Custom icon not found at: {iconPath}");
			}

			// Fall back to system icon if custom icon failed to load
			if (icon == null)
			{
				var systemIconsType = Type.GetType("System.Drawing.SystemIcons, System.Drawing.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");
				icon = systemIconsType?.GetProperty("Application", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
			}

			notifyIconType.GetProperty("Icon")?.SetValue(_notifyIcon, icon);
			notifyIconType.GetProperty("Text")?.SetValue(_notifyIcon, tooltip);
			notifyIconType.GetProperty("Visible")?.SetValue(_notifyIcon, true);

			var contextMenuStripType = Type.GetType("System.Windows.Forms.ContextMenuStrip, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			var contextMenu = Activator.CreateInstance(contextMenuStripType!);

			var itemsProperty = contextMenuStripType!.GetProperty("Items");
			var items = itemsProperty?.GetValue(contextMenu);

			if (items != null)
			{
				// Get the ToolStripMenuItem type
				var toolStripMenuItemType = Type.GetType("System.Windows.Forms.ToolStripMenuItem, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

				if (toolStripMenuItemType != null)
				{
					// Create "Restore" menu item
					var restoreMenuItem = Activator.CreateInstance(toolStripMenuItemType, new object[] { "Restore" });
					var restoreClickEvent = toolStripMenuItemType.GetEvent("Click");
					restoreClickEvent?.AddEventHandler(restoreMenuItem, new EventHandler((s, e) => ShowConsoleWindow()));

					var addMethod = items.GetType().GetMethod("Add", new[] { toolStripMenuItemType });
					addMethod?.Invoke(items, new[] { restoreMenuItem });

					// Create "Exit" menu item
					var exitMenuItem = Activator.CreateInstance(toolStripMenuItemType, new object[] { "Exit" });
					var exitClickEvent = toolStripMenuItemType.GetEvent("Click");
					exitClickEvent?.AddEventHandler(exitMenuItem, new EventHandler((s, e) => Program.RequestExit()));

					addMethod?.Invoke(items, new[] { exitMenuItem });

					var itemCount = items.GetType().GetProperty("Count")?.GetValue(items);
				}
				else
				{
					Program.LogMessage("Warning: Could not load ToolStripMenuItem type");
				}
			}
			else
			{
				Program.LogMessage("Warning: Could not get context menu items collection");
			}

			notifyIconType.GetProperty("ContextMenuStrip")?.SetValue(_notifyIcon, contextMenu);

			// Use MouseDoubleClick event with proper handler
			var mouseDoubleClickEvent = notifyIconType.GetEvent("MouseDoubleClick");
			if (mouseDoubleClickEvent != null)
			{
				var mouseEventHandlerType = Type.GetType("System.Windows.Forms.MouseEventHandler, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
				if (mouseEventHandlerType != null)
				{
					// Create a delegate that matches MouseEventHandler signature: void (object sender, MouseEventArgs e)
					var handler = new Action<object?, object?>((sender, e) => ShowConsoleWindow());
					var delegateHandler = Delegate.CreateDelegate(mouseEventHandlerType, handler.Target, handler.Method);
					mouseDoubleClickEvent.AddEventHandler(_notifyIcon, delegateHandler);
				}
			}

			// Create ApplicationContext and run message loop
			_applicationContext = Activator.CreateInstance(applicationContextType!);
			var runMethod = applicationType.GetMethod("Run", new[] { applicationContextType });
			runMethod?.Invoke(null, new[] { _applicationContext });
		});

		_messageLoopThread.SetApartmentState(ApartmentState.STA);
		_messageLoopThread.IsBackground = true;
		_messageLoopThread.Start();

		// Give the thread time to initialize
		Thread.Sleep(100);

		// Start window monitoring thread
		StartWindowMonitoring();
	}

	private void StartWindowMonitoring()
	{
		_shouldMonitorWindow = true;
		_windowWatchThread = new Thread(() =>
		{
			while (_shouldMonitorWindow)
			{
				if (_consoleWindow != IntPtr.Zero)
				{
					if (IsIconic(_consoleWindow))
					{
						// Window is minimized, hide it
						HideConsoleWindowInternal();
					}
				}
				Thread.Sleep(250); // Check every 250ms
			}
		});
		_windowWatchThread.IsBackground = true;
		_windowWatchThread.Start();
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool BringWindowToTop(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr hWnd);

	private const int SW_HIDE = 0;
	private const int SW_RESTORE = 9;

	public void HideConsole()
	{
		if (OperatingSystem.IsWindows())
		{
			HideConsoleWindowInternal();
		}
	}

	private void HideConsoleWindowInternal()
	{
		if (_consoleWindow != IntPtr.Zero)
		{
			ShowWindow(_consoleWindow, SW_HIDE);
		}
	}

	private void ShowConsoleWindow()
	{
		if (_consoleWindow != IntPtr.Zero)
		{
			ShowWindow(_consoleWindow, SW_RESTORE);
			BringWindowToTop(_consoleWindow);
			SetForegroundWindow(_consoleWindow);
		}
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_shouldMonitorWindow = false;

		if (_notifyIcon != null && OperatingSystem.IsWindows())
		{
			ShowConsoleWindow();

			var notifyIconType = _notifyIcon.GetType();
			notifyIconType.GetProperty("Visible")?.SetValue(_notifyIcon, false);

			var disposeMethod = notifyIconType.GetMethod("Dispose");
			disposeMethod?.Invoke(_notifyIcon, null);
		}

		if (_applicationContext != null)
		{
			var applicationType = Type.GetType("System.Windows.Forms.Application, System.Windows.Forms, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			var exitThreadMethod = applicationType?.GetMethod("ExitThread");
			exitThreadMethod?.Invoke(null, null);
		}

		_disposed = true;
	}
}
