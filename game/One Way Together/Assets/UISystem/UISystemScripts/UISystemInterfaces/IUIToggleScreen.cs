namespace LB.UI.System
{
	public interface IUIToggleScreen
	{
		void OpenScreenAdditive(UIScreenConfig screenConfig);
		void OpenScreenExclusive(UIScreenConfig screenConfig);
		void CloseScreen(UIScreenConfig screenConfig);
		void CloseAllScreens();
		void CloseLastScreen();
	}
}