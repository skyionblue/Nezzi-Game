using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LB.UI.System
{
	/// <summary>
	/// Manages the entire UI system, handling screen transitions, events, and screen history.
	/// Subscribes to screen-related events and manages the hierarchy and history of UI screens.
	/// </summary>
	public class UISystemManager : MonoBehaviour
	{
		/// <summary>
		/// The configuration for the initial screen shown when the UI system starts.
		/// </summary>
		[SerializeField] protected UIScreenConfig initialScreenConfig;

		/// <summary>
		/// A list representing the hierarchy of screens within the UI system.
		/// </summary>
		[SerializeField] private List<UIScreenNode> screenHierarchy = new();

		/// <summary>
		/// A stack that keeps track of the history of opened screens, allowing for back navigation.
		/// </summary>
		protected Stack<UIScreenHistoryEntry> ScreenHistory = new();

		/// <summary>
		/// Manager responsible for handling screen transitions (e.g., animations).
		/// </summary>
		private UIScreenTransitionManager _uiScreenTransitionManager;

		/// <summary>
		/// Manager responsible for maintaining the hierarchy of UI screens.
		/// </summary>
		private UIScreenNodeManager _uiScreenNodeManager;

		/// <summary>
		/// Handles registration of UI screens within the system.
		/// </summary>
		private UIScreenRegistrar _uiScreenRegistrar;

		/// <summary>
		/// Subscribes to UI system events and initializes core managers.
		/// </summary>
		private void Awake()
		{
			UISystemEventBus.Subscribe<UIScreenOpenEvent>(HandleScreenOpenEvent);
			UISystemEventBus.Subscribe<UIScreenCloseEvent>(HandleScreenCloseEvent);
			UISystemEventBus.Subscribe<UIScreenCloseAllEvent>(HandleCloseAllScreensEvent);
			UISystemEventBus.Subscribe<UIScreenCloseLastEvent>(HandleCloseLastScreenEvent);
			UISystemEventBus.Subscribe<UIScreenRegisterEvent>(HandleScreenRegistrationEvent);
			UISystemEventBus.Subscribe<UIScreenBringToFrontEvent>(HandleScreenBringToFrontEvent);

			_uiScreenTransitionManager = new UIScreenTransitionManager();
			_uiScreenNodeManager = new UIScreenNodeManager();
			_uiScreenRegistrar = new UIScreenRegistrar(_uiScreenNodeManager);
		}

		/// <summary>
		/// Called when the UI system starts. Opens the initial screen specified by the initial screen configuration.
		/// </summary>
		private void Start()
		{
			OpenMainScreenExclusive(initialScreenConfig.screenID);
		}

		/// <summary>
		/// Unsubscribes from all UI system events when the manager is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			UISystemEventBus.Unsubscribe<UIScreenOpenEvent>(HandleScreenOpenEvent);
			UISystemEventBus.Unsubscribe<UIScreenCloseEvent>(HandleScreenCloseEvent);
			UISystemEventBus.Unsubscribe<UIScreenCloseAllEvent>(HandleCloseAllScreensEvent);
			UISystemEventBus.Unsubscribe<UIScreenCloseLastEvent>(HandleCloseLastScreenEvent);
			UISystemEventBus.Unsubscribe<UIScreenRegisterEvent>(HandleScreenRegistrationEvent);
			UISystemEventBus.Unsubscribe<UIScreenBringToFrontEvent>(HandleScreenBringToFrontEvent);
		}

		/// <summary>
		/// Updates the system every frame and handles the Escape key functionality.
		/// </summary>
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (ScreenHistory.Count <= 0)
				{
					UISystemEventBus.Publish(new UIScreenOpenEvent(initialScreenConfig, true));
				}
				else
				{
					UpdateEscFunctionality();
				}
			}
		}

		/// <summary>
		/// Functionality triggered by the Escape key. This can be overridden in derived classes.
		/// </summary>
		protected virtual void UpdateEscFunctionality()
		{
			HandleCloseLastScreen();
		}

		#region TemporaryHelper

		/// <summary>
		/// Finds a UIScreenNode by the provided IUIScreen instance.
		/// </summary>
		/// <param name="screen">The screen to find in the hierarchy.</param>
		/// <returns>The corresponding UIScreenNode, or null if not found.</returns>
		public UIScreenNode FindNodeByScreen(IUIScreen screen)
		{
			return _uiScreenNodeManager.FindUIScreenNodeByScreen(screenHierarchy, screen);
		}

		#endregion

		#region EventBus

		/// <summary>
		/// Handles the event to open a screen.
		/// </summary>
		/// <param name="evt">The event data for opening a screen.</param>
		private void HandleScreenOpenEvent(UIScreenOpenEvent evt)
		{
			if (evt.IsExclusive)
			{
				OpenMainScreenExclusive(evt.ScreenConfig.screenID);
			}
			else
			{
				OpenMainScreenAdditive(evt.ScreenConfig.screenID);
			}
		}

		/// <summary>
		/// Handles the event to close a screen.
		/// </summary>
		/// <param name="evt">The event data for closing a screen.</param>
		private void HandleScreenCloseEvent(UIScreenCloseEvent evt)
		{
			CloseScreen(evt.ScreenConfig.screenID);
		}

		/// <summary>
		/// Handles the event to close all screens.
		/// </summary>
		/// <param name="evt">The event data to close all screens.</param>
		private void HandleCloseAllScreensEvent(UIScreenCloseAllEvent evt)
		{
			HandleCloseAllScreens();
		}

		/// <summary>
		/// Handles the event to close the last screen.
		/// </summary>
		/// <param name="evt">The event data for closing the last screen.</param>
		private void HandleCloseLastScreenEvent(UIScreenCloseLastEvent evt)
		{
			HandleCloseLastScreen();
		}

		/// <summary>
		/// Handles the event to register a screen in the system.
		/// </summary>
		/// <param name="evt">The event data for registering a screen.</param>
		private void HandleScreenRegistrationEvent(UIScreenRegisterEvent evt)
		{
			_uiScreenRegistrar.RegisterScreen(screenHierarchy, evt.UIScreen);
		}

		/// <summary>
		/// Handles the event to bring a screen to the front.
		/// </summary>
		/// <param name="evt">The event data for bringing a screen to the front.</param>
		private void HandleScreenBringToFrontEvent(UIScreenBringToFrontEvent evt)
		{
			BringScreenToFront(evt.UIScreen);
		}

		#endregion

		/// <summary>
		/// Opens the main screen exclusively, closing all other screens.
		/// </summary>
		/// <param name="screenID">The ID of the screen to be opened.</param>
		protected void OpenMainScreenExclusive(string screenID)
		{
			UIScreenNode node = _uiScreenNodeManager.FindUIScreenNodeByID(screenHierarchy, screenID);
			if (node == null || node.IuiScreen == null) return;

			HandleCloseAllScreens(true, () => { ActivateScreen(screenID); });
		}

		/// <summary>
		/// Opens the screen additively, without closing other screens.
		/// </summary>
		/// <param name="screenID">The ID of the screen to be opened.</param>
		private void OpenMainScreenAdditive(string screenID)
		{
			ActivateScreen(screenID);
		}

		/// <summary>
		/// Activates a screen by its ID.
		/// </summary>
		/// <param name="screenID">The ID of the screen to activate.</param>
		private void ActivateScreen(string screenID)
		{
			UIScreenNode node = _uiScreenNodeManager.FindUIScreenNodeByID(screenHierarchy, screenID);
			if (node == null || node.IuiScreen == null) return;
			node.isActive = true;

			StartCoroutine(_uiScreenTransitionManager.TransitionOpenScreen(node.IuiScreen));
			BringScreenToFront(node.IuiScreen);
		}

		/// <summary>
		/// Brings the specified screen to the front.
		/// </summary>
		/// <param name="screen">The screen to bring to the front of the UI hierarchy.</param>
		private void BringScreenToFront(IUIScreen screen)
		{
			UIScreenNode node = _uiScreenNodeManager.FindUIScreenNodeByScreen(screenHierarchy, screen);
			if (node != null)
			{
				BringNodeAndAncestorsToFront(node);
			}

			UpdateUnityHierarchy();
			UpdateScreenHistoryFromHierarchy();
		}

		/// <summary>
		/// Brings a node and its ancestor nodes to the front.
		/// </summary>
		/// <param name="node">The node to bring to the front along with its ancestors.</param>
		private void BringNodeAndAncestorsToFront(UIScreenNode node)
		{
			UIScreenNode rootNode = _uiScreenNodeManager.GetRootNode(node, screenHierarchy);

			if (rootNode != null)
			{
				screenHierarchy.Remove(rootNode);
				screenHierarchy.Add(rootNode);
			}

			UIScreenNode currentNode = node;
			while (currentNode != rootNode)
			{
				UIScreenNode parentNode = _uiScreenNodeManager.FindParentNode(screenHierarchy, currentNode);
				if (parentNode != null)
				{
					parentNode.subScreenNodes.Remove(currentNode);
					parentNode.subScreenNodes.Add(currentNode);
				}

				currentNode = parentNode;
			}
		}

		/// <summary>
		/// Closes all screens with optional inclusion of the initial screen.
		/// </summary>
		/// <param name="includeInitialScreen">Whether the initial screen should be closed as well.</param>
		/// <param name="onComplete">Optional action to be called when all screens are closed.</param>
		private void HandleCloseAllScreens(bool includeInitialScreen = false, Action onComplete = null)
		{
			StartCoroutine(CloseAllScreensRoutine(includeInitialScreen, onComplete));
		}

		/// <summary>
		/// Coroutine to close all screens in sequence.
		/// </summary>
		/// <param name="includeInitialScreen">Whether the initial screen should be closed.</param>
		/// <param name="onComplete">Action to call when closing is complete.</param>
		/// <returns>An IEnumerator for the coroutine.</returns>
		private IEnumerator CloseAllScreensRoutine(bool includeInitialScreen, Action onComplete)
		{
			foreach (UIScreenNode screenNode in screenHierarchy)
			{
				if (!screenNode.IuiScreen.GetUIScreenConfig().canBeClosed) continue;
				if (!includeInitialScreen && screenNode.IuiScreen.GetUIScreenConfig().isInitialScreen) continue;

				yield return StartCoroutine(
					_uiScreenTransitionManager.CloseScreenAndSubScreens(this, screenNode.IuiScreen));
			}

			ScreenHistory.Clear();

			onComplete?.Invoke();
		}

		/// <summary>
		/// Handles the closing of the last opened screen.
		/// </summary>
		private void HandleCloseLastScreen()
		{
			if (ScreenHistory.Count <= 0) return;

			UIScreenHistoryEntry lastEntry = ScreenHistory.Peek();

			if (!lastEntry.Screen.GetUIScreenConfig().canBeClosed ||
			    lastEntry.Screen.GetUIScreenConfig().isInitialScreen) return;

			ScreenHistory.Pop();

			CloseScreen(lastEntry.Screen.GetUIScreenConfig().screenID);

			if (ScreenHistory.Count > 0)
			{
				UIScreenHistoryEntry previousEntry = ScreenHistory.Peek();
				StartCoroutine(_uiScreenTransitionManager.TransitionOpenScreen(previousEntry.Screen));
			}
		}

		/// <summary>
		/// Closes a specific screen and its sub-screens by ID.
		/// </summary>
		/// <param name="id">The ID of the screen to close.</param>
		protected void CloseScreen(string id)
		{
			UIScreenNode node = _uiScreenNodeManager.FindUIScreenNodeByID(screenHierarchy, id);
			if (node == null || node.IuiScreen == null)
			{
				return;
			}

			StartCoroutine(_uiScreenTransitionManager.CloseScreenAndSubScreens(this, node.IuiScreen));

			Stack<UIScreenHistoryEntry> tempStack = new Stack<UIScreenHistoryEntry>();
			while (ScreenHistory.Count > 0)
			{
				UIScreenHistoryEntry entry = ScreenHistory.Pop();
				if (entry.Screen == node.IuiScreen)
				{
					break;
				}

				tempStack.Push(entry);
			}

			while (tempStack.Count > 0)
			{
				ScreenHistory.Push(tempStack.Pop());
			}

			StartCoroutine(_uiScreenTransitionManager.CloseScreenAndSubScreens(this, node.IuiScreen));
		}

		#region Hierarchy

		/// <summary>
		/// Updates the Unity hierarchy to reflect the current screen hierarchy in the system.
		/// </summary>
		private void UpdateUnityHierarchy()
		{
			foreach (UIScreenNode node in screenHierarchy)
			{
				UpdateUnityHierarchyRecursive(node, this.transform);
			}
		}

		/// <summary>
		/// Recursively updates the Unity hierarchy for the screen nodes.
		/// </summary>
		/// <param name="node">The current screen node being processed.</param>
		/// <param name="parentTransform">The parent transform in Unity’s hierarchy.</param>
		private void UpdateUnityHierarchyRecursive(UIScreenNode node, Transform parentTransform)
		{
			if (node.IuiScreen != null)
			{
				Transform screenTransform = ((MonoBehaviour)node.IuiScreen).transform;
				screenTransform.SetParent(parentTransform, false);

				screenTransform.SetSiblingIndex(parentTransform.childCount - 1);
			}

			foreach (UIScreenNode subNode in node.subScreenNodes)
			{
				UpdateUnityHierarchyRecursive(subNode, ((MonoBehaviour)node.IuiScreen)?.transform);
			}
		}

		/// <summary>
		/// Updates the screen history based on the current screen hierarchy.
		/// </summary>
		private void UpdateScreenHistoryFromHierarchy()
		{
			ScreenHistory.Clear();

			foreach (UIScreenNode rootNode in screenHierarchy)
			{
				AddActiveScreensToHistoryStack(rootNode);
			}
		}

		/// <summary>
		/// Recursively adds active screens to the history stack.
		/// </summary>
		/// <param name="node">The current screen node being processed.</param>
		private void AddActiveScreensToHistoryStack(UIScreenNode node)
		{
			if (node.isActive && node.IuiScreen != null)
			{
				ScreenHistory.Push(new UIScreenHistoryEntry(node.IuiScreen, false));
			}

			foreach (UIScreenNode subNode in node.subScreenNodes)
			{
				AddActiveScreensToHistoryStack(subNode);
			}
		}

		#endregion
	}
}