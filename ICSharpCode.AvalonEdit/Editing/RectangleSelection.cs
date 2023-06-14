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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;

using AcAvalonEdit.Document;
using AcAvalonEdit.Rendering;
using AcAvalonEdit.Utils;

namespace AcAvalonEdit.Editing
{
	/// <summary>
	/// Rectangular selection ("box selection").
	/// </summary>
	public sealed class RectangleSelection : Selection
	{
		#region Commands
		/// <summary>
		/// Expands the selection left by one character, creating a rectangular selection.
		/// Key gesture: Alt+Shift+Left
		/// </summary>
		public static readonly RoutedUICommand BoxSelectLeftByCharacter = Command("BoxSelectLeftByCharacter");

		/// <summary>
		/// Expands the selection right by one character, creating a rectangular selection.
		/// Key gesture: Alt+Shift+Right
		/// </summary>
		public static readonly RoutedUICommand BoxSelectRightByCharacter = Command("BoxSelectRightByCharacter");

		/// <summary>
		/// Expands the selection left by one word, creating a rectangular selection.
		/// Key gesture: Ctrl+Alt+Shift+Left
		/// </summary>
		public static readonly RoutedUICommand BoxSelectLeftByWord = Command("BoxSelectLeftByWord");

		/// <summary>
		/// Expands the selection right by one word, creating a rectangular selection.
		/// Key gesture: Ctrl+Alt+Shift+Right
		/// </summary>
		public static readonly RoutedUICommand BoxSelectRightByWord = Command("BoxSelectRightByWord");

		/// <summary>
		/// Expands the selection up by one line, creating a rectangular selection.
		/// Key gesture: Alt+Shift+Up
		/// </summary>
		public static readonly RoutedUICommand BoxSelectUpByLine = Command("BoxSelectUpByLine");

		/// <summary>
		/// Expands the selection down by one line, creating a rectangular selection.
		/// Key gesture: Alt+Shift+Down
		/// </summary>
		public static readonly RoutedUICommand BoxSelectDownByLine = Command("BoxSelectDownByLine");

		/// <summary>
		/// Expands the selection to the start of the line, creating a rectangular selection.
		/// Key gesture: Alt+Shift+Home
		/// </summary>
		public static readonly RoutedUICommand BoxSelectToLineStart = Command("BoxSelectToLineStart");

		/// <summary>
		/// Expands the selection to the end of the line, creating a rectangular selection.
		/// Key gesture: Alt+Shift+End
		/// </summary>
		public static readonly RoutedUICommand BoxSelectToLineEnd = Command("BoxSelectToLineEnd");

		static RoutedUICommand Command(string name)
		{
			return new RoutedUICommand(name, name, typeof(RectangleSelection));
		}
		#endregion

		TextDocument document;
		readonly int startLine, endLine;
		readonly double startXPos, endXPos;
		readonly int topLeftOffset, bottomRightOffset;
		readonly TextViewPosition start, end;

		readonly List<SelectionSegment> segments = new List<SelectionSegment>();

		#region Constructors
		/// <summary>
		/// Creates a new rectangular selection.
		/// </summary>
		public RectangleSelection(TextArea textArea, TextViewPosition start, TextViewPosition end)
			: base(textArea)
		{
			InitDocument();
			this.startLine = start.Line;
			this.endLine = end.Line;
			this.startXPos = GetXPos(textArea, start);
			this.endXPos = GetXPos(textArea, end);
			CalculateSegments();
			this.topLeftOffset = this.segments.First().StartOffset;
			this.bottomRightOffset = this.segments.Last().EndOffset;

			this.start = start;
			this.end = end;
		}

		private RectangleSelection(TextArea textArea, int startLine, double startXPos, TextViewPosition end)
			: base(textArea)
		{
			InitDocument();
			this.startLine = startLine;
			this.endLine = end.Line;
			this.startXPos = startXPos;
			this.endXPos = GetXPos(textArea, end);
			CalculateSegments();
			this.topLeftOffset = this.segments.First().StartOffset;
			this.bottomRightOffset = this.segments.Last().EndOffset;

			this.start = GetStart();
			this.end = end;
		}

