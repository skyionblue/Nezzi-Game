using UnityEngine;

namespace LB.UI.System
{
	/// <summary>
	/// A draggable screen in the UI system, extending the base UISystemScreen.
	/// </summary>
	[RequireComponent(typeof(DraggableUIComponent))]
	public class UISystemScreenDraggable : UISystemScreen
	{
		/// <summary>
		/// The draggable component attached to this screen.
		/// </summary>
		private DraggableUIComponent _draggableUIComponent;

		/// <summary>
		/// Initializes the draggable component and sets up the screen.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			_draggableUIComponent = GetComponent<DraggableUIComponent>();
		}

		/// <summary>
		/// Resets the screen and also resets its draggable position.
		/// </summary>
		public override void ResetToDefault()
		{
			base.ResetToDefault();
			_draggableUIComponent.ResetToOriginalPosition();
		}
	}
}