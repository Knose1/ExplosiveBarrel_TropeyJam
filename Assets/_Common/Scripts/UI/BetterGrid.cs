///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 12:19
///-----------------------------------------------------------------

using UnityEngine;
using Com.Github.Knose1.Common.Attributes;
using Com.Github.Knose1.Common.Attributes.Abstract;
using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using System;

namespace Com.Github.Knose1.Common.UI {
	[RequireComponent(typeof(RectTransform))]
	[ExecuteAlways]
	public class BetterGrid : MonoBehaviour
	{
		protected const string BETTER_GRID_DEBUG_TAG = "["+nameof(BetterGrid)+"]";

		[SerializeField] int childByLine = 10;
		[SerializeField] int maxVisibleCol = 10;
		[SerializeField, Range(0,1)] float padding = 0;

		[Header("Twinning")]
		[SerializeField] float moveTime = 0;
		[SerializeField] AnimationCurve moveCurve = AnimationCurve.Linear(0,0,1,1);

		protected RectTransform rectTransform;

		private bool isMoving = false;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		protected void Start()
		{
			if (!Application.isPlaying) return;

			GetColCount(out int length, out int colCount);

			for (int i = 0; i < length; i++)
			{
				Transform child = transform.GetChild(i);

				GetPosFromIndex(i, out int posX, out int posY);

				bool canBeActive = posY < (colCount + 1);

				if (!canBeActive)
				{
					DispatchRemovedEvent(child.GetComponentsInChildren<IBetterGridElement>(), posX, posY);
					child.gameObject.SetActive(false);
				}

				DispatchAddedEvent(child.GetComponentsInChildren<IBetterGridElement>(), posX, posY);
				
			}
		}

		private void Update()
		{
			if (!rectTransform) rectTransform = transform as RectTransform;

			float cellSize = 1 - padding;


			GetColCount(out int length, out int colCount);

			for (int i = 0; i < length; i++)
			{
				RectTransform child = transform.GetChild(i) as RectTransform;
				
				GetPosFromIndex(i, out int posX, out int posY);

				bool canBeActive = posY < (colCount + 1);

				child.sizeDelta = new Vector2(0, 0);
				child.anchoredPosition = new Vector2(0, 0);

				child.anchorMin = new Vector2(cellSize / childByLine * (posX)    , cellSize / colCount * (posY)    );
				child.anchorMax = new Vector2(cellSize / childByLine * (posX + 1), cellSize / colCount * (posY + 1));

				child.pivot = new Vector2(0.5f, 0);
			}
		}

		/// <param name="nextVisibleLine">The line that will be visible at the next call of MoveGridDown</param>
		public delegate void MoveComplete(List<GameObject> nextVisibleLine);

		/// <summary>
		/// Move all the gameobjects down
		/// The bottomLine is moved to the top and disabled
		/// </summary>
		public void MoveGridDown(MoveComplete onComplete)
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning(BETTER_GRID_DEBUG_TAG+" Can't Move the Grid in editor");
				return;
			}

			if (isMoving)
			{
				Debug.LogWarning(BETTER_GRID_DEBUG_TAG+" Wait for the Grid to finish moving before calling " +nameof(MoveGridDown));
				return;
			}

			isMoving = true;

			GetUnclampedColCount(out int length, out int colCount);
			List<GameObject> nextVisibleLine = new List<GameObject>();

			//maxVisibleCol + 1 will be visible so maxVisibleCol + 2 the nextVisibleLine
			int startIndex = Mathf.Min(colCount, maxVisibleCol + 2);

			//If there is no maxVisibleCol + 2, then the raw 0 will be the nextVisibleLine
			if (startIndex == maxVisibleCol + 1) startIndex = 0;

			for (int i = 0; i < childByLine; i++)
			{
				nextVisibleLine.Add(transform.GetChild(i + startIndex).gameObject);
			}

			int colCountClamped = Mathf.Min(colCount, maxVisibleCol);

			Vector2 endPos = new Vector2(0, -rectTransform.rect.height / colCountClamped);

			TweenerCore<Vector2, Vector2, DG.Tweening.Plugins.Options.VectorOptions> twin = rectTransform.DOAnchorPos(endPos, moveTime);

			twin.onComplete = twinComplete;
			twin.SetEase(moveCurve);

			void twinComplete()
			{
				isMoving = false;

				rectTransform.anchoredPosition = Vector2.zero;
				
				for (int i = 0; i < childByLine; i++)
				{
					Transform child = transform.GetChild(0);

					child.SetAsLastSibling();
					DispatchRemovedEvent(child.GetComponentsInChildren<IBetterGridElement>(), i, colCount);
					child.gameObject.SetActive(false);
				}

				for (int i = 0; i < childByLine; i++)
				{
					GameObject child = nextVisibleLine[i];

					child.gameObject.SetActive(true);
					DispatchAddedEvent(child.GetComponentsInChildren<IBetterGridElement>(), i, maxVisibleCol);
				}

				for (int i = 0; i < transform.childCount; i++)
				{
					GameObject child = transform.GetChild(i).gameObject;

					GetPosFromIndex(i, out int x, out int y);
					DispatchMovedEvent(child.GetComponentsInChildren<IBetterGridElement>(), x, y);
				}


				onComplete?.Invoke(nextVisibleLine);
			}
		}

		protected void DispatchMovedEvent(IBetterGridElement[] betterGridElement, int x, int y)
		{
			int length = betterGridElement.Length;
			for (int i = 0; i < length; i++)
			{
				betterGridElement[i].OnMoved(x, y);
			}
		}

		protected void DispatchRemovedEvent(IBetterGridElement[] betterGridElement, int x, int y)
		{
			int length = betterGridElement.Length;
			for (int i = 0; i < length; i++)
			{
				betterGridElement[i].RemovedFromGrid(x, y);
			}
		}

		protected void DispatchAddedEvent(IBetterGridElement[] betterGridElement, int x, int y)
		{
			int length = betterGridElement.Length;
			for (int i = 0; i < length; i++)
			{
				betterGridElement[i].AddedToGrid(x, y);
			}
		}

		public void GetUnclampedColCount(out int length, out int colCount)
		{
			length = transform.childCount;
			colCount = (length - length % childByLine) / childByLine + (length % childByLine == 0 ? 0 : 1);
		}

		/// <summary>
		/// Get the clamped number of column
		/// </summary>
		/// <param name="length">The number of child</param>
		/// <param name="colCount">The number of column</param>
		public void GetColCount(out int length, out int colCount)
		{
			GetUnclampedColCount(out length, out colCount);
			colCount = Mathf.Min(colCount, maxVisibleCol);
		}

		/// <summary>
		/// Get a tile at a certain position of the grid
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public GameObject GetTileAt(int x, int y)
		{
			GetUnclampedColCount(out int length, out int colCount);

			if (x < 0 || y < 0 || x >= childByLine || y >= colCount)
			{
				Debug.LogWarning(BETTER_GRID_DEBUG_TAG+" You are trying to get a child out of bounds");
				return null;
			}

			int index = y * childByLine + x;

			return transform.GetChild(index).gameObject;
		}
		private void GetPosFromIndex(int index, out int posX, out int posY)
		{
			posX = index % childByLine;
			posY = (index - posX) / childByLine;
		}
	}
}