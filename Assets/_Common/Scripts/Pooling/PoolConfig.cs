///-----------------------------------------------------------------
/// Author : Knose1
///-----------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Common.Pooling {
	
	[CreateAssetMenu(
		menuName = "Common/Pooling" + nameof(PoolConfig),
		fileName = nameof(PoolConfig),
		order = 0
	)]
	
	public class PoolConfig : ScriptableObject {
		public GameObject prefab = null;
		public int maxItem = 0;
	}
}