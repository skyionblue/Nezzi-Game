#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace LB.UI.System
{
	/// <summary>
	/// ScriptableObject that defines the configuration for a UI screen.
	/// Includes settings like screen ID, whether it can be closed, and whether it's the initial screen.
	/// </summary>
	[CreateAssetMenu(fileName = "NewUIScreenConfig", menuName = "UI/UIScreenConfig", order = 0)]
	public class UIScreenConfig : ScriptableObject
	{
		/// <summary>
		/// The unique identifier for the screen. This ID is automatically set to the name of the asset.
		/// </summary>
		public string screenID;

		/// <summary>
		/// Indicates whether this screen can be closed by the user.
		/// </summary>
		public bool canBeClosed = true;

		/// <summary>
		/// Specifies if this screen is the initial screen shown at the start.
		/// </summary>
		public bool isInitialScreen = false;

		/// <summary>
		/// Called whenever this scriptable object is modified. Automatically sets the screenID to the name of the asset.
		/// </summary>
		private void OnValidate()
		{
			screenID = name.ToLower();
#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
#endif
		}
	}
}