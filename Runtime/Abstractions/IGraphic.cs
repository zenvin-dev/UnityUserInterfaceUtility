using UnityEngine;
using UnityEngine.UI;

namespace Zenvin.UI.Abstractions {
	public interface IGraphic : ICanvasElement {
		Canvas canvas { get; }
		CanvasRenderer canvasRenderer { get; }
		Color color { get; set; }
		Material defaultMaterial { get; }
		int depth { get; }
		Material material { get; set; }
		Material materialForRendering { get; }
		bool raycastTarget { get; set; }
		RectTransform rectTransform { get; }
	}
}