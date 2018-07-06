﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;

namespace ICSharpCode.AvalonEdit.Rendering
{
	/// <summary>
	/// VisualLineElement that represents a piece of text.
	/// </summary>
	public class VisualLineText : VisualLineElement
	{
		private VisualLine parentVisualLine;

		/// <summary>
		/// Gets the parent visual line.
		/// </summary>
		public VisualLine ParentVisualLine
		{
			get { return parentVisualLine; }
		}

		/// <summary>
		/// Creates a visual line text element with the specified length.
		/// It uses the <see cref="ITextRunConstructionContext.VisualLine"/> and its
		/// <see cref="VisualLineElement.RelativeTextOffset"/> to find the actual text string.
		/// </summary>
		public VisualLineText(VisualLine parentVisualLine, int length) : base(length, length)
		{
			if (parentVisualLine == null)
				throw new ArgumentNullException("parentVisualLine");
			this.parentVisualLine = parentVisualLine;
		}

		/// <summary>
		/// Override this method to control the type of new VisualLineText instances when
		/// the visual line is split due to syntax highlighting.
		/// </summary>
		protected virtual VisualLineText CreateInstance(int length)
		{
			return new VisualLineText(parentVisualLine, length);
		}

		/// <inheritdoc/>
		public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			int relativeOffset = startVisualColumn - VisualColumn;
			StringSegment text = context.GetText(context.VisualLine.FirstDocumentLine.Offset + RelativeTextOffset + relativeOffset, DocumentLength - relativeOffset);
			return new TextCharacters(text.Text, text.Offset, text.Count, this.TextRunProperties);
		}

		/// <inheritdoc/>
		public override bool IsWhitespace(int visualColumn)
		{
			int offset = visualColumn - this.VisualColumn + parentVisualLine.FirstDocumentLine.Offset + this.RelativeTextOffset;
			return char.IsWhiteSpace(parentVisualLine.Document.GetCharAt(offset));
		}

		/// <inheritdoc/>
		public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int visualColumnLimit, ITextRunConstructionContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			int relativeOffset = visualColumnLimit - VisualColumn;
			StringSegment text = context.GetText(context.VisualLine.FirstDocumentLine.Offset + RelativeTextOffset, relativeOffset);
			CharacterBufferRange range = new CharacterBufferRange(text.Text, text.Offset, text.Count);
			return new TextSpan<CultureSpecificCharacterBufferRange>(range.Length, new CultureSpecificCharacterBufferRange(this.TextRunProperties.CultureInfo, range));
		}

		/// <inheritdoc/>
		public override bool CanSplit
		{
			get { return true; }
		}

		/// <inheritdoc/>
		public override void Split(int splitVisualColumn, IList<VisualLineElement> elements, int elementIndex)
		{
			if (splitVisualColumn <= VisualColumn || splitVisualColumn >= VisualColumn + VisualLength)
				throw new ArgumentOutOfRangeException("splitVisualColumn", splitVisualColumn, "Value must be between " + (VisualColumn + 1) + " and " + (VisualColumn + VisualLength - 1));
			if (elements == null)
				throw new ArgumentNullException("elements");
			if (elements[elementIndex] != this)
				throw new ArgumentException("Invalid elementIndex - couldn't find this element at the index");
			int relativeSplitPos = splitVisualColumn - VisualColumn;
			VisualLineText splitPart = CreateInstance(DocumentLength - relativeSplitPos);
			SplitHelper(this, splitPart, splitVisualColumn, relativeSplitPos + RelativeTextOffset);
			elements.Insert(elementIndex + 1, splitPart);
		}

		/// <inheritdoc/>
		public override int GetRelativeOffset(int visualColumn)
		{
			return this.RelativeTextOffset + visualColumn - this.VisualColumn;
		}

		/// <inheritdoc/>
		public override int GetVisualColumn(int relativeTextOffset)
		{
			return this.VisualColumn + relativeTextOffset - this.RelativeTextOffset;
		}

		/// <inheritdoc/>
		public override int GetNextCaretPosition(int visualColumn, LogicalDirection direction, CaretPositioningMode mode)
		{
			int textOffset = parentVisualLine.StartOffset + this.RelativeTextOffset;
			int pos = TextUtilities.GetNextCaretPosition(parentVisualLine.Document, textOffset + visualColumn - this.VisualColumn, direction, mode);
			if (pos < textOffset || pos > textOffset + this.DocumentLength)
				return -1;
			else
				return this.VisualColumn + pos - textOffset;
		}
	}
}