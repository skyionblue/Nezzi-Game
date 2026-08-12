using UnityEngine;
using UnityEngine.EventSystems;

namespace LB.UI.System
{
	/// <summary>
	/// A component that enables a UI element to be dragged within a defined area.
	/// This script requires the object to have a CanvasGroup component.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	public class DraggableUIComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		/// <summary>
		/// The area in which the UI component can be dragged. If null, dragging is unrestricted.
		/// </summary>
		[SerializeField] private RectTransform draggableArea;

		/// <summary>
		/// Reference to the RectTransform of the UI component.
		/// </summary>
		private RectTransform _uiRectTransform;

		/// <summary>
		/// The original position of the UI component, used for resetting its location.
		/// </summary>
		private Vector2 _originalPosition;

		/// <summary>
		/// Offset used to correctly track the dragging position.
		/// </summary>
		private Vector2 _dragOffset;

		/// <summary>
		/// Indicates whether the UI component is currently being dragged.
		/// </summary>
		private bool _isDragging = false;

		/// <summary>
		/// The CanvasGroup attached to this UI component.
		/// </summary>
		public CanvasGroup CanvasGroup { get; private set; }

		/// <summary>
		/// Initializes the component and stores the original position.
		/// </summary>
		private void Awake()
		{
			_uiRectTransform = GetComponent<RectTransform>();
			CanvasGroup = GetComponent<CanvasGroup>();
			_originalPosition = _uiRectTransform.anchoredPosition;
		}

		/// <summary>
		/// Called when the user begins dragging the UI component.
		/// </summary>
		/// <param name="eventData">Pointer event data for the drag.</param>
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (draggableArea != null && IsPointerWithinDraggableArea(eventData))
			{
				_isDragging = true;
				CanvasGroup.blocksRaycasts = false;

				RectTransformUtility.ScreenPointToLocalPointInRectangle(draggableArea, eventData.position,
					eventData.pressEventCamera, out _dragOffset);
				_dragOffset = (Vector2)transform.position - eventData.position;
			}
		}

		/// <summary>
		/// Called when the user is dragging the UI component.
		/// </summary>
		/// <param name="eventData">Pointer event data for the drag.</param>
		public void OnDrag(PointerEventData eventData)
		{
			if (_isDragging)
			{
				Vector2 screenPosition = eventData.position + _dragOffset;
				transform.position = screenPosition;
			}
		}

		/// <summary>
		/// Called when the user stops dragging the UI component.
		/// </summary>
		/// <param name="eventData">Pointer event data for the drag.</param>
		public void OnEndDrag(PointerEventData eventData)
		{
			if (_isDragging)
			{
				_isDragging = false;
				CanvasGroup.blocksRaycasts = true;
			}
		}

		/// <summary>
		/// Checks if the pointer is within the defined draggable area.
		/// </summary>
		/// <param name="eventData">Pointer event data for the check.</param>
		/// <returns>True if the pointer is within the draggable area, otherwise false.</returns>
		private bool IsPointerWithinDraggableArea(PointerEventData eventData)
		{
			if (draggableArea == null) return false;

			return RectTransformUtility.RectangleContainsScreenPoint(draggableArea, eventData.position,
				eventData.pressEventCamera);
		}

		/// <summary>
		/// Resets the UI component to its original position.
		/// </summary>
		public void ResetToOriginalPosition()
		{
			_uiRectTransform.anchoredPosition = _originalPosition;
		}
	}
}