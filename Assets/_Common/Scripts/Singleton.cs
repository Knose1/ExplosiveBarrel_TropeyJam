using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Common.Common
{
	internal class SingletonBehaviour : MonoBehaviour
	{
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);	
		}

		public void WaitForEndOfFrame(System.Action then)
		{
			IEnumerator lFunc()
			{
				yield return new WaitForEndOfFrame();
				then();
			}
			StartCoroutine(lFunc());
		}
	}

	public static class Singleton
	{
		private static SingletonBehaviour singletonBehaviour;

		private const string LOG_SET = "[Singleton] instance of {0} successfuly set.";
		private const string LOG_DESTROY = "[Singleton] instance of {0} successfuly destroyed.";
		private const string LOG_EXISTS = "[Singleton] The instance of {0} already exists";
		private const string LOG_FIND_NEXT_INSTANCE = "[Singleton] Finding a suitable instance for {0} ...";

		/// <summary>
		/// Dictionary to register the instance of each class
		/// </summary>
		private static Dictionary<System.Type, Object> Instances = new Dictionary<System.Type, Object>();

		public static void Init()
		{
			if (!singletonBehaviour)
				singletonBehaviour = new GameObject("Singleton").AddComponent<SingletonBehaviour>();
		}
		
		/// <summary>
		/// Get the instance of a specific class
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static T GetInstance<T>() where T : Object
		{
			Init();
			T instance = GetValueInDictionary<T>();

			if (!instance)
			{
				Debug.Log(string.Format(LOG_FIND_NEXT_INSTANCE, typeof(T).Name));
				instance = Object.FindObjectOfType<T>();
				instance?.SetInstance(true);
			}
			return instance;
		}

		

		/// <summary>
		/// Set the current instance to a value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance">The instance</param>
		/// <param name="force">If true : Will not check if the instance is already registered</param>
		public static void SetInstance<T>(this T instance, bool force = false) where T : Object
		{
			Init();
			System.Type type = typeof(T);

			if (!force && GetValueInDictionary<T>())
			{
				Debug.LogWarning(string.Format(LOG_EXISTS, type.Name));
				return;
			}

			Debug.Log(string.Format(instance == null ? LOG_DESTROY : LOG_SET, type.Name));

			Instances[typeof(T)] = instance;
		}

		/// <summary>
		/// If I am the instance, set the instance to null
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance">The instance</param>
		public static void DestroyInstance<T>(this T instance) where T : Object
		{
			Init();
			if (GetValueInDictionary<T>() != instance) return;

			System.Type type = typeof(T);
			Debug.Log(string.Format(LOG_DESTROY, type.Name));

			SetInstance<T>(null, true);
		}

		/// <summary>
		/// Set the current instance to null
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void DestroyInstance<T>() where T : Object
		{
			Init();
			singletonBehaviour.WaitForEndOfFrame(
				() => { SetInstance<T>(null, true); }
			);
		}

		private static T GetValueInDictionary<T>() where T : Object
		{
			Init();
			System.Type type = typeof(T);
			T instance = null;

			if (Instances.ContainsKey(type))
				instance = Instances[type] as T;

			return instance;
		}
	}
}
