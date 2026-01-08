using System.Media;

namespace SoundbarStandbyHelper.AudioPlayers;

internal class WindowsAudioPlayer : IAudioPlayer
{
	public void PlaySound(string soundFilePath)
	{
#pragma warning disable CA1416 // Validate platform compatibility
		using var player = new SoundPlayer(soundFilePath);
		player.PlaySync();
#pragma warning restore CA1416 // Validate platform compatibility
	}
}
