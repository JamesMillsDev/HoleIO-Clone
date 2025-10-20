namespace HoleIO.Engine.Core
{
	public static class Time
	{
		public static float DeltaTime { get; private set; }
		public static float ElapsedTime { get; private set; }
		public static uint FPS { get; private set; }

		private static float fpsInterval;
		private static uint frames;

		internal static void Tick(double deltaTime, double elapsedTime, bool iconified)
		{
			DeltaTime = (float)deltaTime;
			ElapsedTime = (float)elapsedTime;

			if (iconified)
			{
				return;
			}

			frames++;
			fpsInterval += DeltaTime;

			if (fpsInterval < 1f)
			{
				return;
			}

			FPS = frames;
			frames = 0;
			fpsInterval -= 1f;
		}
	}
}