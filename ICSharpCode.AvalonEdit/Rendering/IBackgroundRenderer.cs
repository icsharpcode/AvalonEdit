// SPDX-License-Identifier: MIT

using System.Windows.Media;

namespace ICSharpCode.AvalonEdit.Rendering
{
	/// <summary>
	/// Background renderers draw in the background of a known layer.
	/// You can use background renderers to draw non-interactive elements on the TextView
	/// without introducing new UIElements.
	/// </summary>
	public interface IBackgroundRenderer
	{
		/// <summary>
		/// Gets the layer on which this background renderer should draw.
		/// </summary>
		KnownLayer Layer { get; }

		/// <summary>
		/// Causes the background renderer to draw.
		/// </summary>
		void Draw(TextView textView, DrawingContext drawingContext);
	}
}
