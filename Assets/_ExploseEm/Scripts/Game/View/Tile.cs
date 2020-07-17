///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 21:00
///-----------------------------------------------------------------

using Com.Github.Knose1.Common.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com.Github.Knose1.ExploseEm.Game.View {
	public class Tile : MonoBehaviour, IBetterGridElement, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public delegate void TileDelegate(Tile tile, int x, int y);

		public static event TileDelegate OnAddedToGrid;
		public static event TileDelegate OnRemovedFromGrid;
		public static event TileDelegate OnPointerDownUnity;
		public static event TileDelegate OnPointerUpUnity;
		public static event TileDelegate OnPointerEnterUnity;
		public static event TileDelegate OnPointerExitUnity;

		[SerializeField] protected GameObject contentContainer;
		
		protected GameObject content;
		private int _x;
		private int _y;

		public int X => _x;
		public int Y => _y;


		public void OnMoved(int x, int y)
		{
			_x = x;
			_y = y;
		}

		public void AddedToGrid(int x, int y)
		{
			_x = x;
			_y = y;
			OnAddedToGrid?.Invoke(this, x, y);
		}

		public void RemovedFromGrid(int x, int y)
		{
			_x = x;
			_y = y;
			OnRemovedFromGrid?.Invoke(this, x, y);

			RemoveContent();
		}

		public void SetContent(GameObject content)
		{
			RemoveContent();

			this.content = content;

			content.transform.SetParent(contentContainer.transform);
			content.transform.localScale = Vector3.one;

			RectTransform contentRectTransform = (content.transform as RectTransform);
			contentRectTransform.sizeDelta = Vector2.zero;
			contentRectTransform.anchoredPosition3D = Vector3.zero;
		}
		public void RemoveContent()
		{
			if (content)
			{
				Debug.Log("[Tile] Destroyed content");
				Destroy(content);
			}
			content = null;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			OnPointerDownUnity?.Invoke(this, X, Y);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			OnPointerUpUnity?.Invoke(this, X, Y);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			OnPointerEnterUnity?.Invoke(this, X, Y);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			OnPointerExitUnity?.Invoke(this, X, Y);
		}
	}
}