using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Github.Knose1.Common.UI
{
	public interface IBetterGridElement
	{
		void OnMoved(int x, int y);
		void AddedToGrid(int x, int y);
		void RemovedFromGrid(int x, int y);
	}
}
