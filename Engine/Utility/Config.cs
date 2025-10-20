using Newtonsoft.Json;

namespace HoleIO.Engine.Utility
{
	public class Config<TConfigData>
	{
		private readonly string name;
		private TConfigData? configData;

		public Config(string name)
		{
			this.name = name;
			this.configData = default;
			Load();
		}

		public TConfigData Get() => this.configData ?? throw new NullReferenceException("Config has not loaded!");

		private void Load()
		{
			string filePath = Path.Combine("Resources", "Config", $"{this.name}.json");
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Config file not found", filePath);
			}

			string contents = File.ReadAllText(filePath);
			
			JsonReader reader = new JsonTextReader(new StringReader(contents));
			JsonSerializer serializer = JsonSerializer.Create();
			this.configData = serializer.Deserialize<TConfigData>(reader);
		}
	}
}