using UnityEngine;
using UnityEngine.EventSystems;

namespace LB.UI.System
{
	/// <summary>
	/// Base class for UI screens, providing common functionality like opening,
	/// closing, and resetting screens.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class UISystemScreen : MonoBehaviour, IUIScreen, IUIToggleScreen, IPointerDownHandler
	{
		/// <summary>
		/// The configuration settings for this screen.
		/// </summary>
		[SerializeField] private UIScreenConfig uiScreenConfig;

		/// <summary>
		/// The CanvasGroup component attached to this screen, used for managing visibility and interaction.
		/// </summary>
		protected CanvasGroup CanvasGroup;

		/// <summary>
		/// Initializes the CanvasGroup and disables interaction initially.
		/// </summary>
		protected virtual void Awake()
		{
			CanvasGroup = GetComponent<CanvasGroup>();
			CanvasGroup.alpha = 0f;
			CanvasGroup.blocksRaycasts = false;
			CanvasGroup.interactable = false;
		}

		/// <summary>
		/// Registers the screen with the system when the scene starts.
		/// </summary>
		protected virtual void Start()
		{
			RegisterScreen();
		}

		/// <summary>
		/// Gets the configuration for this screen.
		/// </summary>
		/// <returns>The UIScreenConfig for this screen.</returns>
		public UIScreenConfig GetUIScreenConfig() => uiScreenConfig;

		/// <summary>
		/// Gets the CanvasGroup component attached to this screen.
		/// </summary>
		/// <returns>The CanvasGroup of this screen.</returns>
		public CanvasGroup GetCanvasGroup() => CanvasGroup;

		/// <summary>
		/// Resets the screen to its default state.
		/// </summary>
		public virtual void ResetToDefault()
		{
		}

		#region EventBus

		/// <summary>
		/// Opens the screen additively, without closing other screens.
		/// </summary>
		/// <param name="screenConfig">The configuration of the screen to be opened.</param>
		public void OpenScreenAdditive(UIScreenConfig screenConfig)
		{
			UISystemEventBus.Publish(new UIScreenOpenEvent(screenConfig, false));
		}

		/// <summary>
		/// Opens the screen exclusively, closing other screens.
		/// </summary>
		/// <param name="screenConfig">The configuration of the screen to be opened.</param>
		public void OpenScreenExclusive(UIScreenConfig screenConfig)
		{
			UISystemEventBus.Publish(new UIScreenOpenEvent(screenConfig, true));
		}

		/// <summary>
		/// Closes the screen.
		/// </summary>
		/// <param name="screenConfig">The configuration of the screen to be closed.</param>
		public void CloseScreen(UIScreenConfig screenConfig)
		{
			UISystemEventBus.Publish(new UIScreenCloseEvent(screenConfig));
		}

		/// <summary>
		/// Closes all screens.
		/// </summary>
		public void CloseAllScreens()
		{
			UISystemEventBus.Publish(new UIScreenCloseAllEvent());
		}

		/// <summary>
		/// Closes the last opened screen.
		/// </summary>
		public void CloseLastScreen()
		{
			UISystemEventBus.Publish(new UIScreenCloseLastEvent());
		}

		/// <summary>
		/// Registers this screen with the UI system.
		/// </summary>
		public void RegisterScreen()
		{
			UISystemEventBus.Publish(new UIScreenRegisterEvent(this));
		}

		#endregion

		#region OrderLayering

		/// <summary>
		/// Called when the user clicks on the screen, bringing it to the front.
		/// </summary>
		/// <param name="eventData">Pointer event data for the click.</param>
		public void OnPointerDown(PointerEventData eventData)
		{
			UISystemEventBus.Publish(new UIScreenBringToFrontEvent(this));
		}

		#endregion
	}
}