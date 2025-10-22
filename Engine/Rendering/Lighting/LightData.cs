using System.Drawing;
using HoleIO.Engine.Rendering.Components;

namespace HoleIO.Engine.Rendering.Lighting
{
    /// <summary>
    /// Manages all lighting information for a scene, including ambient light and dynamic light sources.
    /// Provides centralized storage and access to lights for the rendering system.
    /// </summary>
    public class LightData
    {
       /// <summary>
       /// The base ambient light color applied to all objects in the scene.
       /// Provides minimum illumination even when no direct lights are present.
       /// Default is medium gray (127, 127, 127).
       /// </summary>
       public Color ambientLightColor = Color.FromArgb(255, 127, 127, 127);
       
       /// <summary>
       /// Gets all lights in the scene, sorted by light type.
       /// Lights are ordered as: Directional, then Point, then Spot.
       /// This ordering ensures consistent shader array indexing and optimal rendering.
       /// </summary>
       public IEnumerable<LightComponent> Lights => this.lights.OrderBy(light => light.lightType);
       
       /// <summary>
       /// Internal collection of all active light components in the scene.
       /// </summary>
       private readonly List<LightComponent> lights = [];

       /// <summary>
       /// Registers a light component with the scene's lighting system.
       /// Prevents duplicate registrations of the same light.
       /// </summary>
       /// <param name="light">The light component to add</param>
       internal void AddLight(LightComponent light)
       {
          // Skip if light is already registered
          if (this.lights.Contains(light))
          {
             return;
          }
          
          this.lights.Add(light);
       }

       /// <summary>
       /// Unregisters a light component from the scene's lighting system.
       /// Safely handles removal of lights that aren't registered.
       /// </summary>
       /// <param name="light">The light component to remove</param>
       internal void RemoveLight(LightComponent light)
       {
          // Skip if light isn't registered
          if (!this.lights.Contains(light))
          {
             return;
          }
          
          this.lights.Remove(light);
       }
    }
}