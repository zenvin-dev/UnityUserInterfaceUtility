using System;
using UnityEngine;

namespace Zenvin.UI.Layout {
	[Serializable]
	public class RowDefinition {
		[SerializeField] private CellSizeUnit unit;
		[SerializeField, Min (0)] private float height;

		public CellSizeUnit Unit { get => unit; internal set => unit = value; }
		public float Height { get => height; internal set => height = value; }
	}
}