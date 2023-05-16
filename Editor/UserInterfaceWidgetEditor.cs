using UnityEditor;
using UnityEngine;

namespace Zenvin.UI {
	[CustomEditor (typeof (UserInterfaceWidget), false)]
	internal class UserInterfaceWidgetEditor : Editor {

		internal static readonly float ButtonSize = EditorGUIUtility.singleLineHeight;

		private static GUIContent warningContent;
		internal static GUIContent WarningContent {
			get {
				if (warningContent == null) {
					warningContent = EditorGUIUtility.IconContent ("Warning");
					warningContent.tooltip = "Invalid Identifier";
				}
				return warningContent;
			}
		}


		public override void OnInspectorGUI () {

			GUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Identifier");
			DrawIdentifierProperty (serializedObject.FindProperty ("identifier"), (target as UserInterfaceWidget).Valid, false);
			GUILayout.EndHorizontal ();

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("registerPolicy"), new GUIContent ("Register Policy"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("unregisterPolicy"), new GUIContent ("Unregister Policy"));

			var autoOrderProp = serializedObject.FindProperty ("useAutoOrder");
			EditorGUILayout.PropertyField (autoOrderProp, new GUIContent ("Use Auto-Order"));
			EditorGUI.BeginDisabledGroup (autoOrderProp.boolValue);
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("order"), new GUIContent ("Order"));
			EditorGUI.EndDisabledGroup ();

			serializedObject.ApplyModifiedProperties ();
		}

		internal static void DrawIdentifierProperty (SerializedProperty prop, bool valid, bool placeholderSpace = true) {
			if (prop == null) {
				return;
			}

			EditorGUILayout.PropertyField (prop, GUIContent.none, GUILayout.ExpandWidth (true));

			if (!valid) {
				GUILayout.Box (WarningContent, EditorStyles.label, GUILayout.Height (ButtonSize), GUILayout.Width (ButtonSize));
			} else if (placeholderSpace) {
				GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorStyles.label.margin.left);
			}
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

		private static GUIContent dropdownContentFolded;
		private static GUIContent DropdownContentFolded {
			get {
				if (dropdownContentFolded == null) {
					dropdownContentFolded = EditorGUIUtility.IconContent ("d_scenevis_hidden_hover");
					dropdownContentFolded.tooltip = "Show component";
				}
				return dropdownContentFolded;
			}
		}

		private static GUIContent dropdownContentExpanded;
		private static GUIContent DropdownContentExpanded {
			get {
				if (dropdownContentExpanded == null) {
					dropdownContentExpanded = EditorGUIUtility.IconContent ("d_scenevis_visible_hover");
					dropdownContentExpanded.tooltip = "Hide component";
				}
				return dropdownContentExpanded;
			}
		}


		static UserInterfaceWidgetEditorHeader () {
			Editor.finishedDefaultHeaderGUI += DrawHeader;
		}


		private static void DrawHeader (Editor editor) {
			if (editor.targets.Length > 1) {
				return;
			}

			if ((editor.target is UserInterfaceWidget widget || (editor.target is GameObject go && go.TryGetComponent (out widget))) && widget.GetType () == typeof (UserInterfaceWidget)) {
				SerializedObject obj = new SerializedObject (widget);

				if (Application.isPlaying) {
					DrawRuntimeHeader (widget);
					return;
				}

				GUILayout.BeginHorizontal ();
				GUILayout.Space (42);
				bool repaint = DrawHeaderControls (obj, widget);
				GUILayout.EndHorizontal ();

				if (obj != null && obj.targetObject != null) {
					obj.ApplyModifiedProperties ();
				}
				if (repaint) {
					editor.Repaint ();
				}
			}
		}

		private static void DrawRuntimeHeader (UserInterfaceWidget widget) {
			GUILayout.BeginHorizontal ();
			GUILayout.Space (42);
			EditorGUILayout.LabelField ("Widget Identifier", widget.ID, GUILayout.Width (125));
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
		}

		private static bool DrawHeaderControls (SerializedObject obj, UserInterfaceWidget widget) {
			var repaint = false;
			var prop = obj.FindProperty ("identifier");
			var buttonSize = UserInterfaceWidgetEditor.ButtonSize;

			GUILayout.Label (new GUIContent ("Widget", "A string by which this User Interface Widget will be identified."), GUILayout.ExpandWidth (false));
			UserInterfaceWidgetEditor.DrawIdentifierProperty (prop, widget.Valid);

			bool expanded = (widget.hideFlags & HideFlags.HideInInspector) == 0;
			if (GUILayout.Button (expanded ? DropdownContentExpanded : DropdownContentFolded, EditorStyles.label, GUILayout.Height (buttonSize), GUILayout.Width (buttonSize))) {
				widget.hideFlags = expanded ? HideFlags.HideInInspector : HideFlags.None;
				repaint = true;
			}

			if (GUILayout.Button (MenuContent, EditorStyles.label, GUILayout.Height (EditorGUIUtility.singleLineHeight), GUILayout.Width (EditorGUIUtility.singleLineHeight))) {
				GenericMenu gm = new GenericMenu ();
				gm.AddItem (new GUIContent ("Use Object Name as ID"), false, OnClickUseName, widget);
				gm.AddSeparator ("");
				gm.AddItem (new GUIContent ("Remove Component"), false, OnClickRemove, widget);
				gm.ShowAsContext ();
			}

			return repaint;
		}

		private static void DrawAdditionalProperties (SerializedObject obj) {
			GUILayout.Label ("Register", GUILayout.ExpandWidth (false));
			EditorGUILayout.PropertyField (obj.FindProperty ("registerPolicy"), GUIContent.none);
			GUILayout.FlexibleSpace ();
			GUILayout.Label ("Unregister", GUILayout.ExpandWidth (false));
			EditorGUILayout.PropertyField (obj.FindProperty ("unregisterPolicy"), GUIContent.none);
			GUILayout.FlexibleSpace ();
			GUILayout.Label ("Order", GUILayout.ExpandWidth (false));
			EditorGUILayout.PropertyField (obj.FindProperty ("order"), GUIContent.none);
		}


		private static void OnClickUseName (object value) {
			if (value is UserInterfaceWidget widget) {
				widget.ID = widget.name;
			}
		}

		private static void OnClickRemove (object value) {
			Object.DestroyImmediate ((Object)value, false);
		}


		private class PropertyValue {
			public readonly SerializedProperty Prop;
			public readonly object Value;


			public PropertyValue (SerializedProperty prop, object value) {
				Prop = prop;
				Value = value;
			}
		}
	}
}