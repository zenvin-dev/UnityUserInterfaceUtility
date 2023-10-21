using UnityEngine;
using UnityEngine.UI;

namespace Zenvin.UI.Abstractions {
	/// <summary>
	/// An abstraction of some of Unity's <see cref="Image"/>'s properties.<br></br>
	/// Allows for other controls that behave similarly or in the same way as the built-in image without having to fiddle with different reference types. <br></br>
	/// Implemented through <see cref="UIImage"/>.
	/// </summary>
	public interface IImage : IGraphic, IMaskable {
		/// <inheritdoc cref="Image.sprite"/>
		Sprite sprite { get; set; }
		/// <inheritdoc cref="Graphic.color"/>
		Color color { get; set; }
		/// <inheritdoc cref="Image.fillAmount"/>
		float fillAmount { get; set; }
		/// <inheritdoc cref="Image.alphaHitTestMinimumThreshold"/>
		float alphaHitTestMinimumThreshold { get; set; }
	}
}