using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Zenvin.UI.Components.Grid {
	[CustomEditor (typeof (GridLayout))]
	public class GridLayoutEditor : Editor {

		private static readonly Color CellHighlightColor = new Color (1f, 0f, 0.75f, 0.1f);
		private static readonly Color CellDividerColor = new Color (0.5f, 0f, 1f, 1f);

		private ReorderableList rows;
		private ReorderableList columns;
		private bool expandPreview = true;

		private float[] columnSizes;
		private float[] rowSizes;

		public override void OnInspectorGUI () {
			DrawRows ();
			DrawColumns ();

			if (target is GridLayout grid) {
				DrawGridPreview (grid);
			}

			if (serializedObject.hasModifiedProperties) {
				serializedObject.ApplyModifiedProperties ();
			}
		}

		private void DrawGridPreview (GridLayout grid) {
			expandPreview = EditorGUILayout.Foldout (expandPreview, "Preview");
			if (!expandPreview || grid == null) {
				return;
			}

			var gridSize = grid.GetRectSize ();
			var editorWidth = EditorGUIUtility.currentViewWidth;

			var aspectRatio = gridSize.y / gridSize.x;
			var editorScale = editorWidth / gridSize.x;

			var editorHeight = editorWidth * aspectRatio;
			var editorRect = GUILayoutUtility.GetRect (editorWidth, editorHeight);
			
			EditorGUI.DrawRect (editorRect, Color.gray);

			grid.UpdateColumnWidths (editorRect.size, ref columnSizes, editorScale);
			grid.UpdateRowHeights (editorRect.size, ref rowSizes, editorScale);

			for (int i = 0; i < grid.ColumnCount; i++) {
				var rect = grid.GetRect (new Vector2Int(i, 0), new Vector2Int(1, grid.RowCount), columnSizes, rowSizes);
				rect.position += editorRect.position;

				if (columns.index == i) {
					EditorGUI.DrawRect (rect, CellHighlightColor);
				}
				if (i > 0) {
					rect.width = 1f;
					EditorGUI.DrawRect (rect, CellDividerColor);
				}
			}

			for (int i = 0; i < grid.RowCount; i++) {
				var rect = grid.GetRect (new Vector2Int (0, i), new Vector2Int (grid.ColumnCount, 1), columnSizes, rowSizes);
				rect.position += editorRect.position;

				if (rows.index == i) {
					EditorGUI.DrawRect (rect, CellHighlightColor);
				}
				if (i > 0) {
					rect.height = 1f;
					EditorGUI.DrawRect (rect, CellDividerColor);
				}
			}
		}

		private void DrawRows () {
			if (rows == null) {
				rows = new ReorderableList (serializedObject, serializedObject.FindProperty ("rows"));
				rows.displayAdd = true;
				rows.displayRemove = true;
				rows.drawElementCallback = DrawRow;
				rows.drawHeaderCallback += DrawRowHeader;
			}
			rows.DoLayoutList ();
		}

		private void DrawRowHeader (Rect rect) {
			EditorGUI.LabelField (rect, "Rows");
		}

		private void DrawRow (Rect rect, int index, bool isActive, bool isFocused) {
			var prop = rows.serializedProperty.GetArrayElementAtIndex (index);
			rect = EditorGUI.PrefixLabel (rect, new GUIContent ($"Row {index}"));
			rect.width *= 0.5f;

			float height = rect.height - EditorGUIUtility.singleLineHeight;
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.y += height * 0.5f;

			var valueProp = prop.FindPropertyRelative ("height");
			var unitProp = prop.FindPropertyRelative ("unit");

			FloatField (rect, valueProp);
			rect.x += rect.width;
			EditorGUI.PropertyField (rect, unitProp, GUIContent.none);
		}

		private void DrawColumns () {
			if (columns == null) {
				columns = new ReorderableList (serializedObject, serializedObject.FindProperty ("columns"));
				columns.displayAdd = true;
				columns.displayRemove = true;
				columns.drawElementCallback = DrawColumn;
				columns.drawHeaderCallback += DrawColumnHeader;
			}
			columns.DoLayoutList ();
		}

		private void DrawColumnHeader (Rect rect) {
			EditorGUI.LabelField (rect, "Columns");
		}

		private void DrawColumn (Rect rect, int index, bool isActive, bool isFocused) {
			var prop = columns.serializedProperty.GetArrayElementAtIndex (index);
			rect = EditorGUI.PrefixLabel (rect, new GUIContent ($"Column {index}"));
			rect.width *= 0.5f;

			float height = rect.height - EditorGUIUtility.singleLineHeight;
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.y += height * 0.5f;

			var valueProp = prop.FindPropertyRelative ("width");
			var unitProp = prop.FindPropertyRelative ("unit");

			EditorGUI.PropertyField (rect, valueProp, GUIContent.none);
			rect.x += rect.width;
			EditorGUI.PropertyField (rect, unitProp, GUIContent.none);
		}


		private static void FloatField (Rect rect, SerializedProperty property) {
			if (property == null || property.propertyType != SerializedPropertyType.Float) {
				return;
			}
			float value = EditorGUI.FloatField (rect, property.floatValue);
			property.floatValue = value;
		}
	}
}