using System.Collections.Generic;
using UnityEngine;

namespace LB.UI.System
{
	/// <summary>
	/// Registers screens with the UI system so they can be managed
	/// and handled during screen transitions and events.
	/// </summary>
	public class UIScreenRegistrar
	{
		private readonly UIScreenNodeManager _uiScreenNodeManager;

		/// <summary>
		/// Initializes a new instance of the UIScreenRegistrar class.
		/// </summary>
		/// <param name="nodeManager">The manager responsible for the screen hierarchy.</param>
		public UIScreenRegistrar(UIScreenNodeManager nodeManager)
		{
			this._uiScreenNodeManager = nodeManager;
		}

		/// <summary>
		/// Registers a screen by creating its node in the hierarchy. 
		/// The screen is added either as a root screen or as a child of another screen (parent screen).
		/// </summary>
		/// <param name="screenHierarchy">The hierarchy of the current UI screens.</param>
		/// <param name="screen">The screen to be registered.</param>
		public void RegisterScreen(List<UIScreenNode> screenHierarchy, IUIScreen screen)
		{
			string id = screen.GetUIScreenConfig().screenID;

			if (_uiScreenNodeManager.FindUIScreenNodeByID(screenHierarchy, id) != null) return;

			UIScreenNode node = new UIScreenNode
			{
				screenNodeID = id,
				IuiScreen = screen,
				isActive = false
			};

			Transform parentTransform = ((MonoBehaviour)screen).transform.parent;
			IUIScreen parentScreen = parentTransform?.GetComponentInParent<IUIScreen>(true);

			if (parentScreen != null)
			{
				UIScreenNode parentNode = _uiScreenNodeManager.FindUIScreenNodeByScreen(screenHierarchy, parentScreen);
				if (parentNode != null)
				{
					parentNode.subScreenNodes.Add(node);
				}
				else
				{
					RegisterScreen(screenHierarchy, parentScreen);
					parentNode = _uiScreenNodeManager.FindUIScreenNodeByScreen(screenHierarchy, parentScreen);
					parentNode?.subScreenNodes.Add(node);
				}
			}
			else
			{
				screenHierarchy.Add(node);
			}

			SortScreenHierarchy(screenHierarchy);
		}

		/// <summary>
		/// Sorts the screen hierarchy based on their sibling index in the Unity hierarchy.
		/// Ensures that the screens are ordered correctly in the UI.
		/// </summary>
		/// <param name="screenHierarchy">The hierarchy of screens to be sorted.</param>
		private void SortScreenHierarchy(List<UIScreenNode> screenHierarchy)
		{
			screenHierarchy.Sort((a, b) =>
				((MonoBehaviour)a.IuiScreen).transform.GetSiblingIndex().CompareTo(
					((MonoBehaviour)b.IuiScreen).transform.GetSiblingIndex()));

			foreach (var node in screenHierarchy)
			{
				SortScreenHierarchy(node.subScreenNodes);
			}
		}
	}
}