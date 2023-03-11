using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Zenvin.UI.Components.Grid {
	[DisallowMultipleComponent, ExecuteInEditMode, RequireComponent (typeof (RectTransform))]
	public sealed class GridLayout : UIBehaviour {

		public enum CellSizeUnit {
			Fixed,
			Remaining,
		}

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
				UpdateChildren ();
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
				UpdateChildren ();
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
			if (cell.x < 0 || cell.x >= columnSizes.Length || cell.y < 0 || cell.y >= rowSizes.Length || span.x <= 0 || span.y <= 0) {
				return new Rect ();
			}

			Vector2 position = Vector2.zero;
			Vector2 size = Vector2.zero;

			for (int i = 0; i < cell.x; i++) {
				position.x += columnSizes[i];
			}
			for (int i = 0; i < cell.y; i++) {
				position.y += rowSizes[i];
			}
			for (int i = cell.x; i < Mathf.Min (cell.x + span.x, columnSizes.Length); i++) {
				size.x += columnSizes[i];
			}
			for (int i = cell.y; i < Mathf.Min (cell.y + span.y, rowSizes.Length); i++) {
				size.y += rowSizes[i];
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
				UpdateChildren ();
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
				UpdateChildren ();
				return true;
			}
			return false;
		}

		public void SetColumnUnit (int column, CellSizeUnit unit) {
			if (column >= 0 && column < columns.Count && columns[column].Unit != unit) {
				columns[column].Unit = unit;
				UpdateGrid ();
				UpdateChildren ();
			}
		}

		public void SetColumnWidth (int column, float width) {
			if (column >= 0 && column < columns.Count) {
				columns[column].Width = width;
				UpdateGrid ();
				UpdateChildren ();
			}
		}

		public void SetRowUnit (int row, CellSizeUnit unit) {
			if (row >= 0 && row < rows.Count && rows[row].Unit != unit) {
				rows[row].Unit = unit;
				UpdateGrid ();
				UpdateChildren ();
			}
		}

		public void SetRowHeight (int row, float height) {
			if (row >= 0 && row < rows.Count) {
				rows[row].Height = height;
				UpdateGrid ();
				UpdateChildren ();
			}
		}


		protected override void OnRectTransformDimensionsChange () {
			base.OnRectTransformDimensionsChange ();
			UpdateGrid ();
		}

		protected override void OnValidate () {
			base.OnValidate ();
			UpdateGrid ();
			UpdateChildren ();
		}

		private void UpdateGrid () {
			InitializeArray (ref rowSizes, Mathf.Max (rows?.Count ?? 0, 1));
			InitializeArray (ref columnSizes, Mathf.Max (columns?.Count ?? 0, 1));
			Vector2 totalSize = (transform as RectTransform).rect.size;
			totalSize.Scale (transform.lossyScale);

			UpdateRowHeights (totalSize);
			UpdateColumnWidths (totalSize);
		}

		private void UpdateRowHeights (Vector2 totalSize) {
			if (rows.Count <= 1) {
				rowSizes[0] = totalSize.y;
				return;
			}

			int relativeRows = 0;
			float relativeTotal = 0f;

			for (int i = 0; i < rows.Count; i++) {
				switch (rows[i].Unit) {
					case CellSizeUnit.Fixed:
						totalSize.y -= rows[i].Height;
						rowSizes[i] = rows[i].Height;
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
					rowSizes[i] = height * totalSize.y;
				}
			}
		}

		private void UpdateColumnWidths (Vector2 totalSize) {
			if (columns.Count <= 1) {
				columnSizes[0] = totalSize.x;
				return;
			}

			int relativeColumns = 0;
			float relativeTotal = 0f;

			for (int i = 0; i < columns.Count; i++) {
				switch (columns[i].Unit) {
					case CellSizeUnit.Fixed:
						totalSize.x -= columns[i].Width;
						columnSizes[i] = columns[i].Width;
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
					columnSizes[i] = width * totalSize.x;
				}
			}
		}

		private void UpdateChildren () {
			foreach (Transform child in transform) {
				if (child.TryGetComponent (out GridCell cell)) {
					cell.UpdateCell ();
				}
			}
		}

		private void InitializeArray<T> (ref T[] arr, int size) {
			if (arr != null && arr.Length == size || size < 0) {
				return;
			}
			arr = new T[size];
		}
	}

	[Serializable]
	public class RowDefinition {
		[field: SerializeField] public GridLayout.CellSizeUnit Unit { get; internal set; }
		[field: SerializeField, Min (0)] public float Height { get; internal set; }
	}

	[Serializable]
	public class ColumnDefinition {
		[field: SerializeField] public GridLayout.CellSizeUnit Unit { get; internal set; }
		[field: SerializeField, Min (0)] public float Width { get; internal set; }
	}
}