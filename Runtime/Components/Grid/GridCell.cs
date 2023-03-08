using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zenvin.UI.Components.Grid {
	[DisallowMultipleComponent, ExecuteInEditMode, RequireComponent (typeof (RectTransform))]
	public sealed class GridCell : UIBehaviour, ILayoutSelfController {

		private GridLayout grid;
		private RectTransform rt;

		[SerializeField] private Vector2Int position;
		[SerializeField] private Vector2Int span = Vector2Int.one;


		internal void UpdateCell () {
			SetLayoutHorizontal ();
			SetLayoutVertical ();
		}


		protected override void OnValidate () {
			base.OnValidate ();
			UpdateCell ();
		}

		private bool TryGetGrid (out GridLayout grid) {
			if (this.grid != null) {
				grid = this.grid;
				return grid != null;
			}
			if (transform.parent != null && transform.parent.TryGetComponent (out grid)) {
				this.grid = grid;
				return true;
			}
			grid = null;
			return false;
		}

		private bool TryGetRectTransform (out RectTransform rt) {
			if (this.rt != null) {
				rt = this.rt;
				return true;
			}
			rt = transform as RectTransform;
			return rt != null;
		}

		private bool TryGetRect (out Rect rect) {
			if (TryGetGrid (out GridLayout g)) {
				rect = g.GetRect (position, span);
				return true;
			}
			rect = default;
			return false;
		}


		public void SetLayoutHorizontal () {
			if (!TryGetRect (out Rect r) || !TryGetRectTransform (out RectTransform t)) {
				return;
			}
			t.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Left, r.x, r.width);
		}

		public void SetLayoutVertical () {
			if (!TryGetRect (out Rect r) || !TryGetRectTransform (out RectTransform t)) {
				return;
			}
			t.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Top, r.y, r.height);
		}
	}
}