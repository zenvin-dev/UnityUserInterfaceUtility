using System;
using UnityEngine;

namespace Zenvin.UI.Layout {
	[Serializable]
	public class ColumnDefinition {
		[SerializeField] private CellSizeUnit unit;
		[SerializeField, Min (0)] private float width;

		public CellSizeUnit Unit { get => unit; internal set => unit = value; }
		public float Width { get => width; internal set => width = value; }
	}
}