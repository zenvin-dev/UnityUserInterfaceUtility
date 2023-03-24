using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zenvin.UI.Components.Grid {
	[DisallowMultipleComponent, RequireComponent (typeof (RectTransform))]
	public sealed class GridLayout : LayoutGroup {

		public enum CellSizeUnit {
			Fixed,
			Remaining,
		}

		private readonly List<Tuple<GridCell, Rect>> childCells = new List<Tuple<GridCell, Rect>> ();
		private float[] rowSizes;
		private float[] columnSizes;

		[SerializeField] private List<RowDefinition> rows;
		[SerializeField] private List<ColumnDefinition> columns;


		public int RowCount => rows.Count;
		public int ColumnCount => columns.Count;


		/// <summary>
		/// Adds a new column with a given unit and width to the grid.
		/// Will fail if the given width is equal to or less than 0.
		/// </summary>
		public void AddColumn (CellSizeUnit unit, float width) {
			if (width >= 0) {
				columns.Add (new ColumnDefinition () { Unit = unit, Width = width });
				UpdateGrid ();
			}
		}

		/// <summary>
		/// Adds a new row with a given unit and height to the grid.<br></br>
		/// Will fail if the given height is equal to or less than 0.
		/// </summary>
		public void AddRow (CellSizeUnit unit, float height) {
			if (height >= 0) {
				rows.Add (new RowDefinition () { Unit = unit, Height = height });
				UpdateGrid ();
			}
		}

		/// <summary>
		/// Gets a rect corresponding to a given grid cell's position and span.<br></br>
		/// If the position is outside of the grid's bounds or the span is 0 or less on one or both axes, an empty rect is returned.
		/// </summary>
		/// <param name="cell"> The index of the cell's column and row. </param>
		/// <param name="span"> The number of columns and rows that the cell spans. </param>
		public Rect GetRect (Vector2Int cell, Vector2Int span) {
			if (columnSizes == null || rowSizes == null) {
				UpdateGrid ();
			}
			return GetRect (cell, span, columnSizes, rowSizes);
		}

		internal Rect GetRect (Vector2Int cell, Vector2Int span, float[] columnValues, float[] rowValues) {
			if (columns == null || rows == null) {
				return new Rect();
			}
			if (columnValues == null || rowValues == null) {
				return new Rect ();
			}
			if (cell.x < 0 || cell.x >= columnValues.Length || cell.y < 0 || cell.y >= rowValues.Length || span.x <= 0 || span.y <= 0) {
				return new Rect ();
			}

			Vector2 position = Vector2.zero;
			Vector2 size = Vector2.zero;

			for (int i = 0; i < cell.x; i++) {
				position.x += columnValues[i];
			}
			for (int i = 0; i < cell.y; i++) {
				position.y += rowValues[i];
			}
			for (int i = cell.x; i < Mathf.Min (cell.x + span.x, columnValues.Length); i++) {
				size.x += columnValues[i];
			}
			for (int i = cell.y; i < Mathf.Min (cell.y + span.y, rowValues.Length); i++) {
				size.y += rowValues[i];
			}

			return new Rect (position, size);
		}

		/// <summary>
		/// Attempts to remove the column at the given index.
		/// </summary>
		public bool RemoveColumn (int column) {
			if (column >= 0 && column < columns.Count) {
				columns.RemoveAt (column);
				UpdateGrid ();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Attempts to remove the row at the given index.
		/// </summary>
		public bool RemoveRow (int row) {
			if (row >= 0 && row < rows.Count) {
				rows.RemoveAt (row);
				UpdateGrid ();
				return true;
			}
			return false;
		}

		public void SetColumnUnit (int column, CellSizeUnit unit) {
			if (column >= 0 && column < columns.Count && columns[column].Unit != unit) {
				columns[column].Unit = unit;
				UpdateGrid ();
			}
		}

		public void SetColumnWidth (int column, float width) {
			if (column >= 0 && column < columns.Count) {
				columns[column].Width = width;
				UpdateGrid ();
			}
		}

		public void SetRowUnit (int row, CellSizeUnit unit) {
			if (row >= 0 && row < rows.Count && rows[row].Unit != unit) {
				rows[row].Unit = unit;
				UpdateGrid ();
			}
		}

		public void SetRowHeight (int row, float height) {
			if (row >= 0 && row < rows.Count) {
				rows[row].Height = height;
				UpdateGrid ();
			}
		}


		private void UpdateGrid () {
			Vector2 totalSize = GetRectSize ();

			UpdateRowHeights (totalSize, ref rowSizes);
			UpdateColumnWidths (totalSize, ref columnSizes);
		}

		internal void UpdateRowHeights (Vector2 totalSize, ref float[] values, float scale = 1f) {
			InitializeArray (ref values, Mathf.Max (rows?.Count ?? 0, 1));

			if (rows == null || rows.Count <= 1) {
				values[0] = totalSize.y;
				return;
			}

			int relativeRows = 0;
			float relativeTotal = 0f;

			for (int i = 0; i < rows.Count; i++) {
				switch (rows[i].Unit) {
					case CellSizeUnit.Fixed:
						totalSize.y -= rows[i].Height * scale;
						values[i] = rows[i].Height * scale;
						break;
					case CellSizeUnit.Remaining:
						relativeRows++;
						relativeTotal += rows[i].Height;
						break;
				}
			}

			if (relativeRows == 0) {
				return;
			}

			for (int i = 0; i < rows.Count; i++) {
				if (rows[i].Unit == CellSizeUnit.Remaining) {
					float height = rows[i].Height / relativeTotal;
					values[i] = height * totalSize.y;
				}
			}
		}

		internal void UpdateColumnWidths (Vector2 totalSize, ref float[] values, float scale = 1f) {
			InitializeArray (ref values, Mathf.Max (columns?.Count ?? 0, 1));

			if (columns == null || columns.Count <= 1) {
				values[0] = totalSize.x;
				return;
			}

			int relativeColumns = 0;
			float relativeTotal = 0f;

			for (int i = 0; i < columns.Count; i++) {
				switch (columns[i].Unit) {
					case CellSizeUnit.Fixed:
						totalSize.x -= columns[i].Width * scale;
						values[i] = columns[i].Width * scale;
						break;
					case CellSizeUnit.Remaining:
						relativeColumns++;
						relativeTotal += columns[i].Width;
						break;
				}
			}

			if (relativeColumns == 0) {
				return;
			}

			for (int i = 0; i < columns.Count; i++) {
				if (columns[i].Unit == CellSizeUnit.Remaining) {
					float width = columns[i].Width / relativeTotal;
					values[i] = width * totalSize.x;
				}
			}
		}

		private void InitializeArray<T> (ref T[] arr, int size) {
			if (arr != null && arr.Length == size || size < 0) {
				return;
			}
			arr = new T[size];
		}

		internal Vector2 GetRectSize () {
			RectTransform rt = transform as RectTransform;
			Vector2 size = rt.rect.size;
			return size;
		}


		public override void CalculateLayoutInputHorizontal () {
			UpdateColumnWidths (GetRectSize (), ref columnSizes);
			SetLayoutHorizontal ();
		}

		public override void CalculateLayoutInputVertical () {
			UpdateRowHeights (GetRectSize (), ref rowSizes);
			SetLayoutVertical ();
		}

		public override void SetLayoutHorizontal () {
			childCells.Clear ();
			foreach (Transform child in transform) {
				if (child.TryGetComponent (out GridCell cell)) {
					var rect = GetRect (cell.Position, cell.Span);

					childCells.Add (new Tuple<GridCell, Rect> (cell, rect));
					cell.SetLayoutHorizontal (rect);
				}
			}
		}

		public override void SetLayoutVertical () {
			foreach (var cell in childCells) {
				cell.Item1.SetLayoutVertical (cell.Item2);
			}
		}
	}

	[Serializable]
	public class RowDefinition {
		[SerializeField] private GridLayout.CellSizeUnit unit;
		[SerializeField, Min (0)] private float height;

		public GridLayout.CellSizeUnit Unit { get => unit; internal set => unit = value; }
		public float Height { get => height; internal set => height = value; }
	}

	[Serializable]
	public class ColumnDefinition {
		[SerializeField] private GridLayout.CellSizeUnit unit;
		[SerializeField, Min (0)] private float width;

		public GridLayout.CellSizeUnit Unit { get => unit; internal set => unit = value; }
		public float Width { get => width; internal set => width = value; }
	}
}