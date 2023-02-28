using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

namespace Zenvin.UI {
	/// <summary>
	/// Class for managing <see cref="UserInterfaceController"/>s. Should be placed on the root object of your User Interface.<br></br>
	/// Controllers will automatically register themselves with the manager on Start.<br></br><br></br>
	/// The <see cref="UserInterfaceManager"/> does <b>not</b> implement a singleton functionality!
	/// </summary>
	[DisallowMultipleComponent, DefaultExecutionOrder(-100)]
	public class UserInterfaceManager : MonoBehaviour, IEnumerable<UserInterfaceController> {

		private static bool log;

		private Dictionary<Type, UserInterfaceController> controllers = new Dictionary<Type, UserInterfaceController> ();

		[SerializeField] private bool debugLog = false;


		private void Awake () {
			log = debugLog;
		}


		/// <summary>
		/// Attempts to retrieve a <see cref="UserInterfaceController"/> of a given type.<br></br>
		/// Will return <c>false</c> if there is none registered with this manager.
		/// </summary>
		/// <typeparam name="T"> The type of controller to look for. </typeparam>
		public bool TryGetController<T> (out T controller) where T : UserInterfaceController {
			if (controllers == null) {
				controller = null;
				return false;
			}

			if (controllers.TryGetValue (typeof (T), out UserInterfaceController ctrl)) {
				controller = ctrl as T;
				return true;
			}

			controller = null;
			return false;
		}

		/// <summary>
		/// Retrieves a <see cref="UserInterfaceController"/> of a given type.<br></br>
		/// Will return <c>null</c> if there is none registered with this manager.
		/// </summary>
		/// <typeparam name="T"> The type of controller to look for. </typeparam>
		public T GetController<T> () where T : UserInterfaceController {
			if (TryGetController (out T ctrl)) {
				return ctrl;
			}
			return null;
		}

		/// <summary>
		/// Searches the local hierarchy for <see cref="UserInterfaceController"/>s and registers them with this manager.
		/// </summary>
		/// <param name="depth"></param>
		public int ForceRegisterControllers (bool forceRegisterWidgets = true, int depth = 1) {
			if (depth <= 0) {
				return 0;
			}

			foreach (Transform child in transform) {
				ForceRegisterControllersRecursively (child, forceRegisterWidgets, depth);
			}

			return controllers.Count;
		}

		private void ForceRegisterControllersRecursively (Transform parent, bool forceWidgets, int maxDepth, int currentDepth = 0) {
			if (parent == null || currentDepth >= maxDepth) {
				return;
			}

			if (parent.TryGetComponent(out UserInterfaceController ctrl)) {
				if (Register (ctrl) && forceWidgets) {
					ctrl.ForceRegisterElements (forceWidgets);
				}
				return;
			}

			foreach (Transform child in parent) {
				ForceRegisterControllersRecursively (child, forceWidgets, maxDepth, currentDepth + 1);
			}
		}


		internal bool Register<T> (T controller) where T : UserInterfaceController {
			if (controllers == null) {
				controllers = new Dictionary<Type, UserInterfaceController> ();
			}

			if (controller == null) {
				Log ("Cannot register controller <NULL>.");
				return false;
			}

			Type controllerType = typeof (T);
			if (!controllers.ContainsKey (controllerType)) {
				controllers.Add (controllerType, controller);
				Log ($"Registered controller {controller}.");
				return true;
			}

			Log ($"Did not register controller {controller}, as there already was one of the same type.");
			return false;
		}

		internal void Unregister<T> (T controller) where T : UserInterfaceController {
			if (controllers == null) {
				Log ("Cannot unregister controller <NULL>.");
				return;
			}

			if (controllers.TryGetValue (typeof (T), out UserInterfaceController ctrl) && controller == ctrl) {
				controllers.Remove (typeof (T));
				Log ($"Unregistered controller {controller}.");
				return;
			}

			Log ($"Did not unregister controller {controller}, as it was not registered.");
		}

		internal static void Log (string message) {
			if (log) {
				Debug.Log ("[UI] " + message);
			}
		}


		public IEnumerator<UserInterfaceController> GetEnumerator () {
			return controllers.Values.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}
	}
}