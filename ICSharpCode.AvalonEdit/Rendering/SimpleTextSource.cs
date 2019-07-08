// SPDX-License-Identifier: MIT

using System;
using System.Windows.Media.TextFormatting;

namespace ICSharpCode.AvalonEdit.Rendering
{
	sealed class SimpleTextSource : TextSource
	{
		readonly string text;
		readonly TextRunProperties properties;

		public SimpleTextSource(string text, TextRunProperties properties)
		{
			this.text = text;
			this.properties = properties;
		}

		public override TextRun GetTextRun(int textSourceCharacterIndex)
		{
			if (textSourceCharacterIndex < text.Length)
				return new TextCharacters(text, textSourceCharacterIndex, text.Length - textSourceCharacterIndex, properties);
			else
				return new TextEndOfParagraph(1);
		}

		public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
		{
			throw new NotImplementedException();
		}

		public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
		{
			throw new NotImplementedException();
		}
	}
}
