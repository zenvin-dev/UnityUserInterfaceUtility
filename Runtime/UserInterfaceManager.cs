using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

namespace Zenvin.UI {
	/// <summary>
	/// Class for managing <see cref="UserInterfaceControllerBase"/>s. Should be placed on the root object of your User Interface.<br></br>
	/// Controllers will automatically register themselves with the manager on Start, as long as the manager is in one of their parent objects.<br></br><br></br>
	/// The <see cref="UserInterfaceManager"/> does <b>not</b> implement a singleton functionality!
	/// </summary>
	[DisallowMultipleComponent]
	public class UserInterfaceManager : MonoBehaviour, IEnumerable<UserInterfaceControllerBase> {

		private Dictionary<Type, UserInterfaceControllerBase> controllers = new Dictionary<Type, UserInterfaceControllerBase> ();


		/// <summary>
		/// Attempts to retrieve a <see cref="UserInterfaceControllerBase"/> of a given type.<br></br>
		/// Will return <c>false</c> if there is none registered with this manager.
		/// </summary>
		/// <typeparam name="T"> The type of controller to look for. </typeparam>
		public bool TryGetController<T> (out T controller) where T : UserInterfaceControllerBase {
			if (controllers == null) {
				controller = null;
				return false;
			}

			if (controllers.TryGetValue (typeof (T), out UserInterfaceControllerBase ctrl)) {
				controller = ctrl as T;
				return true;
			}

			controller = null;
			return false;
		}

		/// <summary>
		/// Retrieves a <see cref="UserInterfaceControllerBase"/> of a given type.<br></br>
		/// Will return <c>null</c> if there is none registered with this manager.
		/// </summary>
		/// <typeparam name="T"> The type of controller to look for. </typeparam>
		public T GetController<T> () where T : UserInterfaceControllerBase {
			if (TryGetController (out T ctrl)) {
				return ctrl;
			}
			return null;
		}

		/// <summary>
		/// Searches the local hierarchy for <see cref="UserInterfaceControllerBase"/>s and registers them with this manager.
		/// </summary>
		/// <param name="depth"></param>
		public int ForceRegisterControllers (int depth = 1) {
			if (depth <= 0) {
				return 0;
			}
			controllers = new Dictionary<Type, UserInterfaceControllerBase> ();



			return controllers.Count;
		}


		internal void Register<T> (T controller) where T : UserInterfaceControllerBase {
			if (controllers == null) {
				controllers = new Dictionary<Type, UserInterfaceControllerBase> ();
			}

			if (controller == null)
				return;

			if (!controllers.ContainsKey (typeof (T))) {
				controllers.Add (typeof (T), controller);
			}
		}

		internal void Unregister<T> (T controller) where T : UserInterfaceControllerBase {
			if (controllers == null)
				return;


			if (controllers.TryGetValue (typeof (T), out UserInterfaceControllerBase ctrl) && controller == ctrl) {
				controllers.Remove (typeof (T));
			}
		}

		public IEnumerator<UserInterfaceControllerBase> GetEnumerator () {
			return controllers.Values.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}
	}
}