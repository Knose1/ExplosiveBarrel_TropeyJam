///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 18:54
///-----------------------------------------------------------------

using Com.Github.Knose1.Common.Common;
using Com.Github.Knose1.Common.UI;
using Com.Github.Knose1.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.ExploseEm.Game.View {
	public class GridView : MonoBehaviour {

		public static GridView Instance => Singleton.GetInstance<GridView>();

		public static event Action<int> OnSetBombs;

		[SerializeField] protected BetterGrid grid;

		[Header("Prefabs")]
		[SerializeField] protected GameObject prefabSteel;
		[SerializeField] protected GameObject prefabRock;
		[SerializeField] protected GameObject prefabSilicium;
		[SerializeField] protected GameObject prefabGridBomb;
		[SerializeField] protected GameObject prefabMyBomb;
		[SerializeField] protected ParticleSystem explosionParticlePrefab;
		[SerializeField] protected ParticleSystem explosionBombGridParticlePrefab;

		private bool hasClicked = false;
		private bool hadMoved = false;
		private bool isDoingAnimation = false;
		private bool isExploding = false;
		private List<Vector2> bombPosition = null;

		/// <summary>
		/// A list of next line waiting for animation
		/// </summary>
		private List<List<GameManager.TileType>> animationWaitList = new List<List<GameManager.TileType>>();

		private void Start()
		{
			Tile.OnPointerDownUnity += Tile_OnPointerDownUnity;
			Tile.OnPointerUpUnity += Tile_OnPointerUpUnity;
			Tile.OnPointerEnterUnity += Tile_OnPointerEnterUnity;

			GameManager gm = Singleton.GetInstance<GameManager>();
			gm.OnStart += GameManager_OnStart;
			gm.OnLineAdded += GameManager_OnLineAdded;
			gm.OnExplosion += GameManager_OnExplosion;
			gm.OnExplosionEnd += GameManager_OnExplosionEnd;
		}

		private void GameManager_OnStart(GameManager obj)
		{
			Matrix2D<GameManager.TileType> gmGrid = obj.Grid;
			int width = gmGrid.Width;
			int height = gmGrid.Height;
			
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					GameManager.TileType tileType = gmGrid[x, y];

					if (tileType == GameManager.TileType.None) continue;

					Tile tile = grid.GetTileAt(x, y).GetComponent<Tile>();
					SetContent(tile, tileType);

				}
			}
		}
		
		private void GameManager_OnLineAdded(GameManager obj, List<GameManager.TileType> listToAnimate)
		{

			animationWaitList.Add(listToAnimate);

			if (isDoingAnimation) return;

			void ExecuteAnimation()
			{
				GameManager.Instance.Controller.activateInput = false;
				isDoingAnimation = true;
				grid.MoveGridDown(OnComplete);
			}

			void OnComplete(List<GameObject> nextVisibleLine)
			{
				List<GameManager.TileType> list = animationWaitList[0];
				animationWaitList.RemoveAt(0);

				int length = nextVisibleLine.Count;
				for (int i = 0; i < length; i++)
				{
					SetContent(
						nextVisibleLine[i].GetComponent<Tile>(),
						list[i]
					);
				}

				if (animationWaitList.Count != 0) ExecuteAnimation();
				else
				{
					GameManager.Instance.Controller.activateInput = true;
					isDoingAnimation = false;
				}
			}

			ExecuteAnimation();
		}

		private void GameManager_OnExplosion(GameManager arg1, List<Vector2> arg2, Vector2 origin, bool isGrid)
		{
			for (int i = arg2.Count - 1; i >= 0; i--)
			{
				Vector2 pos = arg2[i];
				grid.GetTileAt((int)pos.x, (int)pos.y).GetComponent<Tile>().RemoveContent();
			}

			RectTransform rectTransform = Instantiate(isGrid ? explosionBombGridParticlePrefab : explosionParticlePrefab, grid.GetTileAt((int)origin.x, (int)origin.y).transform).gameObject.AddComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.sizeDelta = Vector2.zero;
			rectTransform.anchoredPosition3D = Vector3.zero - Vector3.forward * 2;
		}

		private void GameManager_OnExplosionEnd(GameManager obj)
		{
			isExploding = false;
		}

		private void Tile_OnPointerDownUnity(Tile tile, int x, int y)
		{
			if (hasClicked || isDoingAnimation || isExploding) return;
			if (y != 0 || GameManager.Instance.BombCount <= 0) return;

			if (bombPosition != null) FixBombNotDiapearing(bombPosition);

			hadMoved = false;

			bombPosition = new List<Vector2>();
			hasClicked = true;

			AddBombPosition(tile, x, y);
		}

		private void Tile_OnPointerUpUnity(Tile tile, int x, int y)
		{
			if (!hasClicked) return;

			if (bombPosition.Count <= 1 && hadMoved)
			{
				FixBombNotDiapearing(bombPosition);
				hasClicked = false;
				hadMoved = false;
				bombPosition = null;

				OnSetBombs?.Invoke(0);
				return;
			}

			hasClicked = false;
			isExploding = true;
			hadMoved = false;

			GameManager.Instance.Controller.SetBombs(bombPosition);
			bombPosition = null;
		}

		private void Tile_OnPointerEnterUnity(Tile tile, int x, int y)
		{
			if (!hasClicked || bombPosition == null || bombPosition.Count == 0) return;

			hadMoved = true;

			Vector2 pos = new Vector2(x, y);

			if (bombPosition.Contains(pos))
			{
				RemoveBombPosition(tile, x, y);
			}
			else if (GameManager.Instance.BombCount >= (bombPosition.Count + 1))
			{
				Vector2 distance = bombPosition[bombPosition.Count - 1] - pos;
				if (distance.sqrMagnitude == 1)
				{
					AddBombPosition(tile, x, y);
				}
				else if (distance.sqrMagnitude == 2)
				{
					if (GameManager.Instance.IsCellTaken(x, y)) return;

					int newX = (int)distance.x + x;
					int newY = (int)distance.y + y;
					if (AddBombPosition(grid.GetTileAt(newX, y).GetComponent<Tile>(), newX, y) || AddBombPosition(grid.GetTileAt(x, newY).GetComponent<Tile>(), x, (int)distance.y + y))
					{
						AddBombPosition(tile, x, y);
					}
				}
			}
		}

		private void SetContent(Tile tile, GameManager.TileType tileType)
		{
			switch (tileType)
			{
				case GameManager.TileType.Steel:
					tile.SetContent(Instantiate(prefabSteel));
					break;
				case GameManager.TileType.Silicium:
					tile.SetContent(Instantiate(prefabSilicium));
					break;
				case GameManager.TileType.Rock:
					tile.SetContent(Instantiate(prefabRock));
					break;
				case GameManager.TileType.Bomb:
					tile.SetContent(Instantiate(prefabGridBomb));
					break;
				default:
					tile.RemoveContent();
					break;
			}
		}

		private bool AddBombPosition(Tile tile, int x, int y)
		{
			if (bombPosition.Contains(new Vector2(x, y)) || GameManager.Instance.IsCellTaken(x, y)) return false;

			bombPosition.Add(new Vector2(x, y));
			tile.SetContent(Instantiate(prefabMyBomb));

			OnSetBombs?.Invoke(bombPosition.Count);

			return true;
		}

		private void RemoveBombPosition(Tile tile, int x, int y)
		{
			int index = bombPosition.IndexOf(new Vector2(x, y));
			int length = bombPosition.Count;

			for (int i = length - 1; i >= index + 1; i--)
			{
				Vector2 pos = bombPosition[i];
				bombPosition.RemoveAt(i);
				grid.GetTileAt((int)pos.x, (int)pos.y).GetComponent<Tile>().RemoveContent();
			}

			OnSetBombs?.Invoke(bombPosition.Count);
		}

		private void FixBombNotDiapearing(List<Vector2> bombPosition)
		{
			int length = bombPosition.Count;
			for (int i = 0; i < length; i++)
			{
				Vector2 pos = bombPosition[i];
				grid.GetTileAt((int)pos.x, (int)pos.y).GetComponent<Tile>().RemoveContent();
			}
		}

		private void OnDestroy()
		{
			GameManager gm = Singleton.GetInstance<GameManager>();
			if (gm)
			{
				gm.OnStart -= GameManager_OnStart;
				gm.OnLineAdded -= GameManager_OnLineAdded;
				gm.OnExplosion -= GameManager_OnExplosion;
				gm.OnExplosionEnd -= GameManager_OnExplosionEnd;
			}
			Tile.OnPointerDownUnity -= Tile_OnPointerDownUnity;
			Tile.OnPointerUpUnity -= Tile_OnPointerUpUnity;
			Tile.OnPointerEnterUnity -= Tile_OnPointerEnterUnity;
		}
	}
}