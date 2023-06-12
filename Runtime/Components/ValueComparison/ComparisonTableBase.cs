using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Zenvin.UI.Components.ValueComparison {
	public abstract class ComparisonTableBase : MonoBehaviour {

		private readonly List<ComparisonTableRow> rows = new List<ComparisonTableRow> ();

		[field: SerializeField] public TextMeshProUGUI LeftHeaderText { get; private set; }
		[field: SerializeField] public TextMeshProUGUI RightHeaderText { get; private set; }
		[field: Space, SerializeField] public Transform RowParent { get; private set; }
		[field: SerializeField] public ComparisonTableRow RowPrefab { get; private set; }


		protected void UpdateTable (ComparisonTableContent content) {
			if (content == null) {
				ClearTable ();
				return;
			}

			SetText (LeftHeaderText, content.LeftHeader);
			SetText (RightHeaderText, content.RightHeader);

			for (int i = 0; i < Mathf.Max (rows.Count, content.RowCount); i++) {
				if (i < content.RowCount) {
					var value = content[i];
					if (value != null) {
						var row = GetOrCreateRow (i);
						row.EnableRow (value);
					} else {
						continue;
					}
				} else {
					rows[i].DisableRow (false);
				}
			}
		}

		public void ClearTable () {
			SetText (LeftHeaderText, string.Empty);
			SetText (RightHeaderText, string.Empty);

			for (int i = 0; i < rows.Count; i++) {
				if (rows[i] != null) {
					rows[i].DisableRow (true);
				} else {
					rows.RemoveAt (i);
					i--;
				}
			}
		}


		private ComparisonTableRow GetOrCreateRow (int at) {
			ComparisonTableRow row;
			if (at >= rows.Count) {
				row = Instantiate (RowPrefab);
				row.transform.SetParent (RowParent);
				row.transform.localScale = Vector3.one;
				rows.Add (row);
			} else {
				row = rows[at];
			}
			return row;
		}

		internal static void SetText (TextMeshProUGUI tmp, string text) {
			if (tmp != null) {
				tmp.SetText (text);
			}
		}

	}
}