namespace LB.UI.System
{
	/// <summary>
	/// Event raised when a screen is requested to be opened.
	/// </summary>
	public class UIScreenOpenEvent
	{
		/// <summary>
		/// Configuration data for the screen that should be opened.
		/// </summary>
		public UIScreenConfig ScreenConfig { get; }

		/// <summary>
		/// Indicates whether the screen should be opened exclusively (closing other screens).
		/// </summary>
		public bool IsExclusive { get; }

		/// <summary>
		/// Initializes a new instance of the UIScreenOpenEvent class.
		/// </summary>
		/// <param name="screenConfig">The configuration data for the screen.</param>
		/// <param name="isExclusive">Whether the screen should be opened exclusively.</param>
		public UIScreenOpenEvent(UIScreenConfig screenConfig, bool isExclusive)
		{
			ScreenConfig = screenConfig;
			IsExclusive = isExclusive;
		}
	}

	/// <summary>
	/// Event raised when a screen is requested to be closed.
	/// </summary>
	public class UIScreenCloseEvent
	{
		/// <summary>
		/// Configuration data for the screen that should be closed.
		/// </summary>
		public UIScreenConfig ScreenConfig { get; }

		/// <summary>
		/// Constructs a new UIScreenCloseEvent.
		/// </summary>
		/// <param name="screenConfig">The configuration data for the screen.</param>
		public UIScreenCloseEvent(UIScreenConfig screenConfig)
		{
			ScreenConfig = screenConfig;
		}
	}

	/// <summary>
	/// Represents an event to close all screens.
	/// </summary>
	public class UIScreenCloseAllEvent
	{
	}

	/// <summary>
	/// Represents an event to close the last opened screen.
	/// </summary>
	public class UIScreenCloseLastEvent
	{
	}

	/// <summary>
	/// Event raised when a screen is registered with the UI system.
	/// </summary>
	public class UIScreenRegisterEvent
	{
		/// <summary>
		/// The screen being registered.
		/// </summary>

		public IUIScreen UIScreen { get; }

		/// <summary>
		/// Initializes a new instance of the UIScreenRegisterEvent class.
		/// </summary>
		/// <param name="uiScreen">The screen to be registered.</param>
		public UIScreenRegisterEvent(IUIScreen uiScreen)
		{
			UIScreen = uiScreen;
		}
	}

	/// <summary>
	/// Event raised to bring a screen to the front of the UI stack.
	/// </summary>
	public class UIScreenBringToFrontEvent
	{
		/// <summary>
		/// The screen to be brought to the front.
		/// </summary>

		public IUIScreen UIScreen { get; }

		/// <summary>
		/// Initializes a new instance of the UIScreenBringToFrontEvent class.
		/// </summary>
		/// <param name="uiScreen">The screen to bring to the front.</param>
		public UIScreenBringToFrontEvent(IUIScreen uiScreen)
		{
			UIScreen = uiScreen;
		}
	}
}