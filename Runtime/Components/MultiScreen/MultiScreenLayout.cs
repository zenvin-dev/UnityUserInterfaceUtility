using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.DrivenTransformProperties;

namespace Zenvin.UI.Components.MultiScreen {
	[RequireComponent (typeof (RectTransform)), DisallowMultipleComponent, ExecuteAlways, AddComponentMenu("UI/Zenvin/Multi-Screen Layout", 0)]
	public class MultiScreenLayout : UIBehaviour, ILayoutGroup {

		private Vector2Int currentCell = Vector2Int.one * -1;
		private Coroutine moveCoroutine;
		private Coroutine updateCoroutine;
		private Vector3 currentOffset;
		private RectTransform rt;
		private DrivenRectTransformTracker tracker;

		[SerializeField] private Vector2Int cells;
		[SerializeField] private Vector2Int startCell;
		[SerializeField] private float transitionDuration = 0.2f;
#if UNITY_EDITOR
		[Space, SerializeField] private Vector2Int editorCell;
#endif

		public bool IsTransitioning { get; private set; }
		public Vector2Int CurrentCell => currentCell;
		public int CellCount => cells.x * cells.y;

		private RectTransform ObjectTransform => rt == null ? (rt = transform as RectTransform) : rt;


		protected override void Start () {
			base.Start ();
			SnapToCell (startCell);
		}


		public void SnapDown (bool loop) {
			SnapToCell (GetDownCell (loop));
		}

		public void SnapUp (bool loop) {
			SnapToCell (GetUpCell (loop));
		}

		public void SnapRight (bool loop) {
			SnapToCell (GetRightCell (loop));
		}

		public void SnapLeft (bool loop) {
			SnapToCell (GetLeftCell (loop));
		}

		public void SnapToCell (Vector2Int cell) {
			cell = ClampCell (cell);
			if (cell != currentCell) {
				currentOffset = GetCellOffset (cell);
				currentCell = cell;

				if (moveCoroutine != null) {
					StopCoroutine (moveCoroutine);
					IsTransitioning = false;
				}
			}
		}


		public void TransitionDown (bool loop) {
			TransitionToCell (GetDownCell (loop));
		}

		public void TransitionUp (bool loop) {
			TransitionToCell (GetUpCell (loop));
		}

		public void TransitionRight (bool loop) {
			TransitionToCell (GetRightCell (loop));
		}

		public void TransitionLeft (bool loop) {
			TransitionToCell (GetLeftCell (loop));
		}

		public void TransitionToCell (Vector2Int cell) {
			cell = ClampCell (cell);
			if (cell == currentCell) {
				return;
			}
			currentCell = cell;
			if (moveCoroutine != null) {
				StopCoroutine (moveCoroutine);
			}
			moveCoroutine = StartCoroutine (DoTransitionToCell (cell));
		}


		public Vector2Int GetDownCell (bool loop) {
			Vector2Int cell = currentCell;
			cell.y++;
			if (cell.y >= cells.y) {
				cell.y = loop ? 0 : cells.y - 1;
			}
			return cell;
		}

		public Vector2Int GetUpCell (bool loop) {
			Vector2Int cell = currentCell;
			cell.y--;
			if (cell.y < 0) {
				cell.y = loop ? cells.y : 0;
			}
			return cell;
		}

		public Vector2Int GetRightCell (bool loop) {
			Vector2Int cell = currentCell;
			cell.x++;
			if (cell.x >= cells.x) {
				cell.x = loop ? 0 : cells.x - 1;
			}
			return cell;
		}

		public Vector2Int GetLeftCell (bool loop) {
			Vector2Int cell = currentCell;
			cell.x--;
			if (cell.x < 0) {
				cell.x = loop ? cells.x : 0;
			}
			return cell;
		}

		public WaitForTransition Wait () {
			return new WaitForTransition (this);
		}


		private IEnumerator DoTransitionToCell (Vector2Int cell) {
			var startPos = currentOffset;
			var t = 0f;

			IsTransitioning = true;
			while (t <= 1f) {
				var targetPos = GetCellOffset (cell);
				currentOffset = Vector3.Lerp (startPos, targetPos, t);
				SetDirty (false);
				t += Time.deltaTime / transitionDuration;
				yield return null;
			}

			currentOffset = GetCellOffset (cell);
			SetDirty (true);

			IsTransitioning = false;
		}

		private IEnumerator DoDelayedLayoutUpdate (RectTransform rt) {
			yield return null;
			LayoutRebuilder.MarkLayoutForRebuild (rt);
			updateCoroutine = null;
		}


		private Vector3 GetCellOffset (Vector2Int cell) {
			var size = GetRectSize ();
			cell = ClampCell (cell);
			var offset = new Vector3 (cell.x * size.x, cell.y * size.y);
			return offset;
		}