		private RectangleSelection(TextArea textArea, TextViewPosition start, int endLine, double endXPos)
			: base(textArea)
		{
			InitDocument();
			this.startLine = start.Line;
			this.endLine = endLine;
			this.startXPos = GetXPos(textArea, start);
			this.endXPos = endXPos;
			CalculateSegments();
			this.topLeftOffset = this.segments.First().StartOffset;
			this.bottomRightOffset = this.segments.Last().EndOffset;

			this.start = start;
			this.end = GetEnd();
		}

		void InitDocument()
		{
			document = textArea.Document;
			if (document == null)
				throw ThrowUtil.NoDocumentAssigned();
		}

		static double GetXPos(TextArea textArea, TextViewPosition pos)
		{
			DocumentLine documentLine = textArea.Document.GetLineByNumber(pos.Line);
			VisualLine visualLine = textArea.TextView.GetOrConstructVisualLine(documentLine);
			int vc = visualLine.ValidateVisualColumn(pos, true);
			TextLine textLine = visualLine.GetTextLine(vc, pos.IsAtEndOfLine);
			return visualLine.GetTextLineVisualXPosition(textLine, vc);
		}

		void CalculateSegments()
		{
			DocumentLine nextLine = document.GetLineByNumber(Math.Min(startLine, endLine));
			do {
				VisualLine vl = textArea.TextView.GetOrConstructVisualLine(nextLine);
				int startVC = vl.GetVisualColumn(new Point(startXPos, 0), true);
				int endVC = vl.GetVisualColumn(new Point(endXPos, 0), true);

				int baseOffset = vl.FirstDocumentLine.Offset;
				int startOffset = baseOffset + vl.GetRelativeOffset(startVC);
				int endOffset = baseOffset + vl.GetRelativeOffset(endVC);
				segments.Add(new SelectionSegment(startOffset, startVC, endOffset, endVC));

				nextLine = vl.LastDocumentLine.NextLine;
			} while (nextLine != null && nextLine.LineNumber <= Math.Max(startLine, endLine));
		}

		TextViewPosition GetStart()
		{
			SelectionSegment segment = (startLine < endLine ? segments.First() : segments.Last());
			if (startXPos < endXPos) {
				return new TextViewPosition(document.GetLocation(segment.StartOffset), segment.StartVisualColumn);
			} else {
				return new TextViewPosition(document.GetLocation(segment.EndOffset), segment.EndVisualColumn);
			}
		}

		TextViewPosition GetEnd()
		{
			SelectionSegment segment = (startLine < endLine ? segments.Last() : segments.First());
			if (startXPos < endXPos) {
				return new TextViewPosition(document.GetLocation(segment.EndOffset), segment.EndVisualColumn);
			} else {
				return new TextViewPosition(document.GetLocation(segment.StartOffset), segment.StartVisualColumn);
			}
		}
		#endregion

		/// <inheritdoc/>
		public override string GetText()
		{
			StringBuilder b = new StringBuilder();
			foreach (ISegment s in this.Segments) {
				if (b.Length > 0)
					b.AppendLine();
				b.Append(document.GetText(s));
			}
			return b.ToString();
		}

		/// <inheritdoc/>
		public override Selection StartSelectionOrSetEndpoint(TextViewPosition startPosition, TextViewPosition endPosition)
		{
			return SetEndpoint(endPosition);
		}

		/// <inheritdoc/>
		public override int Length {
			get {
				return this.Segments.Sum(s => s.Length);
			}
		}

		/// <inheritdoc/>
		public override bool EnableVirtualSpace {
			get { return true; }
		}

		/// <inheritdoc/>
		public override ISegment SurroundingSegment {
			get {
				return new SimpleSegment(topLeftOffset, bottomRightOffset - topLeftOffset);
			}
		}

