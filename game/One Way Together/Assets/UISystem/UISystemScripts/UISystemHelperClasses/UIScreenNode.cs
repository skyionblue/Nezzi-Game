using System;
using System.Collections.Generic;
using UnityEngine;

namespace LB.UI.System
{    /// <summary>
	/// Represents a node in the UI screen hierarchy, managing individual screens
	/// and their relationships within the UI system.
	/// </summary>

	
	/// Make it Serializable when you want to check the screen hierarchy in the Inspector
	//[Serializable]
	public class UIScreenNode
	{      
		[HideInInspector] public string screenNodeID;
		[HideInInspector] public IUIScreen IuiScreen;
		public bool isActive = true;
		public List<UIScreenNode> subScreenNodes = new List<UIScreenNode>();
	}
}