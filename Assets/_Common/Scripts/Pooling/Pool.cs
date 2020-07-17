///-----------------------------------------------------------------
/// Author : Knose1
///-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Common.Pooling {
	public class Pool
	{
		public const string LOG_PREFIX = "["+nameof(Pool)+"]";

		public PoolConfig config = null;

		/// <summary>
		/// Items inside the pool
		/// </summary>
		public Queue<GameObject> InactiveItems => _inactiveItems;
		private Queue<GameObject> _inactiveItems = new Queue<GameObject>();

		/// <summary>
		/// Items outside the pool
		/// </summary>
		public List<GameObject> ActiveItems => _activeItems;
		private List<GameObject> _activeItems = new List<GameObject>();


		/// <summary>
		/// Constructor of the class
		/// </summary>
		/// <param name="config"></param>
		public Pool(PoolConfig config)
		{
			this.config = config;
		}


		public GameObject Dequeue() => InactiveItems.Dequeue();
		public void Enqueue(GameObject item) => InactiveItems.Enqueue(item);

		public bool ContainsActive(GameObject gameObject) => ActiveItems.Contains(gameObject);
		public bool ContainsInactive(GameObject gameObject) => InactiveItems.Contains(gameObject);

		/// <summary>
		/// Prefer using <see cref="SetInactive(int)"/> for a little bit more optimisation
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="next">An action for async destroy</param>
		/// <returns></returns>
		public GameObject SetInactive(GameObject gameObject, out Action next)
		{
			if (!ContainsActive(gameObject))
			{
				Debug.LogError(LOG_PREFIX +" "+$"GameObject \"{gameObject.name}\" is not in the list " + nameof(ActiveItems));
				next = null;
				return null;
			}

			return SetInactive(ActiveItems.IndexOf(gameObject), out next);
		}
		public GameObject SetInactive(int index, out Action next)
		{
			if (index >= ActiveItems.Count)
			{
				Debug.LogError($"Given index is out of bounds of the array \"" + nameof(ActiveItems) + "\"");
				next = null;
				return null;
			}

			GameObject gameObject = ActiveItems[index];

			ActiveItems.RemoveAt(index);

			next = () =>
			{
				gameObject.SetActive(false);
				InactiveItems.Enqueue(gameObject);
			};

			return gameObject;
		}

		public GameObject SetActive(out Action next)
		{
			GameObject obj = null;

			if (InactiveItems.Count == 0)
			{
				GameObject prefab = config.prefab;
				obj = UnityEngine.Object.Instantiate(prefab);

				Debug.LogWarning($"Instantiated a new {config.name}, you shall to increase \"" + nameof(config.maxItem) + "\"");
			}
			else
			{
				obj = Dequeue();
			}

			next = () => 
			{
				obj.SetActive(true);
				ActiveItems.Add(obj);
			};


			return obj;
		}

		/*/////////////////////////////////////////////////*/
		/*                                                 */
		/*      Operators, Equals() and GetHashCode()      */
		/*                                                 */
		/*/////////////////////////////////////////////////*/

		static public implicit operator Pool(PoolConfig poolItemPrefab) => new Pool(poolItemPrefab);
		static public bool operator ==(Pool a, Pool b) => a.config == b.config;

		static public bool operator !=(Pool a, Pool b) => a.config != b.config;

		public override bool Equals(object obj) => obj is Pool && config == (obj as Pool).config;
		public override int GetHashCode() => -472031493 + EqualityComparer<PoolConfig>.Default.GetHashCode(config);
	}
}