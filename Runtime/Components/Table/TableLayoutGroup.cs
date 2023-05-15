using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenvin.UI.Layout;
using static UnityEngine.DrivenTransformProperties;
using static Zenvin.UI.Layout.LayoutUtility;

namespace Zenvin.UI.Components.Table {
	[RequireComponent (typeof (RectTransform)), ExecuteAlways, AddComponentMenu ("UI/Zenvin/Table Layout", 0)]
	public class TableLayoutGroup : UIBehaviour, ILayoutGroup, ILayoutElement {

		private readonly List<TableElement> rowObjects = new List<TableElement> ();

		private Coroutine updateCoroutine;
		private float[] columnSizes;
		private Vector2 totalMinSize;
		private Vector2 totalPreferredSize;
		private Vector2 totalFlexibleSize;
		private DrivenRectTransformTracker tracker;
		private RectTransform rectTransform;

		[SerializeField] private float rowSpacing = 0f;
		[SerializeField] private List<ColumnDefinition> columns = new List<ColumnDefinition> ();
		[SerializeField, Tooltip ("Will ignore children whose (layout-influenced) child count does not equal the number of columns.")] private bool ignoreMismatchedRows = true;


		public int ColumnCount => columns.Count;
		public RectTransform GroupRectTransform => rectTransform == null ? (rectTransform = transform as RectTransform) : rectTransform;

		float ILayoutElement.minWidth => totalMinSize.x;
		float ILayoutElement.preferredWidth => totalPreferredSize.x;
		float ILayoutElement.flexibleWidth => totalFlexibleSize.x;
		float ILayoutElement.minHeight => totalMinSize.y;
		float ILayoutElement.preferredHeight => totalPreferredSize.y;
		float ILayoutElement.flexibleHeight => totalFlexibleSize.y;
		int ILayoutElement.layoutPriority => 0;


		public void ForceUpdateLayout () {
			SetDirty ();
		}


		protected override void OnDidApplyAnimationProperties () {
			SetDirty ();
		}

		protected override void OnEnable () {
			base.OnEnable ();
			SetDirty ();
		}

		protected override void OnDisable () {
			tracker.Clear ();
			LayoutRebuilder.MarkLayoutForRebuild (GroupRectTransform);
			base.OnDisable ();
		}


		void ILayoutElement.CalculateLayoutInputHorizontal () {
			UpdateRectChildren ();
		}

		void ILayoutElement.CalculateLayoutInputVertical () {
			// interface method
		}

		void ILayoutController.SetLayoutHorizontal () {
			UpdateLayoutHorizontal ();
		}

		void ILayoutController.SetLayoutVertical () {
			UpdateLayoutVertical ();
		}


		private void UpdateLayoutHorizontal () {
			Vector2 size = GroupRectTransform.rect.size;
			CalculateColumnWidths (columns, size, 0f, ref columnSizes);

			for (int i = 0; i < rowObjects.Count; i++) {
				var el = rowObjects[i];

				if (el.Row == null) {
					continue;
				}

				SetChildAlongAxisWithScale (el.Row, 0, 0f, size.x);

				float offset = 0f;
				for (int j = 0; j < el.Columns.Count; j++) {
					var child = el.Columns[j];
					if (child == null) {
						continue;
					}

					offset += SetChildAlongAxisWithScale (child, 0, offset, columnSizes[j], 1f, true, false);
				}
			}
		}

		private void UpdateLayoutVertical () {
			float offset = 0f;
			for (int i = 0; i < rowObjects.Count; i++) {
				var el = rowObjects[i];

				if (el.Row == null) {
					continue;
				}

				offset += SetChildAlongAxisWithScale (el.Row, 1, offset) + rowSpacing;
			}

			totalPreferredSize = new Vector2 (GroupRectTransform.rect.width, offset);
		}

