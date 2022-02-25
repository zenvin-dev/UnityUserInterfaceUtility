using System.Collections.Generic;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo ("Zenvin.UI.Editor")]

namespace Zenvin.UI {
	[DisallowMultipleComponent, DefaultExecutionOrder (-10)]
	public class UserInterfaceWidget : MonoBehaviour {

		private UserInterfaceControllerBase controller;

		private UserInterfaceWidget parent;
		private Dictionary<string, UserInterfaceWidget> children = new Dictionary<string, UserInterfaceWidget>();

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
		public bool Valid => !string.IsNullOrEmpty (ID);
		public UserInterfaceWidget Parent => parent;


		private void OnEnable () {
			if (!Valid) {
				return;
			}

			GetParentControl ();
			if (controller != null) {
				controller.Register (this);
			}
		}

		private void OnDisable () {
			if (controller != null) {
				controller.Unregister (this);
			}
			if (parent != null) {
				parent.RemoveChild (identifier);
			}
		}

		public bool TryGetChildWidget (string identifier, out UserInterfaceWidget widget) {
			return children.TryGetValue (identifier, out widget);
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
				if (!current.children.TryGetValue (parts[i], out current)) {
					return false;
				}
			}

			widget = current;
			return true;
		}

		public bool TryGetChildComponent<T> (string identifier, out T value) where T : Component {
			value = null;
			if (children == null || !children.TryGetValue (identifier, out UserInterfaceWidget widget)) {
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

		private void GetParentControl () {
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
				children = new Dictionary<string, UserInterfaceWidget> ();
			}

			children.Add (widget.ID, widget);
		}

		private void RemoveChild (string identifier) {
			children?.Remove (identifier);
		}

	}
}