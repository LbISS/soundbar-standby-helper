using System.Diagnostics;

namespace SoundbarStandbyHelper.AudioPlayers;

internal class MacOSAudioPlayer : IAudioPlayer
{
	public void PlaySound(string soundFilePath)
	{
		var absolutePath = Path.GetFullPath(soundFilePath);

		var startInfo = new ProcessStartInfo
		{
			FileName = "afplay",
			Arguments = $"\"{absolutePath}\"",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		using var process = Process.Start(startInfo);
		if (process != null)
		{
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				var error = process.StandardError.ReadToEnd();
				throw new Exception($"afplay error: {error}");
			}
		}
		else
		{
			throw new Exception("Failed to start afplay process.");
		}
	}
}