		private float SetChildAlongAxisWithScale (RectTransform rect, int axis, float pos, float? size = null, float scale = 1f, bool controlAnchorX = true, bool controlAnchorY = true) {
			if (rect == null || (axis != 0 && axis != 1)) {
				return 0f;
			}

			tracker.Add (
				this,
				rect,
				(controlAnchorX ? AnchorMinX | AnchorMaxX : 0) |
				(controlAnchorY ? AnchorMinY | AnchorMaxY : 0) |
				(axis == 0 ? AnchoredPositionX | (size.HasValue ? SizeDeltaX : 0) : AnchoredPositionY |
				(size.HasValue ? SizeDeltaY : 0))
			);

			rect.anchorMin = GetManipulatedVector (rect.anchorMin, Vector2.up, controlAnchorX, controlAnchorY);
			rect.anchorMax = GetManipulatedVector (rect.anchorMax, Vector2.up, controlAnchorX, controlAnchorY);

			if (size.HasValue) {
				Vector2 sizeDelta = rect.sizeDelta;
				sizeDelta[axis] = size.Value;
				rect.sizeDelta = sizeDelta;
			} else {
				size = rect.sizeDelta[axis];
			}

			Vector2 anchoredPosition = rect.anchoredPosition;
			anchoredPosition[axis] = (axis == 0) ? (pos + size.Value * rect.pivot[axis] * scale) : (-pos - size.Value * (1f - rect.pivot[axis]) * scale);
			rect.anchoredPosition = anchoredPosition;

			return size.Value;
		}

		private Vector3 GetManipulatedVector (Vector2 original, Vector2 target, bool changeX, bool changeY) {
			if (changeX) {
				original.x = target.x;
			}
			if (changeY) {
				original.y = target.y;
			}
			return original;
		}

		private void UpdateRectChildren () {
			rowObjects.Clear ();

			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++) {

				var rect = transform.GetChild (i) as RectTransform;
				if (rect == null || !rect.gameObject.activeInHierarchy) {
					continue;
				}

				if (rect.TryGetComponent (out ILayoutIgnorer ignorer) && ignorer.ignoreLayout) {
					continue;
				}

				var subChildCount = rect.childCount;
				if (ignoreMismatchedRows && subChildCount < ColumnCount) {
					continue;
				}

				var element = new TableElement (rect);
				for (int j = 0; j < subChildCount; j++) {
					if (element.Columns.Count >= ColumnCount) {
						break;
					}

					var cellRect = rect.GetChild (j) as RectTransform;
					if (cellRect == null || !cellRect.gameObject.activeInHierarchy) {
						continue;
					}

					if (cellRect.TryGetComponent (out ignorer) && ignorer.ignoreLayout) {
						continue;
					}

					element.Columns.Add (cellRect);
				}

				if (element.Columns.Count == ColumnCount || !ignoreMismatchedRows) {
					rowObjects.Add (element);
				}
			}

			tracker.Clear ();
		}

		private void SetDirty () {
			if (!IsActive ()) {
				return;
			}
			if (!CanvasUpdateRegistry.IsRebuildingLayout ()) {
				LayoutRebuilder.MarkLayoutForRebuild (GroupRectTransform);
			} else if (updateCoroutine == null) {
				updateCoroutine = StartCoroutine (DoDelayedLayoutUpdate (GroupRectTransform));
			}
		}


		private IEnumerator DoDelayedLayoutUpdate (RectTransform rt) {
			yield return null;
			LayoutRebuilder.MarkLayoutForRebuild (rt);
			updateCoroutine = null;
		}

#if UNITY_EDITOR
		private void Update () {
			if (Application.isPlaying) {
				return;
			}

			int count = rowObjects.Count;
			UpdateRectChildren ();
			if (count != rowObjects.Count) {
				SetDirty ();
			}
		}

		protected override void OnValidate () {
			base.OnValidate ();
			SetDirty ();
		}
#endif


		private struct TableElement {
			public readonly RectTransform Row;
			public readonly List<RectTransform> Columns;


			public TableElement (RectTransform row) {
				Row = row;
				Columns = new List<RectTransform> ();
			}

			public TableElement (RectTransform row, int columnCount) {
				Row = row;
				Columns = new List<RectTransform> (columnCount);
			}
		}
	}
}