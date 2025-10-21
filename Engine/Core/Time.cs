namespace HoleIO.Engine.Core
{
    /// <summary>
    /// Provides global time information for the game loop.
    /// Similar to Unity's Time class, offering delta time, elapsed time, and FPS tracking.
    /// </summary>
    public static class Time
    {
       /// <summary>
       /// Gets the time in seconds since the last frame.
       /// Use this to make movement frame-rate independent (e.g., position += velocity * DeltaTime).
       /// </summary>
       public static float DeltaTime { get; private set; }
       
       /// <summary>
       /// Gets the total time in seconds since the application started.
       /// Useful for animations, timers, and time-based events.
       /// </summary>
       public static float ElapsedTime { get; private set; }
       
       /// <summary>
       /// Gets the current frames per second (FPS).
       /// Updated once per second based on frame count.
       /// </summary>
       public static uint FPS { get; private set; }

       // Accumulator for tracking time until the next FPS update (resets every second)
       private static float fpsInterval;
       
       // Frame counter that increments each frame and resets when FPS is calculated
       private static uint frames;

       /// <summary>
       /// Updates time values. Should be called once per frame by the engine's main loop.
       /// </summary>
       /// <param name="deltaTime">Time in seconds since the last frame</param>
       /// <param name="elapsedTime">Total time in seconds since application start</param>
       /// <param name="iconified">Whether the window is minimized (pauses FPS counting if true)</param>
       internal static void Tick(double deltaTime, double elapsedTime, bool iconified)
       {
          // Update public time values (convert from double to float for convenience)
          DeltaTime = (float)deltaTime;
          ElapsedTime = (float)elapsedTime;

          // Don't count frames when window is minimized (saves CPU and gives accurate FPS)
          if (iconified)
          {
             return;
          }

          // Increment frame counter
          frames++;
          
          // Accumulate time for FPS calculation
          fpsInterval += DeltaTime;

          // Calculate FPS once per second
          if (fpsInterval < 1f)
          {
             return;
          }

          // Update FPS to the number of frames rendered in the last second
          FPS = frames;
          
          // Reset counters for next second
          frames = 0;
          
          // Subtract 1 second but keep any overflow to maintain accuracy
          // (e.g., if fpsInterval is 1.05s, it becomes 0.05s for the next cycle)
          fpsInterval -= 1f;
       }
    }
}