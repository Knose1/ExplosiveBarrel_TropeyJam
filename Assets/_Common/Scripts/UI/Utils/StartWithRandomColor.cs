///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 06/06/2020 15:56
///-----------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Github.Knose1.Common.UI.Utils {
	[RequireComponent(typeof(Image))]
	public class StartWithRandomColor : MonoBehaviour {
		[SerializeField] List<Color> colors = new List<Color>();
		
		protected virtual void OnEnable () {
			GetComponent<Image>().color = colors[Random.Range(0, colors.Count)];
		}
	}
}