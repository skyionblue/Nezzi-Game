using UnityEngine;

namespace LB.UI.System
{
	/// <summary>
	/// Specialized UI manager for in-game screens. Adds additional functionality for opening
	/// the pause menu and handling specific in-game scenarios.
	/// </summary>
	public class UISystemManagerInGame : UISystemManager
	{
		/// <summary>
		/// Determines if the pause menu can be opened during gameplay.
		/// </summary>
		[SerializeField] private bool canOpenPauseMenu;

		/// <summary>
		/// Configuration for the pause menu screen.
		/// </summary>
		[SerializeField] private UIScreenConfig pauseMenuConfig;

		/// <summary>
		/// Extends the default escape key functionality to handle pause menu actions.
		/// </summary>
		protected override void UpdateEscFunctionality()
		{
			// Logic to toggle between the initial screen and the pause menu when the escape key is pressed.
			if (canOpenPauseMenu)
			{
				if (ScreenHistory.Count == 1 &&
				    ScreenHistory.Peek().Screen.GetUIScreenConfig().isInitialScreen)
				{
					OpenMainScreenExclusive(pauseMenuConfig.screenID);
					return;
				}

				if (ScreenHistory.Count == 1 &&
				    ScreenHistory.Peek().Screen.GetUIScreenConfig() == pauseMenuConfig)
				{
					OpenMainScreenExclusive(initialScreenConfig.screenID);
					return;
				}
			}

			// Call base functionality for other screens.
			base.UpdateEscFunctionality();
		}
	}
}