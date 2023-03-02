using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Zenvin.UI.UserInterfaceManager;
using UIController = Zenvin.UI.UserInterfaceController;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo ("Zenvin.UI.Editor")]

namespace Zenvin.UI {
	[DisallowMultipleComponent, DefaultExecutionOrder (-10)]
	public class UserInterfaceWidget : MonoBehaviour, IEnumerable<WidgetGroup>, IComparable<UserInterfaceWidget> {

		public enum RegisterPolicy {
			OnEnable,
			OnAwake,
			OnStart,
			Manual,
		}

		public enum UnregisterPolicy {
			OnDisable,
			OnDestroy,
		}

		private UIController controller;

		private UserInterfaceWidget parent;
		private Dictionary<string, WidgetGroup> children = new Dictionary<string, WidgetGroup> ();

		[SerializeField, Tooltip ("The identifier with which the widget will register itself in the parent controller.")]
		private string identifier;
		[SerializeField]
		private RegisterPolicy registerPolicy = RegisterPolicy.OnEnable;
		[SerializeField]
		private UnregisterPolicy unregisterPolicy = UnregisterPolicy.OnDisable;
		[SerializeField, Tooltip ("Determines sort order based on sibling index, instead of using the value set in the editor.")]
		private bool useAutoOrder;
		[SerializeField]
		private int order;


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

		public int Order => useAutoOrder ? transform.GetSiblingIndex () : order;

		public UIController Controller => controller;
		public UserInterfaceWidget Parent => parent;
		public bool HasParentControl => Parent != null || Controller != null;
		public int ChildCount => children.Count;


		private void OnEnable () {
			if (registerPolicy == RegisterPolicy.OnEnable) {
				Register ();
			}
		}

		private void Awake () {
			if (registerPolicy == RegisterPolicy.OnAwake) {
				Register ();
			}
		}

		private void Start () {
			if (registerPolicy == RegisterPolicy.OnStart) {
				Register ();
			}
		}

		private void OnDisable () {
			if (unregisterPolicy == UnregisterPolicy.OnDisable) {
				Unregister ();
			}
		}

		private void OnDestroy () {
			if (unregisterPolicy == UnregisterPolicy.OnDestroy) {
				Unregister ();
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

			string[] parts = path.Split (UserInterfaceController.PathSplitValues, System.StringSplitOptions.RemoveEmptyEntries);
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

		public bool TryGetChildComponentByID<T> (string identifier, out T value) where T : Component {
			value = null;
			if (children == null || !children.TryGetValue (identifier, out WidgetGroup group) || !group.TryGetFirst (out UserInterfaceWidget widget)) {
				return false;
			}
			value = widget.GetComponent<T> ();
			return value != null;
		}

		public bool TryGetChildComponentByID<T> (string identifier, int index, out T value) where T : Component {
			value = null;
			if (children == null || !children.TryGetValue (identifier, out WidgetGroup group) || !group.TryGet (index, out UserInterfaceWidget widget)) {
				return false;
			}
			value = widget.GetComponent<T> ();
			return value != null;
		}

		public bool TryGetChildComponentByPath<T> (string path, out T value) where T : Component {
			if (!TryGetChildWidgetByPath (path, out UserInterfaceWidget widget)) {
				value = null;
				return false;
			}
			value = widget.GetComponent<T> ();
			return value != null;
		}


		private void Register () {
			if (!Valid) {
				return;
			}
			if (parent != null || controller != null) {
				return;
			}

			FindParentControl ();
			if (controller != null) {
				controller.Register (this);
				return;
			}
			Log ($"Could not register widget {ToString ()}, because no controller was found.");
		}

		private void Unregister () {
			if (controller != null) {
				controller.Unregister (this);
			}
			if (parent != null) {
				parent.RemoveChild (this);
			}
		}

		internal void ForceParentControl (UIController parent, bool forceRegisterChildren = true) {
			if (!Valid) {
				return;
			}
			if (parent != null || controller != null) {
				return;
			}
			if (parent == null) {
				return;
			}

			controller = parent;
			parent.Register (this);
			if (forceRegisterChildren) {
				ForceRegisterChildren ();
			}
		}

		private void ForceRegisterChildren () {

			Stack<Transform> widgets = new Stack<Transform> ();
			widgets.Push (transform);

			do {

				Transform child = widgets.Pop ();
				if (child.TryGetComponent (out UserInterfaceWidget wid)) {
					wid.ForceParentControl (this);
				}

				if (child.childCount == 0) {
					continue;
				}

				foreach (Transform t in child) {
					widgets.Push (t);
				}

			} while (widgets.Count > 0);

		}

		private void ForceParentControl (UserInterfaceWidget parentWidget) {
			if (!Valid) {
				return;
			}
			if (parent != null || controller != null) {
				return;
			}
			if (parentWidget == null) {
				return;
			}

			parent = parentWidget;
			parent.AddChild (this);
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
				group = new WidgetGroup (widget.ID, widget);
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


		public IEnumerator<WidgetGroup> GetEnumerator () {
			return children.Values.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}

		int IComparable<UserInterfaceWidget>.CompareTo (UserInterfaceWidget other) {
			if (other == null) {
				return 1;
			}
			return Order.CompareTo (other.Order);
		}


		public override string ToString () {
			return $"\"{identifier}\"";
		}
	}
}