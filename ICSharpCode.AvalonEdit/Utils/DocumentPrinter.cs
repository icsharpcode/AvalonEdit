// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

namespace ICSharpCode.AvalonEdit.Utils
{
	/// <summary>
	/// Helps printing documents.
	/// </summary>
	public static class DocumentPrinter
	{
		/// <summary>
		/// Converts an IDocument to a Block and applies the provided highlighter.
		/// </summary>
		public static Block ConvertTextDocumentToBlock(IDocument document, IHighlighter highlighter)
		{
			if (document == null)
				throw new ArgumentNullException("document");
			Paragraph p = new Paragraph();
			p.TextAlignment = TextAlignment.Left;
			for (int lineNumber = 1; lineNumber <= document.LineCount; lineNumber++) {
				if (lineNumber > 1)
					p.Inlines.Add(new LineBreak());
				var line = document.GetLineByNumber(lineNumber);
				if (highlighter != null) {
					HighlightedLine highlightedLine = highlighter.HighlightLine(lineNumber);
					p.Inlines.AddRange(highlightedLine.ToRichText().CreateRuns());
				} else {
					p.Inlines.Add(document.GetText(line));
				}
			}
			return p;
		}

		/// <summary>
		/// Converts an IDocument to a RichText and applies the provided highlighter.
		/// </summary>
		public static RichText ConvertTextDocumentToRichText(IDocument document, IHighlighter highlighter)
		{
			if (document == null)
				throw new ArgumentNullException("document");
			var texts = new List<RichText>();
			for (int lineNumber = 1; lineNumber <= document.LineCount; lineNumber++) {
				var line = document.GetLineByNumber(lineNumber);
				if (lineNumber > 1)
					texts.Add(line.PreviousLine.DelimiterLength == 2 ? "\r\n" : "\n");
				if (highlighter != null) {
					HighlightedLine highlightedLine = highlighter.HighlightLine(lineNumber);
					texts.Add(highlightedLine.ToRichText());
				} else {
					texts.Add(document.GetText(line));
				}
			}
			return RichText.Concat(texts.ToArray());
		}

		/// <summary>
		/// Creates a flow document from the editor's contents.
		/// </summary>
		public static FlowDocument CreateFlowDocumentForEditor(TextEditor editor)
		{
			IHighlighter highlighter = editor.TextArea.GetService(typeof(IHighlighter)) as IHighlighter;
			FlowDocument doc = new FlowDocument(ConvertTextDocumentToBlock(editor.Document, highlighter));
			doc.FontFamily = editor.FontFamily;
			doc.FontSize = editor.FontSize;
			return doc;
		}
	}
}
