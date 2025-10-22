using Newtonsoft.Json;

namespace HoleIO.Engine.Utility
{
	/// <summary>
	/// Generic configuration loader that deserializes JSON config files into strongly-typed data objects.
	/// Automatically loads the config file on construction from Resources/Config/{name}.json.
	/// </summary>
	/// <typeparam name="TConfigData">The type to deserialize the JSON config into</typeparam>
	public class Config<TConfigData>
	{
		// Name of the config file (without path or extension)
		private readonly string name;

		// Cached deserialized config data
		private TConfigData? configData;

		/// <summary>
		/// Creates a new config loader and immediately loads the specified config file.
		/// </summary>
		/// <param name="name">Name of the config file (without path or .json extension)</param>
		/// <exception cref="FileNotFoundException">Thrown if the config file doesn't exist</exception>
		/// <exception cref="JsonException">Thrown if the JSON is malformed or doesn't match TConfigData</exception>
		public Config(string name)
		{
			this.name = name;
			this.configData = default;
			Load();
		}

		/// <summary>
		/// Gets the loaded configuration data.
		/// </summary>
		/// <returns>The deserialized config object</returns>
		/// <exception cref="NullReferenceException">Thrown if config failed to load (shouldn't happen if Load() succeeded)</exception>
		public TConfigData Get() => this.configData ?? throw new NullReferenceException("Config has not loaded!");

		/// <summary>
		/// Loads and deserializes the JSON config file from Resources/Config/{name}.json.
		/// Called automatically during construction.
		/// </summary>
		/// <exception cref="FileNotFoundException">Thrown if the config file doesn't exist</exception>
		/// <exception cref="JsonException">Thrown if JSON deserialization fails</exception>
		private void Load()
		{
			// Construct full path to config file
			string filePath = Path.Combine("Resources", "Config", $"{this.name}.json");
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Config file not found", filePath);
			}

			// Read entire file contents
			string contents = File.ReadAllText(filePath);

			// Deserialize JSON into TConfigData using Newtonsoft.Json
			JsonReader reader = new JsonTextReader(new StringReader(contents));
			JsonSerializer serializer = JsonSerializer.Create();
			this.configData = serializer.Deserialize<TConfigData>(reader);
		}
	}
}