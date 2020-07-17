///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 19:44
///-----------------------------------------------------------------

using DG.Tweening;
using UnityEngine;

namespace Com.Github.Knose1.Common {
	public class DOTweenStarter : MonoBehaviour {
		private void Awake()
		{
			DOTween.Init();
		}
	}
}