namespace LB.UI.System
{
	/// <summary>
	/// Represents an entry in the history of opened UI screens.
	/// Used to keep track of previous screens for navigation purposes.
	/// </summary>
	public class UIScreenHistoryEntry
	{        /// <summary>
		/// The screen associated with this history entry.
		/// </summary>

		public IUIScreen Screen { get; private set; }
		/// <summary>
		/// Indicates whether this screen was opened exclusively.
		/// </summary>

		public bool IsExclusive { get; private set; }
		/// <summary>
		/// Initializes a new instance of the UIScreenHistoryEntry class.
		/// </summary>
		/// <param name="screen">The screen being tracked in history.</param>
		/// <param name="isExclusive">Indicates whether the screen was opened exclusively.</param>

		public UIScreenHistoryEntry(IUIScreen screen, bool isExclusive)
		{
			Screen = screen;
			IsExclusive = isExclusive;
		}
	}
}