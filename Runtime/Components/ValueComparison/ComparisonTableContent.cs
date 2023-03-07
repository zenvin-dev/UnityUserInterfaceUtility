using System.Collections.Generic;

namespace Zenvin.UI.Components.ValueComparison {
	public sealed class ComparisonTableContent {

		private readonly List<ComparisonValue> rows = new List<ComparisonValue> ();
		private string leftHeader;
		private string rightHeader;


		public string LeftHeader {
			get => leftHeader;
			set {
				if (leftHeader == value) {
					return;
				}
				leftHeader = value;
			}
		}
		public string RightHeader {
			get => rightHeader;
			set {
				if (rightHeader == value) {
					return;
				}
				rightHeader = value;
			}
		}
		public int RowCount => rows.Count;
		public ComparisonValue this[int index] => rows[index];


		public void AddValue (ComparisonValue value) {
			if (value != null) {
				rows.Add (value);
			}
		}

		public void RemoveValue (int index) {
			rows.RemoveAt (index);
		}
	}
}