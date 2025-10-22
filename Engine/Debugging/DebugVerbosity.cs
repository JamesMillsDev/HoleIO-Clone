namespace HoleIO.Engine.Debugging
{
	/// <summary>
	/// Defines severity levels for debug logging output.
	/// Used to filter which messages are displayed based on importance.
	/// </summary>
	public enum EDebugVerbosity
	{
		/// <summary>
		/// All debug logging is disabled. Use for production builds.
		/// </summary>
		Disabled = -1,
       
		/// <summary>
		/// Informational messages (lowest severity).
		/// General status updates and non-critical information.
		/// </summary>
		Info,
       
		/// <summary>
		/// Warning messages for potential issues that don't stop execution.
		/// Examples: missing optional resources, deprecated API usage.
		/// </summary>
		Warning,
       
		/// <summary>
		/// Error messages for problems that affect functionality.
		/// Examples: failed to load required asset, invalid configuration.
		/// </summary>
		Error,
       
		/// <summary>
		/// Exception messages for caught exceptions (highest severity).
		/// Critical issues that were handled but indicate serious problems.
		/// </summary>
		Exception
	}
}