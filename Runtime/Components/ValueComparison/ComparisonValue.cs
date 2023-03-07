using System;

namespace Zenvin.UI.Components.ValueComparison {
	public class ComparisonValue {
		public enum Relation {
			/// <summary> The values cannot be compared. </summary>
			Incomparable,
			/// <summary> The values are equal to each other. </summary>
			Equal,
			/// <summary> The left value is better than the right. </summary>
			Better,
			/// <summary> The left value is worse than the right. </summary>
			Worse,
		}

		public string Label { get; private set; }

		public string Left { get; private set; }
		public string Right { get; private set; }
		public Relation ValueRelation { get; private set; }


		public ComparisonValue () { }

		public ComparisonValue (string label) : this () {
			Label = label;
		}

		public ComparisonValue (string label, Relation valueRelation) : this (label) {
			ValueRelation = valueRelation;
		}

		/// <summary>
		/// Sets the label of and returns the current instance.
		/// </summary>
		public ComparisonValue WithLabel (string label) {
			Label = label;
			return this;
		}

		/// <summary>
		/// Sets the left value of and returns the current instance.
		/// </summary>
		/// <param name="value">The new left value.</param>
		public ComparisonValue WithLeft (object value, string formatter = null, IFormatProvider formatProvider = null) {
			Left = GetFormattedString (value, "-", formatter, formatProvider);
			return this;
		}

		/// <summary>
		/// Sets the right value of and returns the current instance.
		/// </summary>
		/// <param name="value">The new right value.</param>
		public ComparisonValue WithRight (object value, string formatter = null, IFormatProvider formatProvider = null) {
			Right = GetFormattedString (value, "-", formatter, formatProvider);
			return this;
		}

		/// <summary>
		/// Sets the relation of and returns the current instance.
		/// </summary>
		/// <param name="relation">The new <see cref="ValueRelation"/> from left to right value.</param>
		public ComparisonValue WithRelation (Relation relation) {
			ValueRelation = relation;
			return this;
		}

		/// <summary>
		/// Determines a <see cref="Relation"/> based on two values <c>x</c> and <c>y</c> (those should correspond to <see cref="Left"/> and <see cref="Right"/>, respectively).<br></br>
		/// Then updates the current instance's <see cref="ValueRelation"/> accordingly and returns the current instance.
		/// </summary>
		/// <param name="x">Left comparer.</param>
		/// <param name="y">Right comparer.</param>
		public ComparisonValue WithRelation<T> (T x, T y) where T : IComparable<T> {
			int comp = x.CompareTo (y);
			switch (comp) {
				case -1:
					ValueRelation = Relation.Worse;
					break;
				case 0:
					ValueRelation = Relation.Equal;
					break;
				case 1:
					ValueRelation = Relation.Better;
					break;
			}
			return this;
		}

		/// <summary>
		/// Attempts to determine a <see cref="Relation"/> based on the current values of <see cref="Left"/> and <see cref="Right"/>, given both of them are convertable to <typeparamref name="T"/>.
		/// <br></br>
		/// Then updates the current instance's <see cref="ValueRelation"/> accordingly and returns the current instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="success"></param>
		/// <returns></returns>
		public ComparisonValue WithRelation<T> (out bool success) where T : IComparable<T> {
			success = false;
			if (Left is T left && Right is T right) {
				success = true;
				int comp = left.CompareTo (right);
				switch (comp) {
					case -1:
						ValueRelation = Relation.Worse;
						break;
					case 0:
						ValueRelation = Relation.Equal;
						break;
					case 1:
						ValueRelation = Relation.Better;
						break;
				}
			} else {
				ValueRelation = Relation.Incomparable;
			}
			return this;
		}

		/// <summary>
		/// Flips the current instance's <see cref="ValueRelation"/> if it has a value of either <see cref="Relation.Better"/> or <see cref="Relation.Worse"/>.<br></br>
		/// Then updates the current instance's <see cref="ValueRelation"/> and returns the current instance.
		/// </summary>
		public ComparisonValue Flipped () {
			switch (ValueRelation) {
				case Relation.Better:
					ValueRelation = Relation.Worse;
					break;
				case Relation.Worse:
					ValueRelation = Relation.Better;
					break;
			}
			return this;
		}


		private static string GetFormattedString (object value, string nullReplacement, string formatter, IFormatProvider formatProvider) {
			if (value == null) {
				return "-";
			}

			if (value is IFormattable format && formatProvider != null) {
				try {
					return format.ToString (formatter, formatProvider);
				} catch {
					return value.ToString ();
				}
			}

			return value.ToString ();
		}
	}
}