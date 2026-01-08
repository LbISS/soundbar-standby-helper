namespace SoundbarStandbyHelper
{
	internal class AppConfig
	{
		public string SoundFilePath { get; set; } = "sound.wav";
		public bool MinimizeToTray { get; set; } = true;
		public int DelaySeconds { get; set; } = 540;
	}
}
