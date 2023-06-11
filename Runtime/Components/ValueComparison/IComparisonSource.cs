namespace Zenvin.UI.Components.ValueComparison {
	public delegate void OnContentChanged ();

	public interface IComparisonSource {
		event OnContentChanged ContentChanged;
		ComparisonTableContent GetContent ();
	}
}