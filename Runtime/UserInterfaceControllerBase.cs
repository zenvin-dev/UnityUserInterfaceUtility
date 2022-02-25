using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.UI {
	/// <summary>
	/// A base class for custom UI Controllers.<br></br>
	/// Contains functionality for managing and interacting with <see cref="UserInterfaceWidget"/>s.
	/// </summary>
	public abstract class UserInterfaceControllerBase : MonoBehaviour {

		public const char PathSeparator = '/';
		internal static readonly string[] PathSplitValues = new string[] { PathSeparator.ToString () };

		private Dictionary<string, UserInterfaceWidget> elements = new Dictionary<string, UserInterfaceWidget> ();


		private void Start () {
			if (transform.root.TryGetComponent (out UserInterfaceManager uiManager)) {
				uiManager.Register (this);
			}
		}


		public bool TryGetByID<T> (string identifier, out T reference) where T : Component {
			if (elements.TryGetValue (identifier, out UserInterfaceWidget uiRef)) {
				reference = uiRef.GetComponent<T> ();
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

		public bool TryGetByPath<T> (string path, out T reference) where T : Component {
			if (string.IsNullOrEmpty (path)) {
				Debug.Log ("Invalid Path");
				reference = null;
				return false;
			}

			string[] split = path.Split (PathSplitValues, System.StringSplitOptions.RemoveEmptyEntries);
			if (split.Length < 2) {
				Debug.Log ("Simplistic Path");
				return TryGetByID (split[0], out reference);
			}

			UserInterfaceWidget current;
			if (!elements.TryGetValue (split[0], out current)) {
				Debug.Log ($"No Widget '{split[0]}' found.");
				reference = null;
				return false;
			}

			for (int i = 1; i < split.Length; i++) {
				if (!current.TryGetChildComponent (split[i], out current)) {
					Debug.Log ($"No Child Widget '{split[i]}' found.");
					reference = null;
					return false;
				}
			}

			reference = current?.GetComponent<T> ();
			return reference != null;
		}

		public T GetByPath<T> (string path) where T : Component {
			if (TryGetByPath (path, out T reference)) {
				return reference;
			}
			return null;
		}


		internal void Register (UserInterfaceWidget reference) {
			if (elements == null) {
				elements = new Dictionary<string, UserInterfaceWidget> ();
			}

			if (reference == null || !reference.Valid)
				return;

			if (!elements.ContainsKey (reference.ID)) {
				elements.Add (reference.ID, reference);
			}
		}

		internal void Unregister (UserInterfaceWidget reference) {
			if (reference == null || !reference.Valid)
				return;


			if (elements.TryGetValue (reference.ID, out UserInterfaceWidget comp) && reference == comp) {
				elements.Remove (reference.ID);
			}
		}


		/// <summary>
		/// A helper class for grouping <see cref="UserInterfaceWidget"/>s with the same identifier.
		/// </summary>
		public class WidgetGroup {

			private readonly List<UserInterfaceWidget> widgets = new List<UserInterfaceWidget> ();


			public int Count => widgets.Count;
			public UserInterfaceWidget this[int index] => index >= 0 && index < Count ? widgets[index] : null;


			public void Add (UserInterfaceWidget widget) {
				if (widget == null) {
					return;
				}
				widgets.Add (widget);
			}

			public bool Remove (UserInterfaceWidget widget, out bool empty) {
				bool rem = widgets.Remove (widget);
				empty = Count == 0;
				return rem;
			}


			public static implicit operator UserInterfaceWidget (WidgetGroup group) {
				if (group == null) {
					return null;
				}
				return group[0];
			}

		}

	}
}