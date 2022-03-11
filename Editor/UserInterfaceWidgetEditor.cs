using UnityEditor;
using UnityEngine;

namespace Zenvin.UI {
	[CustomEditor (typeof (UserInterfaceWidget), false), CanEditMultipleObjects]
	internal class UserInterfaceWidgetEditor : Editor {
		private void OnEnable () {
			(target as UserInterfaceWidget).hideFlags = HideFlags.HideInInspector;
		}
	}

	[InitializeOnLoad]
	internal static class UserInterfaceWidgetEditorHeader {

		private static GUIContent menuContent;
		private static GUIContent MenuContent {
			get {
				if (menuContent == null) {
					menuContent = EditorGUIUtility.IconContent ("_Menu");
				}
				return menuContent;
			}
		}

		private static GUIContent warningContent;
		private static GUIContent WarningContent {
			get {
				if (warningContent == null) {
					warningContent = EditorGUIUtility.IconContent ("Warning");
					warningContent.tooltip = "Invalid Identifier";
				}
				return warningContent;
			}
		}

		private static GUIContent selectContent;
		private static GUIContent SelectContent {
			get {
				if (selectContent == null) {
					selectContent = EditorGUIUtility.IconContent ("d_Grid.Default");
					selectContent.tooltip = "Select Parent Object";
				}
				return selectContent;
			}
		}


		static UserInterfaceWidgetEditorHeader () {
			Editor.finishedDefaultHeaderGUI += DrawHeader;
		}

		private static void DrawHeader (Editor editor) {
			if (editor.targets.Length > 1) {
				return;
			}

			if ((editor.target is UserInterfaceWidget widget || (editor.target is GameObject go && go.TryGetComponent (out widget))) && widget.GetType() == typeof (UserInterfaceWidget)) {
				SerializedObject obj = new SerializedObject (widget);

				if (Application.isPlaying) {
					GUILayout.BeginHorizontal ();
					GUILayout.Space (42);
					EditorGUILayout.LabelField ("Widget Identifier", widget.ID);
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Space (42);

					if (widget.Parent != null) {
						EditorGUILayout.LabelField ("Widget Parent", $"Widget '{widget.Parent.ID}'");
					} else if (widget.Controller != null) {
						EditorGUILayout.LabelField ("Widget Parent", $"Controller '{widget.Controller.GetType ().Name}'");
					} else {
						EditorGUILayout.LabelField ("Widget Parent", "<None>");
					}
					if (widget.HasParentControl) {
						if (GUILayout.Button (SelectContent, EditorStyles.label, GUILayout.Height (EditorGUIUtility.singleLineHeight), GUILayout.Width (EditorGUIUtility.singleLineHeight))) {
							Selection.activeObject = (Object)widget.Controller ?? widget.Parent;
						}
					}
					GUILayout.EndHorizontal ();
				} else {
					GUILayout.BeginHorizontal ();
					GUILayout.Space (42);
					EditorGUILayout.PropertyField (obj.FindProperty ("identifier"), new GUIContent ("Widget Identifier", "A string by which this User Interface Widget will be identified."));
					if (!widget.Valid) {
						GUILayout.Box (WarningContent, EditorStyles.label, GUILayout.Height (EditorGUIUtility.singleLineHeight), GUILayout.Width (EditorGUIUtility.singleLineHeight));
					}
					if (GUILayout.Button (MenuContent, EditorStyles.label, GUILayout.Height (EditorGUIUtility.singleLineHeight), GUILayout.Width (EditorGUIUtility.singleLineHeight))) {
						GenericMenu gm = new GenericMenu ();
						gm.AddItem (new GUIContent ("Use Object Name"), false, OnClickUseName, widget);
						gm.AddSeparator ("");
						gm.AddItem (new GUIContent ("Remove Component"), false, OnClickRemove, widget);
						gm.ShowAsContext ();
					}
					GUILayout.EndHorizontal ();
				}

				if (obj != null && obj.targetObject != null) {
					obj.ApplyModifiedProperties ();
				}
			}
		}

		private static void OnClickUseName (object value) {
			if (value is UserInterfaceWidget widget) {
				widget.ID = widget.name;
			}
		}

		private static void OnClickRemove (object value) {
			Object.DestroyImmediate ((Object)value, false);
		}

	}
}