using UnityEditor;
using UnityEngine;

namespace Zenvin.UI {
	public class RegistrationDebugWindow : EditorWindow {

		private Vector2 scroll;
		private bool wantsRedraw;

		[MenuItem ("Window/Zenvin/UI Registration Debugger")]
		private static void Init () {
			RegistrationDebugWindow window = GetWindow<RegistrationDebugWindow> ();
			window.titleContent = new GUIContent ("UI Registration");
			window.Show ();
		}

		private void OnGUI () {
			if (!Application.isPlaying) {
				EditorGUILayout.HelpBox ("Game must be running to debug", MessageType.Info);
				return;
			}

			GameObject sel = Selection.activeGameObject;
			if (sel == null) {
				EditorGUILayout.HelpBox ("No object selected", MessageType.Info);
				return;
			}

			scroll = GUILayout.BeginScrollView (scroll, false, false);
			DrawManager (sel.GetComponent<UserInterfaceManager> ());
			GUILayout.Space (20);
			DrawController (sel.GetComponent<UserInterfaceController> ());
			GUILayout.Space (20);
			DrawWidget (sel.GetComponent<UserInterfaceWidget> ());
			GUILayout.EndScrollView ();
		}

		private void DrawManager (UserInterfaceManager target) {
			if (target == null) {
				return;
			}

			EditorGUILayout.LabelField ("UI Manager - Registered Controllers", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			foreach (var ctrl in target) {
				EditorGUILayout.LabelField (ctrl.ToString ());
			}
			EditorGUI.indentLevel--;

			if (wantsRedraw) {
				wantsRedraw = false;
				GUI.changed = true;
				Repaint ();
			}
		}

		private void DrawController (UserInterfaceController target) {
			if (target == null) {
				return;
			}

			EditorGUILayout.LabelField ("UI Controller - Registered Widgets", EditorStyles.boldLabel);

			string search = EditorGUILayout.DelayedTextField ("Search by Path", "");
			if (!string.IsNullOrWhiteSpace (search)) {
				if (target.TryGetComponentByPath (search, out UserInterfaceWidget widget)) {
					EditorGUIUtility.PingObject (widget);
				} else {
					Debug.Log ($"Did not find a widget with path \"{search}\" in controller {target}");
				}
			}

			EditorGUI.indentLevel++;
			foreach (var group in target) {
				DrawGroup (group);
			}
			EditorGUI.indentLevel--;
		}

		private void DrawWidget (UserInterfaceWidget target) {
			if (target == null) {
				return;
			}

			EditorGUILayout.LabelField ("UI Widget - Child Widgets", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			foreach (var group in target) {
				DrawGroup (group);
			}
			EditorGUI.indentLevel--;
		}

		private void DrawGroup (WidgetGroup group) {
			EditorGUILayout.LabelField ($"Group \"{group.Identifier}\" ({group.Count} Widgets)");
			EditorGUI.indentLevel++;
			int i = 0;
			foreach (var widget in group) {
				EditorGUILayout.LabelField ($"{widget.name} (Index: {i})");
				i++;
			}
			EditorGUI.indentLevel--;
		}

		private void OnSelectionChange () {
			wantsRedraw = true;
		}

	}
}