namespace SoundbarStandbyHelper.AudioPlayers;

internal static class AudioPlayerFactory
{
	public static IAudioPlayer Create()
	{
		if (OperatingSystem.IsWindows())
		{
			return new WindowsAudioPlayer();
		}
		else if (OperatingSystem.IsLinux())
		{
			return new LinuxAudioPlayer();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return new MacOSAudioPlayer();
		}
		else
		{
			throw new PlatformNotSupportedException("Audio playback is not supported on this operating system.");
		}
	}
}
