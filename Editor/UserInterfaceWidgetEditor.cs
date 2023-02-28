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

			if ((editor.target is UserInterfaceWidget widget || (editor.target is GameObject go && go.TryGetComponent (out widget))) && widget.GetType () == typeof (UserInterfaceWidget)) {
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
					GUILayout.Label (new GUIContent ("Widget", "A string by which this User Interface Widget will be identified."));
					EditorGUILayout.PropertyField (obj.FindProperty ("identifier"), GUIContent.none);
					if (!widget.Valid) {
						GUILayout.Box (WarningContent, EditorStyles.label, GUILayout.Height (EditorGUIUtility.singleLineHeight), GUILayout.Width (EditorGUIUtility.singleLineHeight));
					} else {
						GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorStyles.label.margin.left);
					}
					if (GUILayout.Button (MenuContent, EditorStyles.label, GUILayout.Height (EditorGUIUtility.singleLineHeight), GUILayout.Width (EditorGUIUtility.singleLineHeight))) {
						GenericMenu gm = new GenericMenu ();
						gm.AddItem (new GUIContent ("Use Object Name"), false, OnClickUseName, widget);
						gm.AddSeparator ("");
						AddRegisterPolicyOptions (gm, obj.FindProperty ("registerPolicy"));
						AddUnregisterPolicyOptions (gm, obj.FindProperty ("unregisterPolicy"));
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

		private static void AddUnregisterPolicyOptions (GenericMenu gm, SerializedProperty prop) {
			if (gm == null || prop == null) {
				return;
			}
			gm.AddItem (
				new GUIContent ("Register Policy/On Awake"),
				prop.enumValueIndex == (int)UserInterfaceWidget.RegisterPolicy.OnAwake,
				OnClickRegisterPolicy,
				new PropertyValue(prop, UserInterfaceWidget.RegisterPolicy.OnAwake)
			);
			gm.AddItem (
				new GUIContent ("Register Policy/On Enable"),
				prop.enumValueIndex == (int)UserInterfaceWidget.RegisterPolicy.OnEnable,
				OnClickRegisterPolicy,
				new PropertyValue (prop, UserInterfaceWidget.RegisterPolicy.OnEnable)
			);
			gm.AddItem (
				new GUIContent ("Register Policy/On Start"),
				prop.enumValueIndex == (int)UserInterfaceWidget.RegisterPolicy.OnStart,
				OnClickRegisterPolicy,
				new PropertyValue (prop, UserInterfaceWidget.RegisterPolicy.OnStart)
			);
			gm.AddItem (
				new GUIContent ("Register Policy/Manual"),
				prop.enumValueIndex == (int)UserInterfaceWidget.RegisterPolicy.Manual,
				OnClickRegisterPolicy,
				new PropertyValue (prop, UserInterfaceWidget.RegisterPolicy.Manual)
			);
		}

		private static void AddRegisterPolicyOptions (GenericMenu gm, SerializedProperty prop) {
			if (gm == null || prop == null) {
				return;
			}
			gm.AddItem (
				new GUIContent ("Unegister Policy/On Disable"),
				prop.enumValueIndex == (int)UserInterfaceWidget.UnregisterPolicy.OnDisable,
				OnClickRegisterPolicy,
				new PropertyValue (prop, UserInterfaceWidget.UnregisterPolicy.OnDisable)
			);
			gm.AddItem (
				new GUIContent ("Unegister Policy/On Destroy"),
				prop.enumValueIndex == (int)UserInterfaceWidget.UnregisterPolicy.OnDestroy,
				OnClickRegisterPolicy,
				new PropertyValue (prop, UserInterfaceWidget.UnregisterPolicy.OnDestroy)
			);
		}

		private static void OnClickUseName (object value) {
			if (value is UserInterfaceWidget widget) {
				widget.ID = widget.name;
			}
		}

		private static void OnClickRegisterPolicy (object value) {
			if (!(value is PropertyValue val)) {
				return;
			}
			switch (val.Value) {
				case UserInterfaceWidget.RegisterPolicy reg:
					val.Prop.enumValueIndex = (int)reg;
					val.Prop.serializedObject.ApplyModifiedProperties ();
					break;
				case UserInterfaceWidget.UnregisterPolicy uReg:
					val.Prop.enumValueIndex = (int)uReg;
					val.Prop.serializedObject.ApplyModifiedProperties ();
					break;
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