using System.Collections;
using UnityEngine;

namespace LB.UI.System
{
	public class UIScreenTransitionManager
	{
		private const float FadeInDuration = 0f;
		private const float FadeOutDuration = 0f;

		public IEnumerator CloseScreenAndSubScreens(UISystemManager systemManager, IUIScreen screen)
		{
			UIScreenNode node = systemManager.FindNodeByScreen(screen);

			if (node != null)
			{
				foreach (UIScreenNode subNode in node.subScreenNodes)
				{
					if (subNode.isActive)
					{
						yield return CloseScreenAndSubScreens(systemManager, subNode.IuiScreen);
					}
				}

				if (node.isActive)
				{
					yield return FadeOutScreen(screen, FadeOutDuration);
					screen.ResetToDefault();
					node.isActive = false; 
				}
			}
		}

		public IEnumerator TransitionScreen(IUIScreen oldScreen, IUIScreen newScreen)
		{
			if (oldScreen != null)
			{
				yield return FadeOutScreen(oldScreen, FadeOutDuration);
				oldScreen.ResetToDefault();
			}

			if (newScreen != null)
			{
				yield return FadeInScreen(newScreen, FadeInDuration);
			}
		}

		public IEnumerator TransitionOpenScreen(IUIScreen screen)
		{
			yield return FadeInScreen(screen, FadeInDuration);
		}

		private IEnumerator FadeInScreen(IUIScreen screen, float duration)
		{
			CanvasGroup canvasGroup = screen.GetCanvasGroup();
			float elapsedTime = 0;

			while (elapsedTime < duration)
			{
				canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			canvasGroup.alpha = 1;
		}

		private IEnumerator FadeOutScreen(IUIScreen screen, float duration)
		{
			float elapsedTime = 0;
			CanvasGroup canvasGroup = screen.GetCanvasGroup();
			while (elapsedTime < duration)
			{
				canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			canvasGroup.alpha = 0;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}
}