using System.Drawing;
using Newtonsoft.Json;

namespace HoleIO.Engine.Utility
{
	public class ColorJsonConverter : JsonConverter<Color>
	{
		public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
		{
			// Serialize Color to its ARGB integer value
			writer.WriteValue(value.ToArgb());
		}

		public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Integer)
			{
				// Deserialize from ARGB integer value
				int argb = (int)(long)(reader.Value ?? 0xffffffff);
				return Color.FromArgb(argb);
			}

			if (reader.TokenType != JsonToken.String)
			{
				return Color.Empty; // Default if unable to deserialize
			}

			// Handle potential string representation like "#RRGGBB" or color names
			string? colorString = reader.Value?.ToString();
			if (string.IsNullOrEmpty(colorString))
			{
				return Color.Empty; // Default if unable to deserialize
			}

			try
			{
				// Attempt to parse from hex string (e.g., "#AARRGGBB" or "#RRGGBB")
				if (colorString.StartsWith('#'))
				{
					return ColorTranslator.FromHtml(colorString);
				}

				// Attempt to parse from known color names
				return Color.FromName(colorString);
			}
			catch (Exception)
			{
				// Handle parsing errors, e.g., log or return a default color
				return Color.Empty; // Or another default color
			}
		}
	}
}