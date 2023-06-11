using UnityEngine;

namespace Zenvin.UI.Components.ValueComparison {
	[AddComponentMenu ("UI/Zenvin/Sourced Comparison Table", 0)]
	public class SourcedComparisonTable : ComparisonTableBase {

		private IComparisonSource source;

		public IComparisonSource Source { get => source; set => SetSource (value); }



		private void SetSource (IComparisonSource value) {
			if (source == value) {
				return;
			}
			if (source == null) {
				ClearTable ();
				return;
			}
			source.ContentChanged -= UpdateTableContent;
			source = value;
			if (source != null) {
				UpdateTable (source.GetContent ());
				source.ContentChanged += UpdateTableContent;
			} else {
				ClearTable ();
			}
		}

		public void UpdateTableContent () {
			if (source == null) {
				ClearTable ();
				return;
			}
			var content = source.GetContent ();
			UpdateTable (content);
		}
	}
}