using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenvin.UI.Layout;

using static Zenvin.UI.Layout.LayoutUtility;

namespace Zenvin.UI.Components.Grid {
	[DisallowMultipleComponent, RequireComponent (typeof (RectTransform)), ExecuteAlways]
	public sealed class GridLayout : UIBehaviour, ILayoutGroup, ILayoutElement {

		private readonly List<Tuple<GridCell, Rect>> childCells = new List<Tuple<GridCell, Rect>> ();
		private RectTransform gridRectTransform;
		private float[] rowSizes;
		private float[] columnSizes;
		private Vector2 minSize;
		private Coroutine updateCoroutine;

		[SerializeField] private Vector2 spacing;
		[SerializeField] private List<RowDefinition> rows;
		[SerializeField] private List<ColumnDefinition> columns;


		public int RowCount => rows.Count;
		public int ColumnCount => columns.Count;
		public Vector2 Spacing { get => spacing; set => spacing = value; }
		
		internal List<RowDefinition> Rows => rows;
		internal List<ColumnDefinition> Columns => columns;

		internal RectTransform GridRectTransform => gridRectTransform == null ? (gridRectTransform = transform as RectTransform) : gridRectTransform;

		float ILayoutElement.minWidth => minSize.x;
		float ILayoutElement.preferredWidth => minSize.x;
		float ILayoutElement.flexibleWidth => minSize.x;
		float ILayoutElement.minHeight => minSize.y;
		float ILayoutElement.preferredHeight => minSize.y;
		float ILayoutElement.flexibleHeight => minSize.y;
		int ILayoutElement.layoutPriority => 0;


		public void ForceUpdateLayout () {
			SetDirty ();
		}

		protected override void OnDidApplyAnimationProperties () {
			SetDirty ();
		}

		protected override void OnEnable () {
			base.OnEnable ();
			SetDirty ();
		}

		protected override void OnDisable () {
			LayoutRebuilder.MarkLayoutForRebuild (GridRectTransform);
			base.OnDisable ();
		}


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
				return new Rect ();
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
				position.x += Spacing.x;
			}
			for (int i = 0; i < cell.y; i++) {
				position.y += rowValues[i];
				position.y += Spacing.y;
			}

			for (int i = cell.x; i < Mathf.Min (cell.x + span.x, columnValues.Length); i++) {
				size.x += columnValues[i];
			}
			for (int i = cell.y; i < Mathf.Min (cell.y + span.y, rowValues.Length); i++) {
				size.y += rowValues[i];
			}

			if (span.x == columnValues.Length) {
				size.x += Spacing.x;
			}
			if (span.y == rowValues.Length) {
				size.y += Spacing.y;
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


		internal Vector2 GetRectSize () {
			RectTransform rt = transform as RectTransform;
			Vector2 size = rt.rect.size;
			return size;
		}


		private void UpdateGrid () {
			Vector2 totalSize = GetRectSize ();

			CalculateRowHeights (rows, totalSize, Spacing.y, ref rowSizes);
			CalculateColumnWidths (columns, totalSize, Spacing.x, ref columnSizes);
		}

		private void SetDirty () {
			if (!IsActive ()) {
				return;
			}
			if (!CanvasUpdateRegistry.IsRebuildingLayout ()) {
				LayoutRebuilder.MarkLayoutForRebuild (GridRectTransform);
			} else if (updateCoroutine == null) {
				updateCoroutine = StartCoroutine (DoDelayedLayoutUpdate (GridRectTransform));
			}
		}

		private IEnumerator DoDelayedLayoutUpdate (RectTransform rt) {
			yield return null;
			LayoutRebuilder.MarkLayoutForRebuild (rt);
			updateCoroutine = null;
		}


		void ILayoutElement.CalculateLayoutInputHorizontal () {
			float minWidth = 0f;
			CalculateColumnWidths (columns, GetRectSize (), Spacing.x, ref columnSizes, ref minWidth);
			minSize.x = minWidth;
		}

		void ILayoutElement.CalculateLayoutInputVertical () {
			float minHeight = 0f;
			CalculateRowHeights (rows, GetRectSize (), Spacing.y, ref rowSizes, ref minHeight);
			minSize.y = minHeight;
		}

		void ILayoutController.SetLayoutHorizontal () {
			childCells.Clear ();
			foreach (Transform child in transform) {
				if (child.TryGetComponent (out GridCell cell)) {
					var rect = GetRect (cell.Position, cell.Span);

					childCells.Add (new Tuple<GridCell, Rect> (cell, rect));
					cell.SetLayoutHorizontal (rect);
				}
			}
		}

		void ILayoutController.SetLayoutVertical () {
			foreach (var cell in childCells) {
				cell.Item1.SetLayoutVertical (cell.Item2);
			}
		}


#if UNITY_EDITOR
		private void Update () {
			if (Application.isPlaying) {
				return;
			}
			SetDirty ();
		}

		protected override void OnValidate () {
			base.OnValidate ();
			SetDirty ();
		}
#endif
	}
}