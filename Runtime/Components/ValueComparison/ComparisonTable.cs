using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zenvin.UI.Components.ValueComparison {
	public class ComparisonTable : MonoBehaviour {

		private readonly List<ComparisonTableRow> rows = new List<ComparisonTableRow> ();
		private ComparisonTableContent content;

		[SerializeField] private TextMeshProUGUI leftHeaderText;
		[SerializeField] private TextMeshProUGUI rightHeaderText;
		[Space, SerializeField] private VerticalLayoutGroup rowParent;
		[SerializeField] private ComparisonTableRow rowPrefab;


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
			if (Content == null) {
				ClearTable ();
				return;
			}

			SetText (leftHeaderText, Content.LeftHeader);
			SetText (rightHeaderText, Content.RightHeader);

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
			SetText (leftHeaderText, string.Empty);
			SetText (rightHeaderText, string.Empty);

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
				row = Instantiate (rowPrefab);
				row.transform.SetParent (rowParent.transform);
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