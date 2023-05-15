using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zenvin.UI.Components.ValueComparison {
	[AddComponentMenu ("UI/Zenvin/Comparison Table Row", 0)]
	public class ComparisonTableRow : MonoBehaviour {

		[SerializeField] private TextMeshProUGUI labelText;
		[SerializeField] private TextMeshProUGUI leftValueText;
		[SerializeField] private TextMeshProUGUI rightValueText;
		[Space]
		[SerializeField] private Image relationImage;
		[SerializeField] private TintedSprite equalSprite = new TintedSprite ();
		[SerializeField] private TintedSprite betterSprite = new TintedSprite ();
		[SerializeField] private TintedSprite worseSprite = new TintedSprite ();
		[SerializeField] private TintedSprite incomparableSprite = new TintedSprite ();


		internal void EnableRow (ComparisonValue value) {
			ComparisonTable.SetText (labelText, value.Label);
			ComparisonTable.SetText (leftValueText, value.Left);
			ComparisonTable.SetText (rightValueText, value.Right);

			TintedSprite ts = GetSpriteFromRelation (value.ValueRelation);
			ts?.UpdateImage (relationImage);

			gameObject.SetActive (true);
		}

		internal void DisableRow (bool reset) {
			if (reset) {
				ComparisonTable.SetText (labelText, string.Empty);
				ComparisonTable.SetText (leftValueText, string.Empty);
				ComparisonTable.SetText (rightValueText, string.Empty);
			}
			gameObject.SetActive (false);
		}

		private TintedSprite GetSpriteFromRelation (ComparisonValue.Relation relation) {
			switch (relation) {
				case ComparisonValue.Relation.Incomparable:
					return incomparableSprite;
				case ComparisonValue.Relation.Equal:
					return equalSprite;
				case ComparisonValue.Relation.Better:
					return betterSprite;
				case ComparisonValue.Relation.Worse:
					return worseSprite;
				default:
					return null;
			}
		}


		[Serializable]
		public class TintedSprite {
			[SerializeField] private Sprite sprite = null;
			[SerializeField] private Color tint = Color.clear;

			public void UpdateImage (Image img) {
				if (img != null) {
					img.sprite = sprite;
					img.color = tint;
				}
			}
		}
	}
}