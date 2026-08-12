using UnityEngine;

namespace LB.UI.System
{
	public interface IUIScreen
	{
		UIScreenConfig GetUIScreenConfig();
		CanvasGroup GetCanvasGroup();
		void ResetToDefault();
		void RegisterScreen();
	}
}