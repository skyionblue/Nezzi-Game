using System;
using System.Collections.Generic;

namespace LB.UI.System
{
	/// <summary>
	/// Manages the hierarchical structure of UI screens, providing methods
	/// to create and navigate between screen nodes.
	/// </summary>

	public class UIScreenNodeManager
	{
		/// <summary>
		/// Finds a UIScreenNode by the associated IUIScreen object.
		/// </summary>
		public UIScreenNode FindUIScreenNodeByScreen(List<UIScreenNode> screenHierarchy, IUIScreen screen)
		{
			return FindUIScreenNodeByCondition(screenHierarchy, node => node.IuiScreen == screen);
		}

		/// <summary>
		/// Finds a UIScreenNode by its screenNodeID.
		/// </summary>
		public UIScreenNode FindUIScreenNodeByID(List<UIScreenNode> screenHierarchy, string screenNodeID)
		{
			return FindUIScreenNodeByCondition(screenHierarchy, node => node.screenNodeID == screenNodeID);
		}

		/// <summary>
		/// Finds a UIScreenNode by the associated UISystemScreen component.
		/// </summary>
		public UIScreenNode FindUIScreenNodeByComponent(List<UIScreenNode> screenHierarchy,
			UISystemScreen uiSystemScreen)
		{
			return FindUIScreenNodeByCondition(screenHierarchy,
				node => (UISystemScreen)node.IuiScreen == uiSystemScreen);
		}

		/// <summary>
		/// Returns a list of all IUIScreens in the hierarchy and their sub-screens.
		/// </summary>
		public List<IUIScreen> GetAllIUIScreens(List<UIScreenNode> screenHierarchy)
		{
			return GetAllIUIScreens(screenHierarchy, reversed: false);
		}

		/// <summary>
		/// Returns a list of all IUIScreens in the hierarchy and their sub-screens, in reverse order.
		/// </summary>
		public List<IUIScreen> GetAllIUIScreensReversed(List<UIScreenNode> screenHierarchy)
		{
			return GetAllIUIScreens(screenHierarchy, reversed: true);
		}

		/// <summary>
		/// Finds the parent node of the given UIScreenNode in the hierarchy.
		/// </summary>
		public UIScreenNode FindParentNode(List<UIScreenNode> screenHierarchy, UIScreenNode childNode)
		{
			return FindNodeByCondition(screenHierarchy, node => node.subScreenNodes.Contains(childNode));
		}

		/// <summary>
		/// Finds the root node for a given screen node in the hierarchy.
		/// </summary>
		public UIScreenNode GetRootNode(UIScreenNode node, List<UIScreenNode> screenHierarchy)
		{
			if (screenHierarchy.Contains(node))
			{
				return node;
			}

			return FindNodeByCondition(screenHierarchy, rootNode => FindNodeInSubScreens(rootNode, node) != null);
		}


		/// <summary>
		/// Generic method to find a UIScreenNode by a custom condition.
		/// </summary>
		private UIScreenNode FindUIScreenNodeByCondition(List<UIScreenNode> screenHierarchy,
			Func<UIScreenNode, bool> condition)
		{
			return FindNodeByCondition(screenHierarchy, condition);
		}

		/// <summary>
		/// Generic method to recursively find a UIScreenNode by a custom condition.
		/// </summary>
		private UIScreenNode FindNodeByCondition(List<UIScreenNode> nodes, Func<UIScreenNode, bool> condition)
		{
			foreach (UIScreenNode node in nodes)
			{
				if (condition(node))
				{
					return node;
				}

				UIScreenNode foundNode = FindNodeByCondition(node.subScreenNodes, condition);
				if (foundNode != null)
				{
					return foundNode;
				}
			}

			return null;
		}

		/// <summary>
		/// Recursively searches for a target node within the sub-nodes of a given parent node.
		/// </summary>
		private UIScreenNode FindNodeInSubScreens(UIScreenNode parentNode, UIScreenNode targetNode)
		{
			return FindNodeByCondition(parentNode.subScreenNodes, node => node == targetNode);
		}

		/// <summary>
		/// Returns a list of all IUIScreens in the hierarchy, optionally in reverse order.
		/// </summary>
		private List<IUIScreen> GetAllIUIScreens(List<UIScreenNode> screenHierarchy, bool reversed)
		{
			List<IUIScreen> screens = new List<IUIScreen>();
			GetAllIUIScreensRecursive(screenHierarchy, screens, reversed);
			return screens;
		}

		/// <summary>
		/// Recursive helper method to populate a list of all IUIScreens in the hierarchy.
		/// </summary>
		private void GetAllIUIScreensRecursive(List<UIScreenNode> nodes, List<IUIScreen> screens, bool reversed)
		{
			foreach (UIScreenNode node in nodes)
			{
				if (reversed)
				{
					GetAllIUIScreensRecursive(node.subScreenNodes, screens, true);
				}

				if (node.IuiScreen != null)
				{
					screens.Add(node.IuiScreen);
				}

				if (!reversed)
				{
					GetAllIUIScreensRecursive(node.subScreenNodes, screens, false);
				}
			}
		}
	}
}