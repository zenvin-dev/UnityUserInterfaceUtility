using UnityEditor;
using UnityEngine;
using Zenvin.UI.Layout;

namespace Zenvin.UI.Components.Grid {
	//[CustomEditor (typeof (GridLayout))]
	public class GridLayoutEditor : Editor {

		private static readonly Color CellHighlightColor = new Color (1f, 0f, 0.75f, 0.1f);
		private static readonly Color CellDividerColor = new Color (0.5f, 0f, 1f, 1f);

		private float[] columnSizes;
		private float[] rowSizes;

		public override void DrawPreview (Rect previewArea) {
			DrawGridPreview (previewArea, target as GridLayout);
		}

		private void DrawGridPreview (Rect previewRect, GridLayout grid) {
			if (grid == null) {
				return;
			}

			var gridSize = grid.GetRectSize ();
			var editorWidth = EditorGUIUtility.currentViewWidth;

			var aspectRatio = gridSize.y / gridSize.x;
			var editorScale = editorWidth / gridSize.x;

			var editorHeight = editorWidth * aspectRatio;
			var editorRect = GUILayoutUtility.GetRect (editorWidth, editorHeight);
			
			EditorGUI.DrawRect (editorRect, Color.gray);

			LayoutUtility.CalculateColumnWidths (grid.Columns, editorRect.size, ref columnSizes, editorScale);
			LayoutUtility.CalculateRowHeights (grid.Rows, editorRect.size, ref rowSizes, editorScale);

			for (int i = 0; i < grid.ColumnCount; i++) {
				var rect = grid.GetRect (new Vector2Int(i, 0), new Vector2Int(1, grid.RowCount), columnSizes, rowSizes);
				rect.position += editorRect.position;

				//if (columns.index == i) {
				//	EditorGUI.DrawRect (rect, CellHighlightColor);
				//}
				if (i > 0) {
					rect.width = 1f;
					EditorGUI.DrawRect (rect, CellDividerColor);
				}
			}

			for (int i = 0; i < grid.RowCount; i++) {
				var rect = grid.GetRect (new Vector2Int (0, i), new Vector2Int (grid.ColumnCount, 1), columnSizes, rowSizes);
				rect.position += editorRect.position;

				//if (rows.index == i) {
				//	EditorGUI.DrawRect (rect, CellHighlightColor);
				//}
				if (i > 0) {
					rect.height = 1f;
					EditorGUI.DrawRect (rect, CellDividerColor);
				}
			}
		}
	}
}