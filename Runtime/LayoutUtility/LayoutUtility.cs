using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.UI.Layout {
	public static class LayoutUtility {

		public static void CalculateRowHeights (List<RowDefinition> rows, Vector2 totalSize, float spacing, ref float[] values, float scale = 1f) {
			float v = 0f;
			CalculateRowHeights (rows, totalSize, spacing, ref values, ref v, scale);
		}

		public static void CalculateRowHeights (List<RowDefinition> rows, Vector2 totalSize, float spacing, ref float[] values, ref float minSize, float scale = 1f) {
			InitializeArray (ref values, Mathf.Max (rows?.Count ?? 0, 1));
			minSize = 0f;

			if (rows == null || rows.Count <= 1) {
				values[0] = totalSize.y;
				return;
			}

			spacing = spacing * (rows.Count - 1);
			totalSize.y -= spacing;
			minSize += spacing;

			int relativeRows = 0;
			float relativeTotal = 0f;

			for (int i = 0; i < rows.Count; i++) {
				switch (rows[i].Unit) {
					case CellSizeUnit.Fixed:
						float tempHeight = rows[i].Height * scale;
						totalSize.y -= tempHeight;
						values[i] = tempHeight;
						minSize += tempHeight;
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

		public static void CalculateColumnWidths (List<ColumnDefinition> columns, Vector2 totalSize, float spacing, ref float[] values, float scale = 1f) {
			float v = 0f;
			CalculateColumnWidths (columns, totalSize, spacing, ref values, ref v, scale);
		}

		public static void CalculateColumnWidths (List<ColumnDefinition> columns, Vector2 totalSize, float spacing, ref float[] values, ref float minSize, float scale = 1f) {
			InitializeArray (ref values, Mathf.Max (columns?.Count ?? 0, 1));
			minSize = 0f;

			if (columns == null || columns.Count <= 1) {
				values[0] = totalSize.x;
				return;
			}

			spacing = spacing * (columns.Count - 1);
			totalSize.x -= spacing;
			minSize += spacing;

			int relativeColumns = 0;
			float relativeTotal = 0f;

			for (int i = 0; i < columns.Count; i++) {
				switch (columns[i].Unit) {
					case CellSizeUnit.Fixed:
						float tempWidth = columns[i].Width * scale;
						totalSize.x -= tempWidth;
						values[i] = tempWidth;
						minSize += tempWidth;
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

		private static void InitializeArray<T> (ref T[] arr, int size) {
			if (arr != null && arr.Length == size || size < 0) {
				return;
			}
			arr = new T[size];
		}

	}
}