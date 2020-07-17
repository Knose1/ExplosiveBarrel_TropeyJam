///-----------------------------------------------------------------
/// Author : Knose1
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.Github.Knose1.Common.Pooling {
	public interface IPoolBehaviourStart
	{
		/// <summary>
		/// Dispatched when Instantiated by the pool
		/// </summary>
		void PoolStart();
	}

	public interface IPoolBehaviourLateStart
	{
		void LatePoolStart();
	}

	public interface IPoolBehaviourDestroy
	{
		/// <summary>
		/// Dispatched when Destroyed by the pool
		/// </summary>
		void PoolDestroy();
	}

	public interface IPoolBehaviour : IPoolBehaviourStart, IPoolBehaviourDestroy {}
}