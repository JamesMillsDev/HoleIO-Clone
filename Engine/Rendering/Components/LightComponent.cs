using System.Drawing;
using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Gameplay.Scenes;

namespace HoleIO.Engine.Rendering.Components
{
    /// <summary>
    /// Defines the types of lights available in the rendering system.
    /// </summary>
    public enum ELightType
    {
       /// <summary>
       /// Infinite distance light with parallel rays (e.g., sunlight).
       /// Has direction but no position, affects all objects equally.
       /// </summary>
       Directional,
       
       /// <summary>
       /// Omnidirectional light emanating from a point in space (e.g., light bulb).
       /// Has position and distance-based attenuation falloff.
       /// </summary>
       Point,
       
       /// <summary>
       /// Cone-shaped directional light with limited spread (e.g., flashlight).
       /// Has position, direction, attenuation, and inner/outer cone angles.
       /// </summary>
       Spot
    }

    /// <summary>
    /// Component that adds dynamic lighting to the scene.
    /// Automatically registers/unregisters itself with the scene's lighting system
    /// when the owning actor begins/ends play.
    /// </summary>
    public class LightComponent : ActorComponent
    {
       /// <summary>
       /// Defines distance-based light intensity falloff for point and spot lights.
       /// Uses the formula: 1.0 / (constant + linear * distance + quadratic * distance²)
       /// </summary>
       public class Attenuation
       {
          /// <summary>
          /// Constant term in attenuation formula. Should usually be 1.0 to avoid division by zero.
          /// </summary>
          public float constant = 1f;
          
          /// <summary>
          /// Linear term in attenuation formula. Controls linear distance falloff.
          /// Typical value: 0.09 for moderate range lights.
          /// </summary>
          public float linear = .09f;
          
          /// <summary>
          /// Quadratic term in attenuation formula. Controls exponential distance falloff.
          /// Typical value: 0.032 for moderate range lights.
          /// </summary>
          public float quadratic = .032f;
       }
       
       /// <summary>
       /// The color of the light emitted.
       /// </summary>
       public Color color = Color.White;
       
       /// <summary>
       /// Brightness multiplier applied to the light color.
       /// Values greater than 1.0 create brighter/overexposed lighting.
       /// </summary>
       public float intensity = 1;
       
       /// <summary>
       /// Determines the behavior and properties of this light.
       /// </summary>
       public ELightType lightType = ELightType.Point;

       /// <summary>
       /// Distance-based falloff parameters for point and spot lights.
       /// Not used by directional lights (they have infinite range).
       /// </summary>
       public Attenuation attenuation = new();
       
       /// <summary>
       /// Inner cone angle (in radians) for spotlights where light is at full intensity.
       /// Only used when lightType is Spot.
       /// </summary>
       public float innerCutoff;
       
       /// <summary>
       /// Outer cone angle (in radians) for spotlights where light fades to zero.
       /// Creates smooth edge falloff between innerCutoff and outerCutoff.
       /// Only used when lightType is Spot.
       /// </summary>
       public float outerCutoff;

       /// <summary>
       /// Called when the component starts playing.
       /// Registers this light with the scene's lighting system so it affects rendering.
       /// </summary>
       public override void BeginPlay()
       {
          base.BeginPlay();

          // Add this light to the scene's global light collection
          this.Actor.Scene.LightData.AddLight(this);
       }

       /// <summary>
       /// Called when the component stops playing.
       /// Unregisters this light from the scene's lighting system.
       /// </summary>
       public override void EndPlay()
       {
          base.EndPlay();

          // Remove this light from the scene's global light collection
          this.Actor.Scene.LightData.RemoveLight(this);
       }
    }
}