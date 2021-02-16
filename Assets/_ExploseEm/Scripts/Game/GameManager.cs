///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 23:49
///-----------------------------------------------------------------

using Com.Github.Knose1.Common.Common;
using Com.Github.Knose1.Common.Utils;
using Com.Github.Knose1.ExploseEm.Game.View;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.ExploseEm.Game {
	public class GameManager : MonoBehaviour {

		/**************/
		/*   STATIC   */
		/**************/
		public enum TileType
		{
			None,
			Steel,
			Silicium,
			Rock,
			Bomb,
		}

		protected static int enumMaxValue = 3; //set to 3 to avoid bomb spawning

		public static GameManager Instance => Singleton.GetInstance<GameManager>();

		/**************/
		/*   Events   */
		/**************/
		public event Action<GameManager> OnGameOver;
		public event Action<GameManager> OnStart;
		public event Action<GameManager, List<TileType>> OnLineAdded;
		public event Action<GameManager, List<Vector2>, Vector2, bool> OnExplosion;
		public event Action<GameManager> OnExplosionEnd;
		public event Action<GameManager> OnCraftEnd;

		/******************/
		/*   Serialized   */
		/******************/
		[SerializeField] protected int gridHeight = 11;
		[SerializeField] protected int gridWidth = 5;
		[SerializeField] protected float explosionTime = 0.5f;
		[SerializeField] protected float timeAfterAddLines = 0.5f;

		[Header("Ressources")]
		[SerializeField] protected int startBombCount = 20;
		[SerializeField] protected AnimationCurve ressourceGainOverTime = AnimationCurve.EaseInOut(0,2,20,0.5f);

		[SerializeField] private int _requiredRock = 2;
		public int RequiredRock => _requiredRock;

		[SerializeField] private int _requiredSilicium = 2;
		public int RequiredSilicium => _requiredSilicium;

		[SerializeField] private int _requiredSteel = 2;
		public int RequiredSteel => _requiredSteel;

		[Header("Random")]
		[SerializeField] protected AnimationCurve guarantedDropOverLines = null;
		[SerializeField] protected AnimationCurve extradropChance = AnimationCurve.Linear(0, 0.25f, 10, 0.1f);
		[SerializeField] protected AnimationCurve bombDropChance = AnimationCurve.Linear(0, 0.1f, 10, 0.15f);
		[SerializeField] protected int maxBombInRandomList;

		/**************/
		/*   Fields   */
		/**************/
		private Controller _controller = null;
		public Controller Controller => _controller;

		private Matrix2D<TileType> _grid = null;
		public Matrix2D<TileType> Grid => _grid;

		private List<TileType> listOfNextTileTypeDrop = new List<TileType>();

		//Number of line the player cleared
		private int _linesDrop = 0;
		public int LineDrop => _linesDrop;

		private int _bombCount = 0;
		public int BombCount => _bombCount;

		private int _steelCount = 0;
		public int SteelCount => _steelCount;

		private int _siliciumCount = 0;
		public int SiliciumCount => _siliciumCount;

		private int _rockCount = 0;
		public int RockCount => _rockCount;

		protected void Start()
		{
			StartGame();
		}

		protected void StartGame()
		{
			_linesDrop = 0;
			_bombCount = startBombCount;
			_steelCount = 0;
			_siliciumCount = 0;
			_rockCount = 0;


			_controller = new Controller();
			_grid = new Matrix2D<TileType>();

			_grid.autoUpdateWidth = false;
			_grid.Width = gridWidth;
			_grid.Height = gridHeight;

			for (int y = 0; y < gridHeight; y++)
			{
				CreateLine(y);
			}

			_controller.OnBombsSet += Controller_OnBombsSet;

			OnStart?.Invoke(this);

			OnCraftEnd += GameManager_OnCraftEnd;
		}

		private void GameManager_OnCraftEnd(GameManager obj) 
		{
			if (obj._bombCount <= 0)
				obj.GameOver();
		}

		private void Controller_OnBombsSet()
		{
			StartCoroutine(coroutine());

			IEnumerator coroutine()
			{
				List<Vector2> bombList = Controller.BombList;
				int length = bombList.Count;
				_bombCount -= length;

				Gain gain = new Gain();


				List<Vector2> additiveBombList = new List<Vector2>();

				for (int i = 0; i < length; i++)
				{
					ExploseGridBombs();

					DoExplosion(additiveBombList, bombList[i], ref gain, false);

					yield return new WaitForSecondsRealtime(explosionTime);
				}

				void ExploseGridBombs()
				{
					List<Vector2> additiveBombList2 = new List<Vector2>();
					if (additiveBombList.Count > 0)
					{
						foreach (Vector2 pos in additiveBombList)
						{
							DoExplosion(additiveBombList2, pos, ref gain, true);
						}

						additiveBombList = additiveBombList2;
					}
				}

				while (additiveBombList.Count > 0)
				{
					ExploseGridBombs();
					yield return new WaitForSeconds(explosionTime);
				}

				Debug.Log("[GRID BEFORE]\n" + _grid.ToString());

				yield return new WaitForSeconds(timeAfterAddLines);

				int oldLinesDrop = _linesDrop;

				do
				{
					List<TileType> line = _grid[0];
					bool isLineEmpty = line.FindAll(findAllNullTile).Count >= gridWidth;

					if (!isLineEmpty) break;

					_grid.RemoveLine(0);
					_grid.Height = gridHeight;

					_linesDrop += 1;

					List<TileType> createdLine = CreateLine(gridHeight - 1);

					Debug.Log("[GRID]\n" + _grid.ToString());

					OnLineAdded?.Invoke(this, createdLine);
				}
				while (true);

				Debug.Log("[GRID DONE]\n" + _grid.ToString());

				_rockCount += gain.CalculateTotalGainOverTime(ressourceGainOverTime, oldLinesDrop, gain.rockDestroyed);
				_siliciumCount += gain.CalculateTotalGainOverTime(ressourceGainOverTime, oldLinesDrop, gain.siliciumDestroyed);
				_steelCount += gain.CalculateTotalGainOverTime(ressourceGainOverTime, oldLinesDrop, gain.steelDestroyed);

				OnExplosionEnd?.Invoke(this);

				int canCraft = Mathf.Min(_rockCount / _requiredRock, _siliciumCount / _requiredSilicium, _steelCount / _requiredSteel);
				_bombCount += canCraft;

				_rockCount -= canCraft * _requiredRock;
				_siliciumCount -= canCraft * _requiredSilicium;
				_steelCount -= canCraft * _requiredSteel;

				OnCraftEnd?.Invoke(this);

				bool findAllNullTile(TileType item) 
				{
					return item == TileType.None;
				};

			}
		}

		private void DoExplosion(List<Vector2> additiveBombList, Vector2 origin, ref Gain gain, bool isGrid)
		{
			List<Vector2> explosed = new List<Vector2>
			{
				origin
			};

			for (int x = -1; x <= 1; x += 2)
			{
				Vector2 pos = new Vector2(x, 0) + origin;

				if (Explode(additiveBombList, (int)pos.x, (int)pos.y, ref gain))
				{
					explosed.Add(pos);
				}
			}

			for (int y = -1; y <= 1; y += 2)
			{
				Vector2 pos = new Vector2(0, y) + origin;

				if (Explode(additiveBombList, (int)pos.x, (int)pos.y, ref gain))
				{
					explosed.Add(pos);
				}
			}

			OnExplosion?.Invoke(this, explosed, origin, isGrid);
		}

		private bool Explode(List<Vector2> additiveBombList, int x, int y, ref Gain gain)
		{
			if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;

			TileType tile = _grid[x, y];

			switch (tile)
			{
				case TileType.None:
					return false;
				case TileType.Steel:
					gain.steelDestroyed += 1;
					break;
				case TileType.Silicium:
					gain.siliciumDestroyed += 1;
					break;
				case TileType.Rock:
					gain.rockDestroyed += 1;
					break;
				case TileType.Bomb:
					gain.totalDestroyed += 1;
					additiveBombList.Add(new Vector2(x, y));
					_grid[x, y] = TileType.None;
					return false;
			}

			gain.totalDestroyed += 1;
			_grid[x, y] = TileType.None;

			return true;
		}

		private void GameOver()
		{
			OnGameOver?.Invoke(this);
			EndGame();
		}

		private void EndGame()
		{
			OnCraftEnd -= GameManager_OnCraftEnd;
			_controller.OnBombsSet -= Controller_OnBombsSet;
			_controller.Destroy();
			StopAllCoroutines();

			OnGameOver = null;
			OnStart = null;
			OnLineAdded = null;
			OnExplosion = null;
			OnExplosionEnd = null;
			OnCraftEnd = null;
		}

		public bool IsCellTaken(int x, int y)
		{
			return _grid[x, y] != TileType.None;
		}

		private List<TileType> CreateLine(int y)
		{
			List<TileType> listToPush = new List<TileType>();
			int guaranted = Mathf.FloorToInt(guarantedDropOverLines.Evaluate(y));

			int currentSuccessfullRandom = 0;

			for (int x = 0; x < guaranted; x++)
			{
				listToPush.Add(GetGuarantedRandom(ref currentSuccessfullRandom, y + _linesDrop));
			}

			while (listToPush.Count < gridWidth)
			{
				listToPush.Add(GetRandom(ref currentSuccessfullRandom, y + _linesDrop));
			}

			listToPush.Sort(RandomArray);

			for (int x = 0; x < gridWidth; x++)
			{
				_grid[x, y] = listToPush[x];
			}

			return listToPush;
		}

		private TileType GetGuarantedRandom(ref int currentSuccessfullRandom, int y)
		{
			if (currentSuccessfullRandom == 4) return TileType.None;
			currentSuccessfullRandom += 1;

			SetupTileTypeDrops(y);

			TileType tileType = listOfNextTileTypeDrop[0];
			listOfNextTileTypeDrop.RemoveAt(0);

			return tileType;
		}

		private TileType GetRandom(ref int currentSuccessfullRandom, int y)
		{
			if (currentSuccessfullRandom == 4) return TileType.None;
			SetupTileTypeDrops(y);

			if (UnityEngine.Random.value > extradropChance.Evaluate(y)) return TileType.None;

			TileType tileType = listOfNextTileTypeDrop[0];
			currentSuccessfullRandom += 1;
			listOfNextTileTypeDrop.RemoveAt(0);

			return tileType;
		}

		private void SetupTileTypeDrops(int y)
		{
			if (listOfNextTileTypeDrop.Count != 0) return;
			
			for (int i = 0; i <= enumMaxValue; i++)
			{
				listOfNextTileTypeDrop.Add((TileType)i);
			}

			//Double tour
			for (int i = 0; i <= enumMaxValue; i++)
			{
				listOfNextTileTypeDrop.Add((TileType)i);
			}

			for (int i = 0; i < maxBombInRandomList; i++)
			{
				if (UnityEngine.Random.value <= bombDropChance.Evaluate(y))
					listOfNextTileTypeDrop.Add(TileType.Bomb);
			}
			

			listOfNextTileTypeDrop.Sort(RandomArray);
		}

		private int RandomArray(TileType x, TileType y)
		{
			return UnityEngine.Random.Range(-1, 2);
		}

		private void OnDestroy()
		{
			EndGame();
			Singleton.DestroyInstance(this);
		}

		protected struct Gain
		{
			public int totalDestroyed;
			public int steelDestroyed;
			public int siliciumDestroyed;
			public int rockDestroyed;

			public Gain(int explosionCount, int steelDestroyed, int siliciumDestroyed, int rockDestroyed)
			{
				this.totalDestroyed = explosionCount;
				this.steelDestroyed = steelDestroyed;
				this.siliciumDestroyed = siliciumDestroyed;
				this.rockDestroyed = rockDestroyed;
			}

			public int CalculateTotalGain(int materialByDestroy, int material)
			{
				return totalDestroyed * materialByDestroy * material;
			}

			public int CalculateTotalGainOverTime(AnimationCurve materialOverTime, int time, int material)
			{
				return Mathf.RoundToInt(Mathf.Max(1, totalDestroyed * materialOverTime.Evaluate(time)) * material);
			}

			public int CalculateTotalGain(AnimationCurve materialByDestroy, int material)
			{
				return Mathf.RoundToInt(materialByDestroy.Evaluate(totalDestroyed) * material);
			}
		}
	}
}