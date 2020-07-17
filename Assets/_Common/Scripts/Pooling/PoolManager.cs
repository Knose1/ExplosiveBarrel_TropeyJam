///-----------------------------------------------------------------
/// Author : Knose1
///-----------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Common.Pooling {
	[AddComponentMenu("Common/Pooling/"+nameof(PoolManager))]
	public partial class PoolManager : MonoBehaviour {
		private static PoolManager _instance;
		public static PoolManager Instance => _instance ? _instance : (_instance = FindObjectOfType<PoolManager>());

		[SerializeField] protected List<PoolConfig> configs;
		protected List<Pool> pools = new List<Pool>();


		private void Awake()
		{
			for (int i = configs.Count - 1; i >= 0; i--)
			{
				pools.Add(new Pool(configs[i]));
			}

			FillPools();
			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// Fill the pools
		/// </summary>
		public void FillPools()
		{
			for (int i = pools.Count - 1; i >= 0; i--)
			{
				Pool item = pools[i];

				PoolConfig prefabConfig = item.config;

				GameObject prefab = prefabConfig.prefab;
				for (int k = (prefabConfig.maxItem - (item.ActiveItems.Count + item.InactiveItems.Count)) - 1; k >= 0; k--)
				{
					GameObject obj = Instantiate(prefab);
					obj.transform.SetParent(transform);
					obj.SetActive(false);

					item.Enqueue(obj);
				}
			}

			Debug.Log(Pool.LOG_PREFIX + " " + "Filled the pools");
		}

		/// <summary>
		/// Make all the game objects go back to the pool
		/// </summary>
		public void DestroyAll()
		{
			for (int i = pools.Count - 1; i >= 0; i--)
			{
				Pool item = pools[i];
				
				for (int k = item.ActiveItems.Count - 1; k >= 0; k--)
				{
					GameObject obj = item.ActiveItems[k];

					item.SetInactive(k, out Action next);
					DestroyAsynCoroutine(obj, next);
				}
			}

			Debug.Log(Pool.LOG_PREFIX + " " + "Desactivated all the items outside the pools");
		}

		/// <summary>
		/// Destroy and unparent all the gameobject in the pool
		/// </summary>
		public void ForceDestroyAll()
		{
			for (int i = pools.Count - 1; i >= 0; i--)
			{
				Pool item = pools[i];

				for (int k = item.ActiveItems.Count - 1; k >= 0; k--)
				{
					GameObject obj = item.ActiveItems[k];
					item.ActiveItems.Remove(obj);
					
					Destroy(obj);
				}

				for (int k = item.InactiveItems.Count - 1; k >= 0; k--)
				{
					GameObject obj = item.Dequeue();

					UnityEngine.Object.Destroy(obj);
				}

			}
			Debug.Log(Pool.LOG_PREFIX + " " + "Destroyed all the items");
		}

		/// <summary>
		/// Instantiate a prefab on the scene
		/// </summary>
		/// <param name="item">
		///		The item to instantiate
		/// </param>
		public GameObject Instantiate(PoolConfig item, bool setparent = true)
		{
			if (!pools.Contains(item))
			{
				Debug.LogError($"Item {item.name} is not in the list "+nameof(pools));
				return null;
			}

			Pool pool = pools[pools.IndexOf(item)];

			GameObject obj = pool.SetActive(out Action next);
			if (obj)
			{
				if (setparent) obj.transform.SetParent(transform);
				DispatchPoolStartEvent(obj, next);
			}

			return obj;
		}

		public void Destroy(GameObject obj, bool setparent = true)
		{
			for (int i = pools.Count - 1; i >= 0; i--)
			{
				Pool pool = pools[i];

				if (pool.ContainsInactive(obj))
				{
					Debug.LogError(Pool.LOG_PREFIX + " " + $"{obj.name} is already destroyed");
					return;
				}
				if (!pool.ContainsActive(obj)) continue;

				if (setparent) obj.transform.SetParent(transform);
				pool.SetInactive(obj, out Action next);
				DestroyAsync(obj, next);
				return;
			}

			Debug.LogError(Pool.LOG_PREFIX + " " + $"Can't destroy {obj.name}");
		}

		protected void DispatchPoolStartEvent(GameObject obj, Action setActiveCallback) => StartCoroutine(DispatchPoolStartEventCoroutine(obj, setActiveCallback));
		protected IEnumerator DispatchPoolStartEventCoroutine(GameObject obj, Action setActiveCallback)
		{
			yield return new WaitForEndOfFrame();
			setActiveCallback();

			//Dispatch Start Event
			IPoolBehaviourStart[] behaviours = obj.GetComponentsInChildren<IPoolBehaviourStart>(true);
			for (int i = behaviours.Length - 1; i >= 0; i--)
			{
				behaviours[i].PoolStart();
			}

			//Dispatch Late Start Event
			IPoolBehaviourLateStart[] behaviours2 = obj.GetComponentsInChildren<IPoolBehaviourLateStart>(true);
			for (int i = behaviours2.Length - 1; i >= 0; i--)
			{
				behaviours2[i].LatePoolStart();
			}
		}

		protected void DestroyAsync(GameObject obj, Action setInactiveCallback) => StartCoroutine(DestroyAsynCoroutine(obj, setInactiveCallback));
		protected IEnumerator DestroyAsynCoroutine(GameObject obj, Action setInactiveCallback)
		{
			yield return new WaitForEndOfFrame();
			setInactiveCallback();

			//Dispatch Destroy Event
			IPoolBehaviourDestroy[] behaviours = obj.GetComponentsInChildren<IPoolBehaviourDestroy>(false);
			for (int i = behaviours.Length - 1; i >= 0; i--)
			{
				behaviours[i].PoolDestroy();
			}
		}
	}
}