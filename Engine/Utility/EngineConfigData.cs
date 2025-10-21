using System.Drawing;
using Newtonsoft.Json;

namespace HoleIO.Engine.Utility
{
    /// <summary>
    /// Root configuration structure for engine settings.
    /// Deserialized from JSON config files using Newtonsoft.Json.
    /// Contains window configuration and rendering settings.
    /// </summary>
    /// <example>
    /// Example JSON structure:
    /// {
    ///   "window": {
    ///     "width": 1920,
    ///     "height": 1080,
    ///     "title": "My Game",
    ///     "fullscreen": false,
    ///     "maximised": true
    ///   },
    ///   "clearColor": "#87CEEB"  // Or "A,R,G,B" format
    /// }
    /// </example>
    [JsonObject]
    public struct EngineConfigData
    {
       /// <summary>
       /// Configuration structure for window properties.
       /// Defines initial window size, title, and display mode.
       /// </summary>
       [JsonObject]
       public struct WindowConfigData
       {
          /// <summary>
          /// Initial window width in pixels.
          /// </summary>
          public int width;
          
          /// <summary>
          /// Initial window height in pixels.
          /// </summary>
          public int height;
          
          /// <summary>
          /// Window title displayed in the title bar.
          /// </summary>
          public string title;
          
          /// <summary>
          /// Whether the window should start in fullscreen mode.
          /// If true, takes priority over maximised.
          /// </summary>
          public bool fullscreen;
          
          /// <summary>
          /// Whether the window should start maximized.
          /// Ignored if fullscreen is true.
          /// </summary>
          public bool maximised;
       }
       
       /// <summary>
       /// Window configuration settings.
       /// Controls initial window size, title, and display mode.
       /// </summary>
       [JsonProperty] 
       public WindowConfigData window;
       
       /// <summary>
       /// The background color used to clear the screen each frame.
       /// Supports standard color names (e.g., "Blue") or hex format (e.g., "#87CEEB").
       /// Can also be specified as "R,G,B" or "R,G,B,A" values (0-255).
       /// </summary>
       [JsonProperty] 
       public Color clearColor;
    }
}