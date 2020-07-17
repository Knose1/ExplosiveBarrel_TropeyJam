using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.Github.Knose1.ExploseEm.Game.View
{
	public class Controller
	{
		public bool activateInput = true;
			
		public event Action OnBombsSet;
		protected List<Vector2> _bombList = new List<Vector2>();
		public    List<Vector2> BombList => _bombList;
		
		public void SetBombs(List<Vector2> bombList)
		{
			if (!activateInput) return;

			_bombList = bombList;
			OnBombsSet?.Invoke();
			_bombList = null;
		}

		public void Destroy()
		{
			OnBombsSet = null;
		}
	}
}
