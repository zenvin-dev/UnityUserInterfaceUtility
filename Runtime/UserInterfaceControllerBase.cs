using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.UI {
	/// <summary>
	/// A base class for custom UI Controllers.<br></br>
	/// Contains functionality for managing and interacting with <see cref="UserInterfaceWidget"/>s.
	/// </summary>
	[DisallowMultipleComponent]
	public abstract class UserInterfaceControllerBase : MonoBehaviour, IEnumerable<WidgetGroup> {

		public const char PathSeparator = '/';
		internal static readonly string[] PathSplitValues = new string[] { PathSeparator.ToString () };
		public const char IndexSeparator = '|';
		internal static readonly string[] IndexSplitValues = new string[] { IndexSeparator.ToString () };

		private Dictionary<string, WidgetGroup> elements = new Dictionary<string, WidgetGroup> ();


		private void Start () {
			if (transform.root.TryGetComponent (out UserInterfaceManager uiManager)) {
				uiManager.Register (this);
			}
		}


		public bool TryGetByID<T> (string identifier, out T reference) where T : Component {
			if (elements.TryGetValue (identifier, out WidgetGroup uiRef) && uiRef.TryGetFirst (out UserInterfaceWidget wid)) {
				reference = wid.GetComponent<T> ();
				return reference != null;
			}

			reference = null;
			return false;
		}

		public T GetByID<T> (string identifier) where T : Component {
			if (TryGetByID (identifier, out T value)) {
				return value;
			}
			return null;
		}

		public bool TryGetComponentByPath<T> (string path, out T reference) where T : Component {
			if (string.IsNullOrEmpty (path)) {
				reference = null;
				return false;
			}

			string[] split = path.Split (PathSplitValues, System.StringSplitOptions.RemoveEmptyEntries);
			if (split.Length < 2) {
				return TryGetByID (split[0], out reference);
			}

			PathPartSplit (split[0], out string singlePart, out int widgetIndex);
			UserInterfaceWidget current;
			if (!elements.TryGetValue (singlePart, out WidgetGroup group) || !group.TryGet (widgetIndex, out current)) {
				reference = null;
				return false;
			}

			for (int i = 1; i < split.Length; i++) {
				PathPartSplit (split[i], out singlePart, out widgetIndex);
				if (!current.TryGetChildComponent (singlePart, widgetIndex, out current)) {
					reference = null;
					return false;
				}
			}

			reference = current?.GetComponent<T> ();
			return reference != null;
		}

		public T GetByPath<T> (string path) where T : Component {
			if (TryGetComponentByPath (path, out T reference)) {
				return reference;
			}
			return null;
		}


		internal void Register (UserInterfaceWidget reference) {
			if (elements == null) {
				elements = new Dictionary<string, WidgetGroup> ();
			}

			if (reference == null || !reference.Valid)
				return;

			if (elements.TryGetValue (reference.ID, out WidgetGroup group)) {
				group.Add (reference);
			} else {
				group = new WidgetGroup ();
				group.Add (reference);
				elements[reference.ID] = group;
			}
		}

		internal void Unregister (UserInterfaceWidget reference) {
			if (reference == null || !reference.Valid)
				return;


			if (elements.TryGetValue (reference.ID, out WidgetGroup comp)) {
				if (comp.Remove (reference, out bool empty) && empty) {
					elements.Remove (reference.ID);
				}
			}
		}


		internal static void PathPartSplit (string value, out string part, out int index) {
			if (value == null || value.Length == 0 || !value.Contains (IndexSeparator.ToString ())) {
				part = value;
				index = 0;
				return;
			}

			string[] parts = value.Split (IndexSplitValues, System.StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) {
				part = value;
				index = 0;
				return;
			}
			if (!int.TryParse (parts[1], out index)) {
				index = 0;
			}
			part = parts[0];
		}


		public IEnumerator<WidgetGroup> GetEnumerator () {
			return elements.Values.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}
	}
}