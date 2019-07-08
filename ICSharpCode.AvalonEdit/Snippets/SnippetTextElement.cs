// SPDX-License-Identifier: MIT

using System;
using System.Windows.Documents;

namespace ICSharpCode.AvalonEdit.Snippets
{
	/// <summary>
	/// Represents a text element in a snippet.
	/// </summary>
	[Serializable]
	public class SnippetTextElement : SnippetElement
	{
		string text;

		/// <summary>
		/// The text to be inserted.
		/// </summary>
		public string Text {
			get { return text; }
			set { text = value; }
		}

		/// <inheritdoc/>
		public override void Insert(InsertionContext context)
		{
			if (text != null)
				context.InsertText(text);
		}

		/// <inheritdoc/>
		public override Inline ToTextRun()
		{
			return new Run(text ?? string.Empty);
		}
	}
}
