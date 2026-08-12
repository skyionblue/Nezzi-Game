using System;
using System.Collections.Generic;

namespace LB.UI.System
{
	/// <summary>
	/// A static class that manages the event bus for the UI system, allowing for
	/// subscribing, unsubscribing, and publishing events in a decoupled manner.
	/// </summary>
	public static class UISystemEventBus
	{
		/// <summary>
		/// A dictionary that maps event types to their respective delegates (listeners).
		/// Each key is a type of event (T), and the value is the corresponding delegate (listeners).
		/// </summary>
		private static Dictionary<Type, Delegate> eventTable = new Dictionary<Type, Delegate>();

		/// <summary>
		/// Subscribes a listener to an event of type T.
		/// </summary>
		/// <typeparam name="T">The type of event to subscribe to.</typeparam>
		/// <param name="listener">The method to be called when the event is raised.</param>
		public static void Subscribe<T>(Action<T> listener) where T : class
		{
			if (eventTable.TryGetValue(typeof(T), out Delegate existingDelegate))
			{
				eventTable[typeof(T)] = existingDelegate as Action<T> + listener;
			}
			else
			{
				eventTable[typeof(T)] = listener;
			}
		}

		/// <summary>
		/// Unsubscribes a listener from an event of type T.
		/// </summary>
		/// <typeparam name="T">The type of event to unsubscribe from.</typeparam>
		/// <param name="listener">The method that was previously subscribed.</param>
		public static void Unsubscribe<T>(Action<T> listener) where T : class
		{
			if (eventTable.TryGetValue(typeof(T), out Delegate existingDelegate))
			{
				Action<T> newDelegate = existingDelegate as Action<T> - listener;

				if (newDelegate == null)
				{
					eventTable.Remove(typeof(T));
				}
				else
				{
					eventTable[typeof(T)] = newDelegate;
				}
			}
		}

		/// <summary>
		/// Publishes an event of type T to all subscribed listeners.
		/// </summary>
		/// <typeparam name="T">The type of event to publish.</typeparam>
		/// <param name="eventMessage">The event data to be passed to the listeners.</param>
		public static void Publish<T>(T eventMessage) where T : class
		{
			if (eventTable.TryGetValue(typeof(T), out Delegate existingDelegate))
			{
				(existingDelegate as Action<T>)?.Invoke(eventMessage);
			}
		}

		/// <summary>
		/// Clears all event subscriptions. This is useful for cleanup, especially in situations
		/// where you want to reset the event system.
		/// </summary>
		public static void ClearAll()
		{
			eventTable.Clear();
		}
	}
}