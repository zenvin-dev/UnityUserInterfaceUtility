using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Zenvin.UI.UserInterfaceManager;

namespace Zenvin.UI {
	/// <summary>
	/// A base class for custom UI Controllers.<br></br>
	/// Contains functionality for managing and interacting with <see cref="UserInterfaceWidget"/>s.
	/// </summary>
	[DisallowMultipleComponent]
	public class UserInterfaceController : MonoBehaviour, IEnumerable<WidgetGroup> {

		public const char PathSeparator = '/';
		internal static readonly string[] PathSplitValues = new string[] { PathSeparator.ToString () };
		public const char IndexSeparator = '|';
		internal static readonly string[] IndexSplitValues = new string[] { IndexSeparator.ToString () };

		private Dictionary<string, WidgetGroup> elements = new Dictionary<string, WidgetGroup> ();


		private void Start () {
			if (transform.root.TryGetComponent (out UserInterfaceManager uiManager)) {
				uiManager.Register (this);
			} else {
				Transform current = transform;
				while (current != null && uiManager == null) {
					if (current.TryGetComponent (out uiManager)) {
						uiManager.Register (this);
					}
					current = current.parent;
				}
			}
		}


		/// <summary>
		/// Tries to get (a component from) the first <see cref="UserInterfaceWidget"/> with a matching <paramref name="identifier"/>, that is registered to the Controller.
		/// </summary>
		public bool TryGetByID<T> (string identifier, out T reference) where T : Component {
			PathPartSplit (identifier, out string part, out int index);
			if (elements.TryGetValue (part, out WidgetGroup uiRef) && uiRef.TryGet (index, out UserInterfaceWidget wid)) {
				reference = wid.GetComponent<T> ();
				return reference != null;
			}

			reference = null;
			return false;
		}

		/// <summary>
		/// Gets (a component from) the first <see cref="UserInterfaceWidget"/> with a matching <paramref name="identifier"/>, that is registered to the Controller.
		/// </summary>
		public T GetByID<T> (string identifier) where T : Component {
			if (TryGetByID (identifier, out T value)) {
				return value;
			}
			return null;
		}

		/// <summary>
		/// Tries to get (a component from) the <see cref="UserInterfaceWidget"/> with a matching <paramref name="path"/>, that is registered to the Controller.<br></br>
		/// Other than with <see cref="TryGetByID{T}(string, out T)"/>, paths allow finding nested elements.
		/// <br></br><br></br>
		/// Separate identifiers using a forward slash (<c>/</c>).<br></br>
		/// If there are multiple elements of the same name on the same level, use a pipe (<c>|</c>) followed by a number to specify an index.
		/// <br></br><br></br>
		/// Example path:<br></br>
		/// <code>
		/// player_hud/stats_display/bar|2
		/// </code>
		/// </summary>
		public bool TryGetComponentByPath<T> (string path, out T reference) where T : Component {
			if (TryGetWidgetByPath(path, out UserInterfaceWidget widget) && widget.TryGetComponent(out reference)) {
				return true;
			}
			reference = null;
			return false;
		}

		/// <summary>
		/// Tries to get the <see cref="UserInterfaceWidget"/> with a matching <paramref name="path"/>, that is registered to the Controller.<br></br>
		/// Other than with <see cref="TryGetByID{T}(string, out T)"/>, paths allow finding nested elements.
		/// <br></br><br></br>
		/// Separate identifiers using a forward slash (<c>/</c>).<br></br>
		/// If there are multiple elements of the same name on the same level, use a pipe (<c>|</c>) followed by a number to specify an index.
		/// <br></br><br></br>
		/// Example path:<br></br>
		/// <code>
		/// player_hud/stats_display/bar|2
		/// </code>
		/// </summary>
		public bool TryGetWidgetByPath (string path, out UserInterfaceWidget reference) {
			if (string.IsNullOrEmpty (path)) {
				reference = null;
				return false;
			}

			string[] split = path.Split (PathSplitValues, System.StringSplitOptions.RemoveEmptyEntries);
			if (split.Length == 0) {
				reference = null;
				return false;
			}

			if (split.Length == 1) {
				return TryGetByID (split[0], out reference);
			}

			UserInterfaceWidget current;

			PathPartSplit (split[0], out string groupIdentifier, out int indexInGroup);
			if (!elements.TryGetValue (groupIdentifier, out WidgetGroup group) || !group.TryGet (indexInGroup, out current)) {
				reference = null;
				return false;
			}

			for (int i = 1; i < split.Length; i++) {
				PathPartSplit (split[i], out string widIdentifier, out int widIndex);
				if (!current.TryGetChildWidget (widIdentifier, widIndex, out current)) {
					break;
				}
			}

			reference = current;
			return reference != null;
		}

