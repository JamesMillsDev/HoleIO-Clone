using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace HoleIO.Engine.Core
{
	/// <summary>
	/// Singleton class that manages input from keyboard, mouse, and other input devices.
	/// Provides a centralized interface for querying input state and registering input callbacks.
	/// </summary>
	public static class Input
	{
		/// <summary>
		/// The Silk.NET input context that manages all input devices.
		/// </summary>
		private static IInputContext inputContext = null!;

		/// <summary>
		/// Primary keyboard device for input queries.
		/// </summary>
		private static IKeyboard primaryKeyboard = null!;

		/// <summary>
		/// Primary mouse device for input queries.
		/// </summary>
		private static IMouse primaryMouse = null!;

		/// <summary>
		/// Tracks keys that were pressed in the previous frame for detecting key press events.
		/// </summary>
		private static readonly HashSet<Key> previousKeyStates = [];

		/// <summary>
		/// Tracks keys that are currently pressed this frame.
		/// </summary>
		private static readonly HashSet<Key> currentKeyStates = [];

		/// <summary>
		/// Tracks mouse buttons that were pressed in the previous frame for detecting button press events.
		/// </summary>
		private static readonly HashSet<MouseButton> previousMouseStates = [];

		/// <summary>
		/// Tracks mouse buttons that are currently pressed this frame.
		/// </summary>
		private static readonly HashSet<MouseButton> currentMouseStates = [];

		/// <summary>
		/// Previous mouse position for calculating delta movement.
		/// </summary>
		private static Vector2 previousMousePosition;

		/// <summary>
		/// Current mouse position.
		/// </summary>
		private static Vector2 currentMousePosition;

		/// <summary>
		/// Accumulated mouse scroll delta this frame.
		/// </summary>
		private static float mouseScrollDelta;

		/// <summary>
		/// Flag to handle first mouse move event (prevents large delta on startup).
		/// </summary>
		private static bool firstMouseMove = true;

		/// <summary>
		/// Initializes the input system with the given window.
		/// Creates the input context and sets up primary keyboard and mouse devices.
		/// Should be called once during application startup.
		/// </summary>
		/// <param name="window">The window to create input context from</param>
		internal static void Initialize(IWindow window)
		{
			inputContext = window.CreateInput();

			// Get primary keyboard (first available keyboard device)
			if (inputContext.Keyboards.Count > 0)
			{
				primaryKeyboard = inputContext.Keyboards[0];

				// Register keyboard callbacks
				primaryKeyboard.KeyDown += OnKeyDown;
				primaryKeyboard.KeyUp += OnKeyUp;
			}

			// Get primary mouse (first available mouse device)
			if (inputContext.Mice.Count > 0)
			{
				primaryMouse = inputContext.Mice[0];

				// Register mouse callbacks
				primaryMouse.MouseDown += OnMouseDown;
				primaryMouse.MouseUp += OnMouseUp;
				primaryMouse.MouseMove += OnMouseMove;
				primaryMouse.Scroll += OnMouseScroll;

				// Initialize mouse position
				currentMousePosition = primaryMouse.Position;
				previousMousePosition = currentMousePosition;
			}
		}

		/// <summary>
		/// Checks if a key is currently being held down.
		/// </summary>
		/// <param name="key">The key to check</param>
		/// <returns>True if the key is currently down</returns>
		public static bool IsKeyDown(Key key)
		{
			return primaryKeyboard?.IsKeyPressed(key) ?? false;
		}

		/// <summary>
		/// Checks if a key is currently up (not pressed).
		/// </summary>
		/// <param name="key">The key to check</param>
		/// <returns>True if the key is currently up</returns>
		public static bool IsKeyUp(Key key)
		{
			return !IsKeyDown(key);
		}

		/// <summary>
		/// Checks if a key was pressed this frame (transition from up to down).
		/// </summary>
		/// <param name="key">The key to check</param>
		/// <returns>True if the key was pressed this frame</returns>
		public static bool WasKeyPressed(Key key)
		{
			return currentKeyStates.Contains(key) && !previousKeyStates.Contains(key);
		}

		/// <summary>
		/// Checks if a key was released this frame (transition from down to up).
		/// </summary>
		/// <param name="key">The key to check</param>
		/// <returns>True if the key was released this frame</returns>
		public static bool WasKeyReleased(Key key)
		{
			return !currentKeyStates.Contains(key) && previousKeyStates.Contains(key);
		}

		/// <summary>
		/// Gets all keys that are currently pressed.
		/// </summary>
		/// <returns>Collection of currently pressed keys</returns>
		public static IEnumerable<Key> GetPressedKeys()
		{
			return currentKeyStates;
		}

		/// <summary>
		/// Checks if a mouse button is currently being held down.
		/// </summary>
		/// <param name="button">The mouse button to check</param>
		/// <returns>True if the button is currently down</returns>
		public static bool IsMouseButtonDown(MouseButton button)
		{
			return primaryMouse?.IsButtonPressed(button) ?? false;
		}

		/// <summary>
		/// Checks if a mouse button is currently up (not pressed).
		/// </summary>
		/// <param name="button">The mouse button to check</param>
		/// <returns>True if the button is currently up</returns>
		public static bool IsMouseButtonUp(MouseButton button)
		{
			return !IsMouseButtonDown(button);
		}

		/// <summary>
		/// Checks if a mouse button was pressed this frame (transition from up to down).
		/// </summary>
		/// <param name="button">The mouse button to check</param>
		/// <returns>True if the button was pressed this frame</returns>
		public static bool WasMouseButtonPressed(MouseButton button)
		{
			return currentMouseStates.Contains(button) && !previousMouseStates.Contains(button);
		}

		/// <summary>
		/// Checks if a mouse button was released this frame (transition from down to up).
		/// </summary>
		/// <param name="button">The mouse button to check</param>
		/// <returns>True if the button was released this frame</returns>
		public static bool WasMouseButtonReleased(MouseButton button)
		{
			return !currentMouseStates.Contains(button) && previousMouseStates.Contains(button);
		}

		/// <summary>
		/// Gets the current X position of the mouse in screen coordinates.
		/// </summary>
		/// <returns>Mouse X position</returns>
		public static float GetMouseX()
		{
			return currentMousePosition.X;
		}

		/// <summary>
		/// Gets the current Y position of the mouse in screen coordinates.
		/// </summary>
		/// <returns>Mouse Y position</returns>
		public static float GetMouseY()
		{
			return currentMousePosition.Y;
		}

		/// <summary>
		/// Gets the current mouse position.
		/// </summary>
		/// <returns>Mouse position as a Vector2</returns>
		public static Vector2 GetMousePosition()
		{
			return currentMousePosition;
		}

		/// <summary>
		/// Gets how much the mouse moved horizontally this frame.
		/// </summary>
		/// <returns>Mouse X delta</returns>
		public static float GetMouseDeltaX()
		{
			return currentMousePosition.X - previousMousePosition.X;
		}

		/// <summary>
		/// Gets how much the mouse moved vertically this frame.
		/// </summary>
		/// <returns>Mouse Y delta</returns>
		public static float GetMouseDeltaY()
		{
			return currentMousePosition.Y - previousMousePosition.Y;
		}

		/// <summary>
		/// Gets how much the mouse moved this frame as a vector.
		/// </summary>
		/// <returns>Mouse movement delta as a Vector2</returns>
		public static Vector2 GetMouseDelta()
		{
			return currentMousePosition - previousMousePosition;
		}

		/// <summary>
		/// Gets how far the mouse wheel was scrolled this frame.
		/// Positive values indicate scrolling up, negative values indicate scrolling down.
		/// </summary>
		/// <returns>Mouse scroll delta</returns>
		public static float GetMouseScroll()
		{
			return mouseScrollDelta;
		}

		/// <summary>
		/// Clears frame-specific input state.
		/// Should be called at the beginning of each frame before processing input.
		/// </summary>
		internal static void ClearFrameState()
		{
			// Update previous states for next frame comparison
			previousKeyStates.Clear();
			foreach (Key key in currentKeyStates)
			{
				previousKeyStates.Add(key);
			}

			previousMouseStates.Clear();
			foreach (MouseButton button in currentMouseStates)
			{
				previousMouseStates.Add(button);
			}

			// Update mouse position
			previousMousePosition = currentMousePosition;

			// Reset scroll delta (accumulates within a frame)
			mouseScrollDelta = 0f;
		}

		/// <summary>
		/// Callback invoked when a key is pressed down.
		/// </summary>
		private static void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
		{
			currentKeyStates.Add(key);
		}

		/// <summary>
		/// Callback invoked when a key is released.
		/// </summary>
		private static void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
		{
			currentKeyStates.Remove(key);
		}

		/// <summary>
		/// Callback invoked when a mouse button is pressed down.
		/// </summary>
		private static void OnMouseDown(IMouse mouse, MouseButton button)
		{
			currentMouseStates.Add(button);
		}

		/// <summary>
		/// Callback invoked when a mouse button is released.
		/// </summary>
		private static void OnMouseUp(IMouse mouse, MouseButton button)
		{
			currentMouseStates.Remove(button);
		}

		/// <summary>
		/// Callback invoked when the mouse moves.
		/// </summary>
		private static void OnMouseMove(IMouse mouse, Vector2 position)
		{
			// Handle first mouse move to prevent large delta on startup
			if (firstMouseMove)
			{
				previousMousePosition = position;
				firstMouseMove = false;
			}

			currentMousePosition = position;
		}

		/// <summary>
		/// Callback invoked when the mouse wheel is scrolled.
		/// </summary>
		private static void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
		{
			// Accumulate scroll delta (Y component is vertical scroll)
			mouseScrollDelta += scroll.Y;
		}

		/// <summary>
		/// Cleans up the input system and disposes of the input context.
		/// Should be called during application shutdown.
		/// </summary>
		internal static void Shutdown()
		{
			inputContext.Dispose();
		}
	}
}