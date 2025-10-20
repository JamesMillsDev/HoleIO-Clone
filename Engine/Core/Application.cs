using HoleIO.Engine.Gameplay;
using HoleIO.Engine.Utility;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace HoleIO.Engine.Core
{
	public class Application
	{
		private static Application? instance;

		public static void Open<TGameInstance>() where TGameInstance : GameInstance, new()
		{
			if (instance != null)
			{
				throw new InvalidOperationException("Application is already opened.");
			}

			instance = new Application(new TGameInstance());

			instance.Run();

			instance = null;
		}

		public static void Quit()
		{
			if (instance == null)
			{
				throw new InvalidOperationException("Application is not open.");
			}

			instance.window!.Close();
		}

		public static GL OpenGlContext() => instance == null
			? throw new InvalidOperationException("Application is not open.")
			: instance.window?.openGlContext!;

		public static Window OpenGlWindow() => instance == null
			? throw new InvalidOperationException("Application is not open.")
			: instance.window!;

		private readonly Config<EngineConfigData> config = new("Engine");
		private Window? window;
		private readonly GameInstance gameInstance;

		private Application(GameInstance gameInstance)
		{
			this.gameInstance = gameInstance;
		}

		private void Run()
		{
			this.window = new Window(this.config.Get());

			try
			{
				this.window.Open(Load, Tick, Render, Unload);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void Load()
		{
			this.gameInstance.BeginPlay();
			this.gameInstance.currentScene.rootActor.BeginPlay();
		}

		private void Tick(double deltaTime)
		{
			Time.Tick(deltaTime, this.window!.window!.Time, this.window.window.WindowState == WindowState.Minimized);
			this.gameInstance.Tick();
			this.gameInstance.currentScene.ApplyChanges();
			this.gameInstance.currentScene.rootActor.Tick();
		}

		private void Render(double deltaTime)
		{
			this.gameInstance.Render();
			this.gameInstance.currentScene.rootActor.Render();
		}

		private void Unload()
		{
			this.gameInstance.currentScene.rootActor.EndPlay();
			this.gameInstance.EndPlay();
		}
	}
}