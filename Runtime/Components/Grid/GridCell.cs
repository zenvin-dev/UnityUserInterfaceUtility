using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zenvin.UI.Components.Grid {
	[DisallowMultipleComponent, RequireComponent (typeof (RectTransform))]
	public sealed class GridCell : UIBehaviour, ILayoutSelfController {

		private GridLayout grid;
		private RectTransform rt;

		[SerializeField] private Vector2Int position;
		[SerializeField] private Vector2Int span = Vector2Int.one;
		[SerializeField] private RectOffset margin = new RectOffset ();

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
			if (!TryGetRect (out Rect r)) {
				return;
			}
			SetLayoutHorizontal (r);
		}

		public void SetLayoutVertical () {
			if (!TryGetRect (out Rect r)) {
				return;
			}
			SetLayoutVertical (r);
		}

		internal void SetLayoutHorizontal (Rect rect) {
			rect.x += margin.left;
			rect.width -= margin.horizontal;

			(transform as RectTransform).SetInsetAndSizeFromParentEdge (RectTransform.Edge.Left, rect.x, rect.width);
		}

		internal void SetLayoutVertical (Rect rect) {
			rect.y += margin.top;
			rect.height -= margin.vertical;

			(transform as RectTransform).SetInsetAndSizeFromParentEdge (RectTransform.Edge.Top, rect.y, rect.height);
		}
	}
}