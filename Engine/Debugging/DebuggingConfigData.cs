using Newtonsoft.Json;

namespace HoleIO.Engine.Debugging
{
	/// <summary>
	/// Configuration structure for engine debugging settings.
	/// Deserialized from JSON config files using Newtonsoft.Json.
	/// Controls debug logging, error reporting, and development-time features.
	/// </summary>
	[JsonObject]
	public struct DebuggingConfigData
	{
		/// <summary>
		/// Whether to log shader compilation and linking exceptions to the console.
		/// When true, shader errors are written to Console.WriteLine for debugging.
		/// When false, shader errors are silently ignored (useful in production builds).
		/// Default: typically false for release builds, true for development.
		/// </summary>
		[JsonProperty] public bool logShaderExceptions;
		
		/// <summary>
		/// The level of detail for debug logging output.
		/// Controls how much information is written to the console during execution.
		/// Higher verbosity levels include all messages from lower levels.
		/// Use lower verbosity in production to reduce performance overhead and console clutter.
		/// </summary>
		[JsonProperty] 
		public EDebugVerbosity logVerbosity;
	}
}