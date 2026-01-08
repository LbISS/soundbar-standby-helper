using System.Text.Json;

namespace SoundbarStandbyHelper;

internal static class ConfigManager
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true
	};

	public static AppConfig Load(string configPath)
	{
		if (!File.Exists(configPath))
		{
			Program.LogMessage($"Configuration file '{configPath}' not found. Creating default config...");
			CreateDefault(configPath);
			Program.LogMessage("Configuration saved successfully");
		}

		return ReadConfig(configPath);
	}

	public static void Save(string configPath, AppConfig config)
	{
		try
		{
			var json = JsonSerializer.Serialize(config, JsonOptions);
			File.WriteAllText(configPath, json);
			Program.LogMessage("Configuration saved successfully");
		}
		catch (Exception ex)
		{
			Program.LogMessage($"Error saving configuration: {ex.Message}");
		}
	}

	private static AppConfig ReadConfig(string path)
	{
		try
		{
			var json = File.ReadAllText(path);
			var config = JsonSerializer.Deserialize<AppConfig>(json);

			if (config == null)
			{
				throw new Exception("Failed to deserialize configuration.");
			}

			return config;
		}
		catch (Exception ex)
		{
			Program.LogMessage($"Error loading configuration: {ex.Message}");
			Program.LogMessage("Using default configuration...");
			return new AppConfig();
		}
	}

	private static void CreateDefault(string path)
	{
		var defaultConfig = new AppConfig();
		var json = JsonSerializer.Serialize(defaultConfig, JsonOptions);

		File.WriteAllText(path, json);
		Program.LogMessage($"Default configuration created at '{path}'");
		Program.LogMessage("Please update the configuration file with your desired settings.");
		Console.WriteLine();
	}
}
