// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace ICSharpCode.AvalonEdit.Utils
{
	/// <summary>
	/// Creates TextFormatter instances that with the correct TextFormattingMode, if running on .NET 4.0.
	/// </summary>
	static class TextFormatterFactory
	{
		/// <summary>
		/// Creates a <see cref="TextFormatter"/> using the formatting mode used by the specified owner object.
		/// </summary>
		public static TextFormatter Create(DependencyObject owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			return TextFormatter.Create(TextOptions.GetTextFormattingMode(owner));
		}

		/// <summary>
		/// Returns whether the specified dependency property affects the text formatter creation.
		/// Controls should re-create their text formatter for such property changes.
		/// </summary>
		public static bool PropertyChangeAffectsTextFormatter(DependencyProperty dp)
		{
			return dp == TextOptions.TextFormattingModeProperty;
		}

		/// <summary>
		/// Creates formatted text.
		/// </summary>
		/// <param name="element">The owner element. The text formatter setting are read from this element.</param>
		/// <param name="text">The text.</param>
		/// <param name="typeface">The typeface to use. If this parameter is null, the typeface of the <paramref name="element"/> will be used.</param>
		/// <param name="emSize">The font size. If this parameter is null, the font size of the <paramref name="element"/> will be used.</param>
		/// <param name="foreground">The foreground color. If this parameter is null, the foreground of the <paramref name="element"/> will be used.</param>
		/// <returns>A FormattedText object using the specified settings.</returns>
		public static FormattedText CreateFormattedText(FrameworkElement element, string text, Typeface typeface, double? emSize, Brush foreground)
		{
			if (element == null)
				throw new ArgumentNullException("element");
			if (text == null)
				throw new ArgumentNullException("text");
			if (typeface == null)
				typeface = element.CreateTypeface();
			if (emSize == null)
				emSize = TextBlock.GetFontSize(element);
			if (foreground == null)
				foreground = TextBlock.GetForeground(element);
			return new FormattedText(
				text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				typeface,
				emSize.Value,
				foreground,
				null,
				TextOptions.GetTextFormattingMode(element)
			);
		}
	}
}
