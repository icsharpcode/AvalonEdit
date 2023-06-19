// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using AcAvalonEdit.Highlighting;
using AcAvalonEdit.Utils;

namespace AcAvalonEdit.Rendering
{
	sealed class TextViewCachedElements : IDisposable
	{
		TextFormatter formatter;
		Dictionary<string, TextLine> nonPrintableCharacterTexts;

		public TextLine GetTextForNonPrintableCharacter(string text, ITextRunConstructionContext context)
		{
			if (nonPrintableCharacterTexts == null)
				nonPrintableCharacterTexts = new Dictionary<string, TextLine>();
			TextLine textLine;
			VisualLineElementTextRunProperties? p;
			if (context.VisualLine?.Elements?.Any() ?? false)
				p = new VisualLineElementTextRunProperties(context.VisualLine.Elements[0].TextRunProperties);
			else {
				p = new VisualLineElementTextRunProperties(context.GlobalTextRunProperties);
				p.SetForegroundBrush(context.TextView.NonPrintableCharacterBrush);
			}


			//var p = new VisualLineElementTextRunProperties(context.GlobalTextRunProperties);
			//p.SetForegroundBrush(context.TextView.NonPrintableCharacterBrush);
			if (formatter == null)
				formatter = TextFormatterFactory.Create(context.TextView);
			textLine = FormattedTextElement.PrepareText(formatter, text, p);
			nonPrintableCharacterTexts[text] = textLine;
			return textLine;
		}

		public TextLine GetTextForNonPrintableCharacter(string text, ITextRunConstructionContext context, RichTextColorizer? props)
		{
			if (props is null)
				return (GetTextForNonPrintableCharacter(text, context));
			if (nonPrintableCharacterTexts == null)
				nonPrintableCharacterTexts = new Dictionary<string, TextLine>();
			TextLine textLine;


			var color = props.GetColorForOffsett(context.VisualLine.StartOffset);

			
			var p = new VisualLineElementTextRunProperties(context.GlobalTextRunProperties);
			
			if (color.Foreground != null) {
				Brush b = color.Foreground.GetBrush(context);
				if (b != null)
					p.SetForegroundBrush(b);
			}
			if (color.Background != null) {
				Brush b = color.Background.GetBrush(context);
				if (b != null)
					p.SetBackgroundBrush(b);
			}
			if (color.FontStyle != null || color.FontWeight != null || color.FontFamily != null) {
				Typeface tf =p.Typeface;
				p.SetTypeface(new Typeface(
					color.FontFamily ?? tf.FontFamily,
					color.FontStyle ?? tf.Style,
					color.FontWeight ?? tf.Weight,
					tf.Stretch
				));
			}
			if (color.Underline ?? false)
				p.SetTextDecorations(TextDecorations.Underline);
			if (color.Strikethrough ?? false)
				p.SetTextDecorations(TextDecorations.Strikethrough);
			if (color.FontSize.HasValue)
				p.SetFontRenderingEmSize(color.FontSize.Value);


			if (formatter == null)
				formatter = TextFormatterFactory.Create(context.TextView);
			textLine = FormattedTextElement.PrepareText(formatter, text, p);
			nonPrintableCharacterTexts[text] = textLine;
			return textLine;
		}

		public void Dispose()
		{
			if (nonPrintableCharacterTexts != null) {
				foreach (TextLine line in nonPrintableCharacterTexts.Values)
					line.Dispose();
			}
			if (formatter != null)
				formatter.Dispose();
		}
	}
}
