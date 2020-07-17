///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 10/06/2020 17:18
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.Github.Knose1.Common.UI {
	public class BetterGridElementDestroyOnDrop : MonoBehaviour, IBetterGridElement
	{
		public virtual void AddedToGrid(int x, int y) { }
		public virtual void OnMoved(int x, int y){}

		public virtual void RemovedFromGrid(int x, int y)
		{
			Destroy(gameObject);
		}
	}
}