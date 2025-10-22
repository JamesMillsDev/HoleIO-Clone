namespace HoleIO.Engine.Utility
{
    /// <summary>
    /// Utility class providing common mathematical operations and constants.
    /// Wraps System.MathF functions with more convenient names and adds game-specific helpers.
    /// Similar to Unity's Mathf class.
    /// </summary>
    public abstract class Maths
    {
       /// <summary>
       /// Conversion factor from degrees to radians.
       /// Multiply degrees by this to get radians: radians = degrees * Deg2Rad
       /// </summary>
       public static float Deg2Rad => Pi / 180f;
       
       /// <summary>
       /// Conversion factor from radians to degrees.
       /// Multiply radians by this to get degrees: degrees = radians * Rad2Deg
       /// </summary>
       public static float Rad2Deg => 180 / Pi;

       /// <summary>
       /// Gets the smallest positive float value greater than zero.
       /// Useful for floating-point comparisons and avoiding divide-by-zero.
       /// </summary>
       public static float Epsilon => float.Epsilon;
       
       /// <summary>
       /// Gets the mathematical constant Pi (π ≈ 3.14159).
       /// </summary>
       public static float Pi => MathF.PI;
       
       /// <summary>
       /// Gets the mathematical constant e (≈ 2.71828), the base of natural logarithms.
       /// </summary>
       public static float E => MathF.E;
       
       /// <summary>
       /// Gets a value representing negative infinity (-∞).
       /// </summary>
       public static float NegativeInfinity => float.NegativeInfinity;
       
       /// <summary>
       /// Gets a value representing positive infinity (+∞).
       /// </summary>
       public static float PositiveInfinity => float.PositiveInfinity;

       /// <summary>
       /// Compares two floating-point values for approximate equality.
       /// Uses a relative epsilon comparison that scales with the magnitude of the values,
       /// which is more robust than a fixed threshold for both small and large numbers.
       /// </summary>
       /// <param name="a">First value to compare</param>
       /// <param name="b">Second value to compare</param>
       /// <param name="threshold">Additional absolute tolerance (default: 0.000001)</param>
       /// <returns>True if the values are approximately equal within the threshold</returns>
       /// <example>
       /// bool equal = Maths.Compare(0.1f + 0.2f, 0.3f); // Handles floating-point precision issues
       /// </example>
       public static bool Compare(float a, float b, float threshold = .000001f)
       {
          // Relative epsilon comparison: scales tolerance with magnitude of values
          // Formula: |a - b| <= (ε + threshold) * max(1, max(|a|, |b|))
          return MathF.Abs(a - b) <= (Epsilon + threshold) * Max(1f, Max(Abs(a), Abs(b)));
       }

       /// <summary>
       /// Returns the larger of two float values.
       /// </summary>
       /// <param name="a">First value</param>
       /// <param name="b">Second value</param>
       /// <returns>The larger value</returns>
       public static float Max(float a, float b)
       {
          return MathF.Max(a, b);
       }

       /// <summary>
       /// Returns the smaller of two float values.
       /// </summary>
       /// <param name="a">First value</param>
       /// <param name="b">Second value</param>
       /// <returns>The smaller value</returns>
       public static float Min(float a, float b)
       {
          return MathF.Min(a, b);
       }

       /// <summary>
       /// Returns the absolute value of a float.
       /// </summary>
       /// <param name="a">Input value</param>
       /// <returns>The absolute value (always non-negative)</returns>
       public static float Abs(float a)
       {
          return MathF.Abs(a);
       }

       /// <summary>
       /// Clamps a value between a minimum and maximum range.
       /// If value is less than min, returns min. If greater than max, returns max. Otherwise returns value.
       /// </summary>
       /// <param name="value">Value to clamp</param>
       /// <param name="min">Minimum allowed value</param>
       /// <param name="max">Maximum allowed value</param>
       /// <returns>The clamped value</returns>
       /// <example>
       /// float health = Maths.Clamp(damage, 0f, 100f); // Keep health between 0-100
       /// </example>
       public static float Clamp(float value, float min, float max)
       {
          if (value < min)
          {
             return min;
          }

          return value > max ? max : value;
       }

       /// <summary>
       /// Clamps a value between 0 and 1.
       /// Commonly used for normalizing values like alpha, percentages, or interpolation factors.
       /// </summary>
       /// <param name="value">Value to clamp</param>
       /// <returns>The clamped value between 0 and 1</returns>
       /// <example>
       /// float alpha = Maths.Clamp01(fadeAmount); // Ensure alpha is valid for rendering
       /// </example>
       public static float Clamp01(float value)
       {
          return Clamp(value, 0f, 1f);
       }

       /// <summary>
       /// Returns the sine of the specified angle in radians.
       /// </summary>
       /// <param name="value">Angle in radians</param>
       /// <returns>Sine of the angle (range: -1 to 1)</returns>
       public static float Sin(float value)
       {
          return MathF.Sin(value);
       }

       /// <summary>
       /// Returns the cosine of the specified angle in radians.
       /// </summary>
       /// <param name="value">Angle in radians</param>
       /// <returns>Cosine of the angle (range: -1 to 1)</returns>
       public static float Cos(float value)
       {
          return MathF.Cos(value);
       }

       /// <summary>
       /// Returns the angle in radians whose tangent is the quotient of two specified numbers.
       /// Handles all four quadrants correctly (unlike simple atan(y/x)).
       /// </summary>
       /// <param name="y">The y coordinate of a point</param>
       /// <param name="x">The x coordinate of a point</param>
       /// <returns>Angle in radians (range: -π to π)</returns>
       /// <example>
       /// float angle = Maths.Atan2(targetY - posY, targetX - posX); // Calculate direction to target
       /// </example>
       public static float Atan2(float y, float x)
       {
          return MathF.Atan2(y, x);
       }

       /// <summary>
       /// Returns the arcsine (inverse sine) of the specified value.
       /// </summary>
       /// <param name="value">A number representing a sine (range: -1 to 1)</param>
       /// <returns>Angle in radians (range: -π/2 to π/2)</returns>
       public static float Asin(float value)
       {
          return MathF.Asin(value);
       }

       /// <summary>
       /// Returns the square root of a specified number.
       /// </summary>
       /// <param name="value">The number whose square root is to be found</param>
       /// <returns>The positive square root of value</returns>
       public static float Sqrt(float value)
       {
          return MathF.Sqrt(value);
       }

       /// <summary>
       /// Returns the sign of a floating-point number.
       /// </summary>
       /// <param name="value">A signed number</param>
       /// <returns>1 if positive, -1 if negative, 0 if zero</returns>
       public static float Sign(float value)
       {
          return MathF.Sign(value);
       }
    }
}