using UIController = Zenvin.UI.UserInterfaceControllerBase;
using System.Collections.Generic;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo ("Zenvin.UI.Editor")]

namespace Zenvin.UI {
	[DisallowMultipleComponent, DefaultExecutionOrder (-10)]
	public class UserInterfaceWidget : MonoBehaviour {

		private UserInterfaceControllerBase controller;

		private UserInterfaceWidget parent;
		private Dictionary<string, WidgetGroup> children = new Dictionary<string, WidgetGroup>();

		[SerializeField, Tooltip ("The identifier with which the widget will register itself in the parent controller.")]
		private string identifier;


		/// <summary> The identifier with which the Widget will register itself to the parent control. </summary>
		public string ID {
			get {
				return identifier;
			}
			internal set {
				identifier = value ?? "";
			}
		}
		/// <summary> Returns whether this Widget has a valid <see cref="ID"/>. </summary>
		public bool Valid => !string.IsNullOrEmpty (ID) && !ID.Contains (UIController.PathSeparator.ToString ()) && !ID.Contains (UIController.IndexSeparator.ToString ());

		public UIController Controller => controller;
		public UserInterfaceWidget Parent => parent;
		public bool HasParentControl => Parent != null || Controller != null;


		private void OnEnable () {
			if (!Valid) {
				return;
			}
			if (parent != null || controller != null) {
				return;
			}

			FindParentControl ();
			if (controller != null) {
				controller.Register (this);
			}
		}

		private void OnDisable () {
			if (controller != null) {
				controller.Unregister (this);
			}
			if (parent != null) {
				parent.RemoveChild (this);
			}
		}


		public bool TryGetChildWidget (string identifier, out UserInterfaceWidget widget) {
			if (children.TryGetValue (identifier, out WidgetGroup group)) {
				widget = group.First;
				return widget != null;
			}
			widget = null;
			return false;
		}

		public bool TryGetChildWidget (string identifier, int index, out UserInterfaceWidget widget) {
			if (children.TryGetValue (identifier, out WidgetGroup group)) {
				widget = group[index];
				return widget != null;
			}
			widget = null;
			return false;
		}

		public bool TryGetAllChildWidgets (string identifier, out WidgetGroup widgetGroup) {
			if (children.TryGetValue (identifier, out widgetGroup)) {
				return true;
			}
			widgetGroup = null;
			return false;
		}

		public bool TryGetChildWidgetByPath (string path, out UserInterfaceWidget widget) {
			widget = null;
			if (string.IsNullOrEmpty (path)) {
				return false;
			}

			string[] parts = path.Split (UserInterfaceControllerBase.PathSplitValues, System.StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 1) {
				return TryGetChildWidget (parts[0], out widget);
			}

			UserInterfaceWidget current = this;
			for (int i = 0; i < path.Length; i++) {
				if (!current.children.TryGetValue (parts[i], out WidgetGroup group) || !group.TryGetFirst (out current)) {
					return false;
				}
			}

			widget = current;
			return true;
		}

		public bool TryGetChildComponent<T> (string identifier, out T value) where T : Component {
			value = null;
			if (children == null || !children.TryGetValue (identifier, out WidgetGroup group) || !group.TryGetFirst (out UserInterfaceWidget widget)) {
				return false;
			}
			value = widget.GetComponent<T> ();
			return value != null;
		}

		public bool TryGetChildComponent<T> (string identifier, int index, out T value) where T : Component {
			value = null;
			if (children == null || !children.TryGetValue (identifier, out WidgetGroup group) || !group.TryGet (index, out UserInterfaceWidget widget)) {
				return false;
			}
			value = widget.GetComponent<T> ();
			return value != null;
		}

		public bool TryGetChildComponentByPath<T> (string path, out T value) where T : Component {
			if (!TryGetChildWidgetByPath(path, out UserInterfaceWidget widget)) {
				value = null;
				return false;
			}
			value = widget.GetComponent<T> ();
			return value != null;
		}


		private void FindParentControl () {
			if (controller != null)
				return;

			Transform current = transform;
			
			do {

				if (current.TryGetComponent (out controller)) {
					break;
				}
				if (current != transform && current.TryGetComponent (out parent)) {
					parent.AddChild (this);
					break;
				}

				current = current.parent;

			} while (current != null);
		}

		private void AddChild (UserInterfaceWidget widget) {
			if (widget == null || !widget.Valid)
				return;

			if (children == null) {
				children = new Dictionary<string, WidgetGroup> ();
			}

			if (children.TryGetValue (widget.ID, out WidgetGroup group)) {
				group.Add (widget);
			} else {
				group = new WidgetGroup (widget);
				children.Add (widget.ID, group);
			}
		}

		private void RemoveChild (UserInterfaceWidget widget) {
			if (children.TryGetValue (widget.identifier, out WidgetGroup group)) {
				if (group.Remove (widget, out bool empty) && empty) {
					children.Remove (widget.identifier);
				}
			}
		}

	}
}