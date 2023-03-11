using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zenvin.UI.Components.Grid {
	[DisallowMultipleComponent, ExecuteInEditMode, RequireComponent (typeof (RectTransform))]
	public sealed class GridLayout : UIBehaviour, ILayoutGroup {

		public enum CellSizeUnit {
			Fixed,
			Remaining,
		}

		private float[] rowSizes;
		private float[] columnSizes;

		[SerializeField] private List<RowDefinition> rows;
		[SerializeField] private List<ColumnDefinition> columns;


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
			for (int i = cell.x; i < cell.x + span.x; i++) {
				size.x += columnSizes[i];
			}
			for (int i = cell.y; i < cell.y + span.y; i++) {
				size.y += rowSizes[i];
			}

			return new Rect (position, size);
		}


		protected override void OnRectTransformDimensionsChange () {
			base.OnRectTransformDimensionsChange ();
			UpdateGrid ();
		}

		protected override void OnValidate () {
			base.OnValidate ();
			UpdateGrid ();

			foreach (Transform child in transform) {
				if (child.TryGetComponent (out GridCell cell)) {
					cell.UpdateCell ();
				}
			}
		}

		private void UpdateGrid () {
			InitializeArray (ref rowSizes, Mathf.Max (rows.Count, 1));
			InitializeArray (ref columnSizes, Mathf.Max (columns.Count, 1));
			Vector2 totalSize = (transform as RectTransform).sizeDelta;

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
						totalSize.x -= rows[i].Height;
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
					rowSizes[i] = height * totalSize.x;
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

		private void InitializeArray<T> (ref T[] arr, int size) {
			if (arr != null && arr.Length == size || size < 0) {
				return;
			}
			arr = new T[size];
		}

		void ILayoutController.SetLayoutHorizontal () {
			
		}

		void ILayoutController.SetLayoutVertical () {
			
		}
	}

	[Serializable]
	public class RowDefinition {
		[field: SerializeField] public GridLayout.CellSizeUnit Unit { get; private set; }
		[field: SerializeField, Min (0)] public float Height { get; private set; }
	}

	[Serializable]
	public class ColumnDefinition {
		[field: SerializeField] public GridLayout.CellSizeUnit Unit { get; private set; }
		[field: SerializeField, Min (0)] public float Width { get; private set; }
	}
}