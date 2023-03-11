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
		[SerializeField] private RectOffset margin = new RectOffset();

		/// <summary> The position of the Cell within its <see cref="GridLayout"/>. </summary>
		public Vector2Int Position {
			get => position;
			set {
				value.x = Mathf.Max (value.x, 0);
				value.y = Mathf.Max (value.y, 0);
				position = value;
				UpdateCell ();
			}
		}
		/// <summary> The number of grid cells that this Cell occupies in its <see cref="GridLayout"/>. </summary>
		public Vector2Int Span {
			get => span;
			set {
				value.x = Mathf.Max (value.x, 1);
				value.y = Mathf.Max (value.y, 1);
				span = value;
				UpdateCell ();
			}
		}
		/// <summary> The top margin of the cell object. </summary>
		public int Top {
			get => margin.top;
			set {
				margin.top = value;
				UpdateCell ();
			}
		}
		/// <summary> The bottom margin of the cell object. </summary>
		public int Bottom {
			get => margin.bottom;
			set {
				margin.bottom = value;
				UpdateCell ();
			}
		}
		/// <summary> The left margin of the cell object. </summary>
		public int Left {
			get => margin.left;
			set {
				margin.left = value;
				UpdateCell ();
			}
		}
		/// <summary> The right margin of the cell object. </summary>
		public int Right {
			get => margin.right;
			set {
				margin.right = value;
				UpdateCell ();
			}
		}


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

			r.x += margin.left;
			r.width -= margin.horizontal;
			
			t.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Left, r.x, r.width);
		}

		public void SetLayoutVertical () {
			if (!TryGetRect (out Rect r) || !TryGetRectTransform (out RectTransform t)) {
				return;
			}

			r.y += margin.top;
			r.height -= margin.vertical;

			t.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Top, r.y, r.height);
		}
	}
}