		/// <summary>
		/// Gets the <see cref="UserInterfaceWidget"/> with a matching <paramref name="path"/>, that is registered to the Controller.<br></br>
		/// Other than with <see cref="TryGetByID{T}(string, out T)"/>, paths allow finding nested elements.
		/// <br></br><br></br>
		/// Separate identifiers using a forward slash (<c>/</c>).<br></br>
		/// If there are multiple elements of the same name on the same level, use a pipe (<c>|</c>) followed by a number to specify an index.
		/// <br></br><br></br>
		/// Example path:<br></br>
		/// <code>
		/// player_hud/stats_display/bar|2
		/// </code>
		/// </summary>
		public T GetComponentByPath<T> (string path) where T : Component {
			if (TryGetComponentByPath (path, out T reference)) {
				return reference;
			}
			return null;
		}


		internal void ForceRegisterElements (bool forceWidgets) {
			ForceRegisterWidgetsRecursively (transform, forceWidgets);
		}

		private void ForceRegisterWidgetsRecursively (Transform parent, bool forceWidgets) {
			if (parent == null) {
				return;
			}
			if (parent.TryGetComponent (out UserInterfaceWidget widget)) {
				Register (widget);
			}

			foreach (Transform child in parent) {
				if (child.TryGetComponent (out UserInterfaceController ctrl)) {
					ctrl.ForceRegisterElements (forceWidgets);
					continue;
				}
				if (forceWidgets && parent.TryGetComponent (out widget)) {
					widget.ForceParentControl (this, true);
				}
				ForceRegisterWidgetsRecursively (parent, forceWidgets);
			}
		}


		internal void Register (UserInterfaceWidget reference) {
			if (elements == null) {
				elements = new Dictionary<string, WidgetGroup> ();
			}

			if (reference == null || !reference.Valid) {
				Log ($"Did not register widget {reference} in controller {ToString ()}, because it was not valid.");
				return;
			}

			if (elements.TryGetValue (reference.ID, out WidgetGroup group)) {
				group.Add (reference);
			} else {
				group = new WidgetGroup (reference.ID);
				group.Add (reference);
				elements[reference.ID] = group;
			}
			Log ($"Registered widget {reference} in controller {ToString ()}. Index in Group: {group.Count - 1}");
		}

		internal void Unregister (UserInterfaceWidget reference) {
			if (reference == null || !reference.Valid) {
				Log ($"Did not unregister widget {reference} from controller {ToString ()}, because it was not valid.");
				return;
			}

			if (elements.TryGetValue (reference.ID, out WidgetGroup comp)) {
				if (comp.Remove (reference, out bool empty) && empty) {
					elements.Remove (reference.ID);
				}
			}
			Log ($"Unregistered widget {reference} from controller {ToString ()}.");
		}


		internal static void PathPartSplit (string value, out string part, out int index) {
			if (value == null || value.Length == 0) {
				part = value;
				index = 0;
				return;
			}

			int i;
			bool endsInNumber = true;
			for (i = value.Length - 1; i >= 0; i--) {
				char c = value[i];
				if (c == IndexSeparator) {
					break;
				}
				if (!char.IsNumber (c)) {
					endsInNumber = false;
					break;
				}
			}

			if (endsInNumber) {
				string indexString = value.Substring (i + 1);

				part = value.Substring (0, i);
				int.TryParse (indexString, out index);
				return;
			}

			index = 0;
			part = value;
		}


		public IEnumerator<WidgetGroup> GetEnumerator () {
			return elements.Values.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}


		public override string ToString () {
			return $"\"{name}\" ({GetType ().FullName})";
		}
	}
}