		/// <inheritdoc/>
		public override IEnumerable<SelectionSegment> Segments {
			get { return segments; }
		}

		/// <inheritdoc/>
		public override TextViewPosition StartPosition {
			get { return start; }
		}

		/// <inheritdoc/>
		public override TextViewPosition EndPosition {
			get { return end; }
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			RectangleSelection r = obj as RectangleSelection;
			return r != null && r.textArea == this.textArea
				&& r.topLeftOffset == this.topLeftOffset && r.bottomRightOffset == this.bottomRightOffset
				&& r.startLine == this.startLine && r.endLine == this.endLine
				&& r.startXPos == this.startXPos && r.endXPos == this.endXPos;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return topLeftOffset ^ bottomRightOffset;
		}

		/// <inheritdoc/>
		public override Selection SetEndpoint(TextViewPosition endPosition)
		{
			return new RectangleSelection(textArea, startLine, startXPos, endPosition);
		}

		int GetVisualColumnFromXPos(int line, double xPos)
		{
			var vl = textArea.TextView.GetOrConstructVisualLine(textArea.Document.GetLineByNumber(line));
			return vl.GetVisualColumn(new Point(xPos, 0), true);
		}

		/// <inheritdoc/>
		public override Selection UpdateOnDocumentChange(DocumentChangeEventArgs e)
		{
			TextLocation newStartLocation = textArea.Document.GetLocation(e.GetNewOffset(topLeftOffset, AnchorMovementType.AfterInsertion));
			TextLocation newEndLocation = textArea.Document.GetLocation(e.GetNewOffset(bottomRightOffset, AnchorMovementType.BeforeInsertion));

			return new RectangleSelection(textArea,
										  new TextViewPosition(newStartLocation, GetVisualColumnFromXPos(newStartLocation.Line, startXPos)),
										  new TextViewPosition(newEndLocation, GetVisualColumnFromXPos(newEndLocation.Line, endXPos)));
		}

		/// <inheritdoc/>
		public override void ReplaceSelectionWithText(string newText)
		{
			if (newText == null)
				throw new ArgumentNullException("newText");
			using (textArea.Document.RunUpdate()) {
				TextViewPosition start = new TextViewPosition(document.GetLocation(topLeftOffset), GetVisualColumnFromXPos(startLine, startXPos));
				TextViewPosition end = new TextViewPosition(document.GetLocation(bottomRightOffset), GetVisualColumnFromXPos(endLine, endXPos));
				int insertionLength;
				int totalInsertionLength = 0;
				int firstInsertionLength = 0;
				int editOffset = Math.Min(topLeftOffset, bottomRightOffset);
				TextViewPosition pos;
				if (NewLineFinder.NextNewLine(newText, 0) == SimpleSegment.Invalid) {
					// insert same text into every line
					foreach (SelectionSegment lineSegment in this.Segments.Reverse()) {
						ReplaceSingleLineText(textArea, lineSegment, newText, out insertionLength);
						totalInsertionLength += insertionLength;
						firstInsertionLength = insertionLength;
					}

					int newEndOffset = editOffset + totalInsertionLength;
					pos = new TextViewPosition(document.GetLocation(editOffset + firstInsertionLength));

					textArea.Selection = new RectangleSelection(textArea, pos, Math.Max(startLine, endLine), GetXPos(textArea, pos));
				} else {
					string[] lines = newText.Split(NewLineFinder.NewlineStrings, segments.Count, StringSplitOptions.None);
					int line = Math.Min(startLine, endLine);
					for (int i = lines.Length - 1; i >= 0; i--) {
						ReplaceSingleLineText(textArea, segments[i], lines[i], out insertionLength);
						firstInsertionLength = insertionLength;
					}
					pos = new TextViewPosition(document.GetLocation(editOffset + firstInsertionLength));
					textArea.ClearSelection();
				}
				textArea.Caret.Position = textArea.TextView.GetPosition(new Point(GetXPos(textArea, pos), textArea.TextView.GetVisualTopByDocumentLine(Math.Max(startLine, endLine)))).GetValueOrDefault();
			}
		}

