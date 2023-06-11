using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zenvin.UI.Components.ValueComparison {
	[AddComponentMenu("UI/Zenvin/Comparison Table", 0)]
	public class ComparisonTable : ComparisonTableBase {

		private ComparisonTableContent content;


		public ComparisonTableContent Content {
			get => content;
			set {
				if (content != value) {
					content = value;
					UpdateTableContent ();
				}
			}
		}


		public void UpdateTableContent () {
			UpdateTable (Content);
		}

	}
}