		private Vector2 GetRectSize () {
			return ObjectTransform.rect.size;
		}

		public Vector2 GetFullSize () {
			Vector2 size = GetRectSize ();
			size.Scale (cells);
			return size;
		}

		private Vector2Int ClampCell (Vector2Int cell) {
			cell.x = Mathf.Clamp (cell.x, 0, cells.x - 1);
			cell.y = Mathf.Clamp (cell.y, 0, cells.y - 1);
			return cell;
		}

		private Vector2Int IndexToCell (int index) {
			return new Vector2Int (Mathf.RoundToInt (index % (float)cells.x), Mathf.RoundToInt (index / (float)cells.y));
		}

		private void SetChildHorizontal (RectTransform rt, float position, float size) {
			if (rt == null || rt.parent != transform) {
				return;
			}

			tracker.Add (this, rt, AnchoredPositionX | PivotX | SizeDeltaX | AnchorMinX | AnchorMaxX);

			rt.anchorMin = new Vector2 (0f, 1f);
			rt.anchorMax = new Vector2 (0f, 1f);
			rt.pivot = new Vector2 (0f, 1f);

			//var sizeDelta = new Vector2 (size, rt.sizeDelta.y);
			//rt.sizeDelta = sizeDelta;

			//var pos = rt.anchoredPosition;
			//pos.x = position;
			//rt.anchoredPosition = pos;

			rt.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Left, position, size);
		}

		private void SetChildVertical (RectTransform rt, float position, float size) {
			if (rt == null || rt.parent != transform) {
				return;
			}

			tracker.Add (this, rt, AnchoredPositionY | PivotY | SizeDeltaY | AnchorMinY | AnchorMaxY);

			rt.anchorMin = new Vector2 (0f, 1f);
			rt.anchorMax = new Vector2 (0f, 1f);
			rt.pivot = new Vector2 (0f, 1f);

			//var sizeDelta = new Vector2 (size, rt.sizeDelta.y);
			//rt.sizeDelta = sizeDelta;

			//var pos = rt.anchoredPosition;
			//pos.y = position;
			//rt.anchoredPosition = pos;

			rt.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Top, position, size);
		}


		private void SetDirty (bool forceDelayed = true) {
			if (!IsActive ()) {
				return;
			}
			if (!CanvasUpdateRegistry.IsRebuildingLayout ()) {
				LayoutRebuilder.MarkLayoutForRebuild (ObjectTransform);
			} else if (updateCoroutine == null && forceDelayed) {
				updateCoroutine = StartCoroutine (DoDelayedLayoutUpdate (ObjectTransform));
			}
		}


		protected override void OnCanvasHierarchyChanged () {
			base.OnCanvasHierarchyChanged ();
			SetDirty ();
		}

		protected override void OnDidApplyAnimationProperties () {
			base.OnDidApplyAnimationProperties ();
			SetDirty ();
		}

		protected override void OnRectTransformDimensionsChange () {
			base.OnRectTransformDimensionsChange ();
			SetDirty ();
		}

#if UNITY_EDITOR
		private void Update () {
			if (Application.isPlaying) {
				return;
			}
			editorCell = ClampCell (editorCell);
			var current = currentCell;
			SnapToCell (editorCell);
			if (current != currentCell) {
				SetDirty ();
			}
		}

		protected override void OnValidate () {
			base.OnValidate ();

			cells.x = Mathf.Max (cells.x, 1);
			cells.y = Mathf.Max (cells.y, 1);

			startCell = ClampCell (startCell);
		}
#endif


		void ILayoutController.SetLayoutHorizontal () {
			tracker.Clear ();

			var count = Mathf.Min (transform.childCount, CellCount);
			var size = GetRectSize ();

			for (int i = 0; i < count; i++) {
				var cell = IndexToCell (i);
				var off = GetCellOffset (cell);
				var pos = -currentOffset + off;
				SetChildHorizontal (transform.GetChild (i) as RectTransform, pos.x, size.x);
			}
		}

		void ILayoutController.SetLayoutVertical () {
			var count = Mathf.Min (transform.childCount, CellCount);
			var size = GetRectSize ();

			for (int i = 0; i < count; i++) {
				var cell = IndexToCell (i);
				var off = GetCellOffset (cell);
				var pos = -currentOffset + off;
				SetChildVertical (transform.GetChild (i) as RectTransform, pos.y, size.y);
			}
		}


		public class WaitForTransition : CustomYieldInstruction {
			public readonly MultiScreenLayout Layout;

			public override bool keepWaiting => Layout != null && Layout.IsTransitioning;


			public WaitForTransition (MultiScreenLayout layout) {
				this.Layout = layout;
			}
		}
	}
}