		void ReplaceSingleLineText(TextArea textArea, SelectionSegment lineSegment, string newText, out int insertionLength)
		{
			if (lineSegment.Length == 0) {
				if (newText.Length > 0 && textArea.ReadOnlySectionProvider.CanInsert(lineSegment.StartOffset)) {
					newText = AddSpacesIfRequired(newText, new TextViewPosition(document.GetLocation(lineSegment.StartOffset), lineSegment.StartVisualColumn), new TextViewPosition(document.GetLocation(lineSegment.EndOffset), lineSegment.EndVisualColumn));
					textArea.Document.Insert(lineSegment.StartOffset, newText);
				}
			} else {
				ISegment[] segmentsToDelete = textArea.GetDeletableSegments(lineSegment);
				for (int i = segmentsToDelete.Length - 1; i >= 0; i--) {
					if (i == segmentsToDelete.Length - 1) {
						if (segmentsToDelete[i].Offset == SurroundingSegment.Offset && segmentsToDelete[i].Length == SurroundingSegment.Length) {
							newText = AddSpacesIfRequired(newText, new TextViewPosition(document.GetLocation(lineSegment.StartOffset), lineSegment.StartVisualColumn), new TextViewPosition(document.GetLocation(lineSegment.EndOffset), lineSegment.EndVisualColumn));
						}
						textArea.Document.Replace(segmentsToDelete[i], newText);
					} else {
						textArea.Document.Remove(segmentsToDelete[i]);
					}
				}
			}
			insertionLength = newText.Length;
		}

		/// <summary>
		/// Performs a rectangular paste operation.
		/// </summary>
		public static bool PerformRectangularPaste(TextArea textArea, TextViewPosition startPosition, string text, bool selectInsertedText)
		{
			if (textArea == null)
				throw new ArgumentNullException("textArea");
			if (text == null)
				throw new ArgumentNullException("text");
			int newLineCount = text.Count(c => c == '\n'); // TODO might not work in all cases, but single \r line endings are really rare today.
			TextLocation endLocation = new TextLocation(startPosition.Line + newLineCount, startPosition.Column);
			if (endLocation.Line <= textArea.Document.LineCount) {
				int endOffset = textArea.Document.GetOffset(endLocation);
				if (textArea.Selection.EnableVirtualSpace || textArea.Document.GetLocation(endOffset) == endLocation) {
					RectangleSelection rsel = new RectangleSelection(textArea, startPosition, endLocation.Line, GetXPos(textArea, startPosition));
					rsel.ReplaceSelectionWithText(text);
					if (selectInsertedText && textArea.Selection is RectangleSelection) {
						RectangleSelection sel = (RectangleSelection)textArea.Selection;
						textArea.Selection = new RectangleSelection(textArea, startPosition, sel.endLine, sel.endXPos);
					}
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the name of the entry in the DataObject that signals rectangle selections.
		/// </summary>
		public const string RectangularSelectionDataType = "AvalonEditRectangularSelection";

		/// <inheritdoc/>
		public override System.Windows.DataObject CreateDataObject(TextArea textArea)
		{
			var data = base.CreateDataObject(textArea);

			if (EditingCommandHandler.ConfirmDataFormat(textArea, data, RectangularSelectionDataType)) {
				MemoryStream isRectangle = new MemoryStream(1);
				isRectangle.WriteByte(1);
				data.SetData(RectangularSelectionDataType, isRectangle, false);
			}
			return data;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			// It's possible that ToString() gets called on old (invalid) selections, e.g. for "change from... to..." debug message
			// make sure we don't crash even when the desired locations don't exist anymore.
			return string.Format("[RectangleSelection {0} {1} {2} to {3} {4} {5}]", startLine, topLeftOffset, startXPos, endLine, bottomRightOffset, endXPos);
		}
	}
}
