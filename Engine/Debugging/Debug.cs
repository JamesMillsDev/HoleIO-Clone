using HoleIO.Engine.Utility;

namespace HoleIO.Engine.Debugging
{
    /// <summary>
    /// Centralized debug logging system for the engine.
    /// Provides severity-filtered console output with color coding and timestamps.
    /// Similar to Unity's Debug class.
    /// Must be initialized via Initialise() before use.
    /// </summary>
    public static class Debug
    {
       /// <summary>
       /// Maps verbosity levels to console colors for visual distinction.
       /// Info = White, Warning = Yellow, Error = Red, Exception = Dark Magenta.
       /// </summary>
       private static readonly Dictionary<EDebugVerbosity, ConsoleColor> VerbosityColors = new()
       {
          { EDebugVerbosity.Info, ConsoleColor.White },
          { EDebugVerbosity.Warning, ConsoleColor.Yellow },
          { EDebugVerbosity.Error, ConsoleColor.Red },
          { EDebugVerbosity.Exception, ConsoleColor.DarkMagenta }
       };

       /// <summary>
       /// Gets the current debug configuration loaded from Resources/Config/Debug.json.
       /// </summary>
       public static DebuggingConfigData Config => config.Get();
       
       // Configuration instance (set during initialization)
       private static Config<DebuggingConfigData> config = null!;

       /// <summary>
       /// Logs an informational message to the console.
       /// Only displayed if verbosity is set to Info or higher.
       /// Use for general status updates and non-critical information.
       /// </summary>
       /// <param name="msg">The message to log</param>
       /// <example>
       /// Debug.Log("Player spawned at position (10, 0, 5)");
       /// </example>
       public static void Log(string msg) 
       {
          Log(msg, EDebugVerbosity.Info);
       }

       /// <summary>
       /// Logs a warning message to the console in yellow.
       /// Only displayed if verbosity is set to Warning or higher.
       /// Use for potential issues that don't prevent execution.
       /// </summary>
       /// <param name="msg">The warning message to log</param>
       /// <example>
       /// Debug.LogWarning("Texture not found, using default");
       /// </example>
       public static void LogWarning(string msg) 
       {
          Log(msg, EDebugVerbosity.Warning);
       }

       /// <summary>
       /// Logs an error message to the console in red.
       /// Only displayed if verbosity is set to Error or higher.
       /// Use for problems that affect functionality but are handled.
       /// </summary>
       /// <param name="msg">The error message to log</param>
       /// <example>
       /// Debug.LogError("Failed to load required asset: player_model.fbx");
       /// </example>
       public static void LogError(string msg) 
       {
          Log(msg, EDebugVerbosity.Error);
       }

       /// <summary>
       /// Logs an exception to the console in dark magenta.
       /// Only displayed if verbosity is set to Exception.
       /// Includes the full exception message and stack trace.
       /// </summary>
       /// <param name="e">The exception to log</param>
       /// <example>
       /// try { ... }
       /// catch (Exception e) { Debug.LogException(e); }
       /// </example>
       public static void LogException(Exception e) 
       {
          Log(e.ToString(), EDebugVerbosity.Exception);
       }

       /// <summary>
       /// Initializes the debug system by loading the debug configuration.
       /// Must be called during engine startup before any logging occurs.
       /// </summary>
       internal static void Initialise()
       {
          config = new Config<DebuggingConfigData>("Debug");
       }

       /// <summary>
       /// Core logging method that handles filtering, formatting, and colored output.
       /// Messages are filtered based on the configured verbosity level.
       /// Format: [Abbreviated Verbosity] [YY:MM:DD-Time] - Message
       /// </summary>
       /// <param name="msg">The message to log</param>
       /// <param name="verbosity">The severity level of this message</param>
       private static void Log(string msg, EDebugVerbosity verbosity)
       {
          // Filter out messages below the configured verbosity threshold
          // Higher enum values = higher severity, so only log if message severity >= config severity
          if ((int)verbosity > (int)Config.logVerbosity)
          {
             return;
          }

          // Get current timestamp
          DateTime time = DateTime.Now;

          // Format timestamp: [24:12:25-14:30:45]
          string timeString = $"[{time:yy:MM:dd}-{time:HH:mm:ss}]";
          
          // Format abbreviated verbosity tag: [Inf], [Wrn], [Err], [Exc]
          string verbosityString = $"[{VerbosityString(verbosity)}]";

          // Output with appropriate color - verbosity tag first, then timestamp, then message
          Console.ForegroundColor = VerbosityColors[verbosity];
          Console.WriteLine($"{verbosityString} {timeString} - {msg}");
          Console.ResetColor();
       }

       /// <summary>
       /// Converts verbosity enum to a short 3-character string for compact log output.
       /// Info -> "Inf", Warning -> "Wrn", Error -> "Err", Exception -> "Exc"
       /// </summary>
       /// <param name="verbosity">The verbosity level to convert</param>
       /// <returns>3-character abbreviated string</returns>
       /// <exception cref="NotImplementedException">Thrown if Disabled verbosity is passed (should never be logged)</exception>
       /// <exception cref="ArgumentOutOfRangeException">Thrown for unknown verbosity values</exception>
       private static string VerbosityString(EDebugVerbosity verbosity)
       {
          return verbosity switch
          {
             EDebugVerbosity.Info => "Inf",
             EDebugVerbosity.Warning => "Wrn",
             EDebugVerbosity.Error => "Err",
             EDebugVerbosity.Exception => "Exc",
             EDebugVerbosity.Disabled => throw new NotImplementedException(),
             _ => throw new ArgumentOutOfRangeException(nameof(verbosity), verbosity, null)
          };
       }
    }
}