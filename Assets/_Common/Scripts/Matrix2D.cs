using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.Github.Knose1.Common.Utils
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Matrix2D<T>
	{
		public bool autoUpdateWidth = true;

		private List<List<T>> grid = new List<List<T>>();

		/// <summary>
		/// 
		/// </summary>
		public List<List<T>> Grid { get => grid; }

		public Matrix2D() { }


		#region Line
		/// <summary>
		/// Add a multiple lines
		/// </summary>
		public void AddLines(List<List<T>> lines, bool cropLists)
		{
			for (int i = lines.Count - 1; i >= 0; i--)
			{
				CreateLine(lines[i], cropLists);
			}
		}

		/// <summary>
		/// Create and add a new line
		/// </summary>
		/// <returns>The pushed line</returns>
		public List<T> CreateLine()
		{
			return CreateLine(new List<T>());
		}

		/// <summary>
		/// Add a new line
		/// </summary>
		/// <returns>The pushed line</returns>
		public List<T> CreateLine(List<T> line, bool cropList = true)
		{
			if (!cropList && line.Count > Width) Width = line.Count;

			grid.Add(line);
			UpdateLineLength(line);

			return line;
		}

		/// <summary>
		/// Remove the last line
		/// </summary>
		public void RemoveLine()
		{
			RemoveLine(Height - 1);
		}

		/// <summary>
		/// Remove a line
		/// </summary>
		public void RemoveLine(int index)
		{
			grid.RemoveAt(index);
		}

		/// <summary>
		/// Remove a line
		/// </summary>
		public void RemoveLine(List<T> item)
		{
			grid.Remove(item);
		}
		#endregion Line

		#region Column
		/// <summary>
		/// Add a multiple columns
		/// </summary>
		public void AddColumns(List<List<T>> columns)
		{
			for (int i = columns.Count - 1; i >= 0; i--)
			{
				CreateColumn(columns[i]);
			}
		}
		/// <summary>
		/// Create and add a new column
		/// </summary>
		public void CreateColumn()
		{
			CreateColumn(new List<T>());
		}

		/// <summary>
		/// Add a new column
		/// </summary>
		public void CreateColumn(List<T> column)
		{
			int lWidth = grid.Count;
			while (column.Count < lWidth)
			{
				column.Add(default);
			}

			for (int i = 0; i < lWidth; i++)
			{
				grid[i].Add(column[i]);
			}

			width += 1;
		}

		/// <summary>
		/// Remove the last column
		/// </summary>
		public void RemoveColumn()
		{
			RemoveColumn(width - 1);
		}

		/// <summary>
		/// Remove a column
		/// </summary>
		public void RemoveColumn(int index)
		{
			for (int i = grid.Count - 1; i >= 0; i--)
			{
				grid[i].RemoveAt(index);
			}

			width -= 1;
		}
		#endregion Column

		#region Height / Width
		public int Height
		{
			get => grid.Count;
			set
			{
				int lWidth = Height;

				if (value == lWidth) return;
				if (value < lWidth)
				{
					grid.RemoveRange(value, lWidth - value);
					return;
				}

				while (grid.Count < value)
				{
					CreateLine();
				}
			}
		}

		private int width = 0;
		public int Width
		{
			get
			{
				if (autoUpdateWidth) UpdateHeight();
				return width;
			}
			set
			{
				width = value;

				for (int i = grid.Count - 1; i >= 0; i--)
				{
					UpdateLineLength(grid[i]);
				}
			}
		}

		#endregion Height / Width

		#region Matrix accessor
		public T this[int x, int y]
		{
			get
			{
				if (y >= grid.Count)
				{
					Debug.LogError($"Index out of range : ({nameof(x)}:{x},{nameof(y)}:{y})");
					return default;
				}

				List<T> yList = grid[y]; //Start with y because the grid is inverted
				if (x >= yList.Count)
				{
					Debug.LogError($"Index out of range : ({nameof(x)}:{x},{nameof(y)}:{y})");
					return default;
				}

				return yList[x];
			}
			set
			{
				if (grid == null) Debug.Log("NULL");
				while (grid.Count <= y)
				{
					CreateLine();
				}
				if (autoUpdateWidth && x >= Width) Width = x + 1;

				grid[y][x] = value;
			}
		}
		public List<T> this[int x]
		{
			get => grid[x];
			set
			{
				while (grid.Count < x)
				{
					CreateLine();
				}
				grid[x] = value;
				//grid[x].Capacity = Height;
			}
		}
		#endregion Matrix accessor

		/// <summary>
		/// Fill with default(T) or remove a line's item depending on the width.<br/>
		/// <br/>
		/// If there are too much items, the line will lose items.<br/>
		/// If there aren't enough items, it will be filled with default(T).<br/>
		/// </summary>
		/// <param name="line"></param>
		private void UpdateLineLength(List<T> line)
		{
			if (line.Count > width)
			{
				line.RemoveRange(width, line.Count - width);
				return;
			}

			while (line.Count < width)
			{
				line.Add(default);
			}

		}

		public void UpdateHeight()
		{
			int lMaxHeight = 0;
			List<T> lLine;
			for (int i = Height - 1; i >= 0; i--)
			{
				lLine = grid[i];
				if (lLine.Count > lMaxHeight) lMaxHeight = lLine.Count;
			}

			Width = lMaxHeight;
		}

		#region Equals / ToString / operator List<List<T>>
		public override bool Equals(object obj)
		{
			if (!(obj is Matrix2D<T>)) return false;

			Matrix2D<T> matrixObj = (Matrix2D<T>)obj;

			List<T> lInnerGridMe;
			List<T> lInnerGridObj;
			for (int i = grid.Count - 1; i >= 0; i--)
			{
				lInnerGridMe = grid[i];
				lInnerGridObj = matrixObj[i];

				if (lInnerGridObj.Count != lInnerGridMe.Count) return false;

				for (int j = lInnerGridMe.Count - 1; j >= 0; j--)
				{
					if (!lInnerGridMe[j].Equals(lInnerGridObj[j])) return false;
				}
			}

			return true;
		}

		public static bool operator ==(Matrix2D<T> matrix1, Matrix2D<T> matrix2)
		{
			return matrix1.Equals(matrix2);
		}

		public static bool operator !=(Matrix2D<T> matrix1, Matrix2D<T> matrix2)
		{
			return !matrix1.Equals(matrix2);
		}

		public static implicit operator List<List<T>>(Matrix2D<T> matrix)
		{
			return matrix.grid;
		}

		public override string ToString()
		{
			List<T> lInnerGridMe;
			string lToReturn = "";


			int lGridCount = grid.Count;
			for (int i = 0; i < lGridCount; i++)
			{
				lInnerGridMe = grid[i];

				lToReturn += "[";

				for (int j = 0; j < Width; j++)
				{
					lToReturn += lInnerGridMe[j].ToString() + ",";
				}
				lToReturn = lToReturn.Remove(lToReturn.Length - 1);
				lToReturn += "]\r\n";
			}

			return lToReturn;
		}

		public override int GetHashCode()
		{
			var hashCode = -1084719171;
			hashCode = hashCode * -1521134295 + autoUpdateWidth.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<List<List<T>>>.Default.GetHashCode(grid);
			hashCode = hashCode * -1521134295 + EqualityComparer<List<List<T>>>.Default.GetHashCode(Grid);
			hashCode = hashCode * -1521134295 + Height.GetHashCode();
			hashCode = hashCode * -1521134295 + width.GetHashCode();
			hashCode = hashCode * -1521134295 + Width.GetHashCode();
			return hashCode;
		}
		#endregion Equals / ToString / operator List<List<T>>
	}
}
