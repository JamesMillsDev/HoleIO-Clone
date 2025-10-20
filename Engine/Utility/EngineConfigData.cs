using System.Drawing;
using Newtonsoft.Json;

namespace HoleIO.Engine.Core
{
	[JsonObject]
	public struct EngineConfigData
	{
		[JsonObject]
		public struct WindowConfigData
		{
			public int width;
			public int height;
			public string title;
			public bool fullscreen;
			public bool maximised;
		}
		
		[JsonProperty] public WindowConfigData window;
		[JsonProperty] public Color clearColor;
	}
}