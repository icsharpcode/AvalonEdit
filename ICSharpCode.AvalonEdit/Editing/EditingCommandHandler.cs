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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Editing
{
	/// <summary>
	/// We re-use the CommandBinding and InputBinding instances between multiple text areas,
	/// so this class is static.
	/// </summary>
	static class EditingCommandHandler
	{
		/// <summary>
		/// Creates a new <see cref="TextAreaInputHandler"/> for the text area.
		/// </summary>
		public static TextAreaInputHandler Create(TextArea textArea)
		{
			TextAreaInputHandler handler = new TextAreaInputHandler(textArea);
			handler.CommandBindings.AddRange(CommandBindings);
			handler.InputBindings.AddRange(InputBindings);
			return handler;
		}

		static readonly List<CommandBinding> CommandBindings = new List<CommandBinding>();
		static readonly List<InputBinding> InputBindings = new List<InputBinding>();

		static void AddBinding(ICommand command, ModifierKeys modifiers, Key key, ExecutedRoutedEventHandler handler)
		{
			CommandBindings.Add(new CommandBinding(command, handler));
			InputBindings.Add(TextAreaDefaultInputHandler.CreateFrozenKeyBinding(command, modifiers, key));
		}

		static EditingCommandHandler()
		{
			CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, OnDelete(CaretMovementType.None), CanDelete));
			AddBinding(EditingCommands.Delete, ModifierKeys.None, Key.Delete, OnDelete(CaretMovementType.CharRight));
			AddBinding(EditingCommands.DeleteNextWord, ModifierKeys.Control, Key.Delete, OnDelete(CaretMovementType.WordRight));
			AddBinding(EditingCommands.Backspace, ModifierKeys.None, Key.Back, OnDelete(CaretMovementType.Backspace));
			InputBindings.Add(TextAreaDefaultInputHandler.CreateFrozenKeyBinding(EditingCommands.Backspace, ModifierKeys.Shift, Key.Back)); // make Shift-Backspace do the same as plain backspace
			AddBinding(EditingCommands.DeletePreviousWord, ModifierKeys.Control, Key.Back, OnDelete(CaretMovementType.WordLeft));
			AddBinding(EditingCommands.EnterParagraphBreak, ModifierKeys.None, Key.Enter, OnEnter);
			AddBinding(EditingCommands.EnterLineBreak, ModifierKeys.Shift, Key.Enter, OnEnter);
			AddBinding(EditingCommands.TabForward, ModifierKeys.None, Key.Tab, OnTab);
			AddBinding(EditingCommands.TabBackward, ModifierKeys.Shift, Key.Tab, OnShiftTab);

			CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopy, CanCutOrCopy));
			CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnCut, CanCutOrCopy));
			CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPaste, CanPaste));

			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ToggleOverstrike, OnToggleOverstrike));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.DeleteLine, OnDeleteLine));

			CommandBindings.Add(new CommandBinding(AvalonEditCommands.RemoveLeadingWhitespace, OnRemoveLeadingWhitespace));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.RemoveTrailingWhitespace, OnRemoveTrailingWhitespace));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ConvertToUppercase, OnConvertToUpperCase));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ConvertToLowercase, OnConvertToLowerCase));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ConvertToTitleCase, OnConvertToTitleCase));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.InvertCase, OnInvertCase));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ConvertTabsToSpaces, OnConvertTabsToSpaces));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ConvertSpacesToTabs, OnConvertSpacesToTabs));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ConvertLeadingTabsToSpaces, OnConvertLeadingTabsToSpaces));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.ConvertLeadingSpacesToTabs, OnConvertLeadingSpacesToTabs));
			CommandBindings.Add(new CommandBinding(AvalonEditCommands.IndentSelection, OnIndentSelection));

			TextAreaDefaultInputHandler.WorkaroundWPFMemoryLeak(InputBindings);
		}

		static TextArea GetTextArea(object target)
		{
			return target as TextArea;
		}

		#region Text Transformation Helpers
		enum DefaultSegmentType
		{
			None,
			WholeDocument,
			CurrentLine
		}

		/// <summary>
		/// Calls transformLine on all lines in the selected range.
		/// transformLine needs to handle read-only segments!
		/// </summary>
		static void TransformSelectedLines(Action<TextArea, DocumentLine> transformLine, object target, ExecutedRoutedEventArgs args, DefaultSegmentType defaultSegmentType)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				using (textArea.Document.RunUpdate()) {
					DocumentLine start, end;
					if (textArea.Selection.IsEmpty) {
						if (defaultSegmentType == DefaultSegmentType.CurrentLine) {
							start = end = textArea.Document.GetLineByNumber(textArea.Caret.Line);
						} else if (defaultSegmentType == DefaultSegmentType.WholeDocument) {
							start = textArea.Document.Lines.First();
							end = textArea.Document.Lines.Last();
						} else {
							start = end = null;
						}
					} else {
						ISegment segment = textArea.Selection.SurroundingSegment;
						start = textArea.Document.GetLineByOffset(segment.Offset);
						end = textArea.Document.GetLineByOffset(segment.EndOffset);
						// don't include the last line if no characters on it are selected
						if (start != end && end.Offset == segment.EndOffset)
							end = end.PreviousLine;
					}
					if (start != null) {
						transformLine(textArea, start);
						while (start != end) {
							start = start.NextLine;
							transformLine(textArea, start);
						}
					}
				}
				textArea.Caret.BringCaretToView();
				args.Handled = true;
			}
		}

		/// <summary>
		/// Calls transformLine on all writable segment in the selected range.
		/// </summary>
		static void TransformSelectedSegments(Action<TextArea, ISegment> transformSegment, object target, ExecutedRoutedEventArgs args, DefaultSegmentType defaultSegmentType)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				using (textArea.Document.RunUpdate()) {
					IEnumerable<ISegment> segments;
					if (textArea.Selection.IsEmpty) {
						if (defaultSegmentType == DefaultSegmentType.CurrentLine) {
							segments = new ISegment[] { textArea.Document.GetLineByNumber(textArea.Caret.Line) };
						} else if (defaultSegmentType == DefaultSegmentType.WholeDocument) {
							segments = textArea.Document.Lines.Cast<ISegment>();
						} else {
							segments = null;
						}
					} else {
						segments = textArea.Selection.Segments.Cast<ISegment>();
					}
					if (segments != null) {
						foreach (ISegment segment in segments.Reverse()) {
							foreach (ISegment writableSegment in textArea.GetDeletableSegments(segment).Reverse()) {
								transformSegment(textArea, writableSegment);
							}
						}
					}
				}
				textArea.Caret.BringCaretToView();
				args.Handled = true;
			}
		}
		#endregion

		#region EnterLineBreak
		static void OnEnter(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.IsKeyboardFocused) {
				textArea.PerformTextInput("\n");
				args.Handled = true;
			}
		}
		#endregion

		#region Tab
		static void OnTab(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				using (textArea.Document.RunUpdate()) {
					if (textArea.Selection.IsMultiline) {
						var segment = textArea.Selection.SurroundingSegment;
						DocumentLine start = textArea.Document.GetLineByOffset(segment.Offset);
						DocumentLine end = textArea.Document.GetLineByOffset(segment.EndOffset);
						// don't include the last line if no characters on it are selected
						if (start != end && end.Offset == segment.EndOffset)
							end = end.PreviousLine;
						DocumentLine current = start;
						while (true) {
							int offset = current.Offset;
							if (textArea.ReadOnlySectionProvider.CanInsert(offset))
								textArea.Document.Replace(offset, 0, textArea.Options.IndentationString, OffsetChangeMappingType.KeepAnchorBeforeInsertion);
							if (current == end)
								break;
							current = current.NextLine;
						}
					} else {
						string indentationString = textArea.Options.GetIndentationString(textArea.Caret.Column);
						textArea.ReplaceSelectionWithText(indentationString);
					}
				}
				textArea.Caret.BringCaretToView();
				args.Handled = true;
			}
		}

		static void OnShiftTab(object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedLines(
				delegate (TextArea textArea, DocumentLine line) {
					int offset = line.Offset;
					ISegment s = TextUtilities.GetSingleIndentationSegment(textArea.Document, offset, textArea.Options.IndentationSize);
					if (s.Length > 0) {
						s = textArea.GetDeletableSegments(s).FirstOrDefault();
						if (s != null && s.Length > 0) {
							textArea.Document.Remove(s.Offset, s.Length);
						}
					}
				}, target, args, DefaultSegmentType.CurrentLine);
		}
		#endregion

		#region Delete
		static ExecutedRoutedEventHandler OnDelete(CaretMovementType caretMovement)
		{
			return (target, args) => {
				TextArea textArea = GetTextArea(target);
				if (textArea != null && textArea.Document != null) {
					if (textArea.Selection.IsEmpty) {
						TextViewPosition startPos = textArea.Caret.Position;
						bool enableVirtualSpace = textArea.Options.EnableVirtualSpace;
						// When pressing delete; don't move the caret further into virtual space - instead delete the newline
						if (caretMovement == CaretMovementType.CharRight)
							enableVirtualSpace = false;
						double desiredXPos = textArea.Caret.DesiredXPos;
						TextViewPosition endPos = CaretNavigationCommandHandler.GetNewCaretPosition(
							textArea.TextView, startPos, caretMovement, enableVirtualSpace, ref desiredXPos);
						// GetNewCaretPosition may return (0,0) as new position,
						// thus we need to validate endPos before using it in the selection.
						if (endPos.Line < 1 || endPos.Column < 1)
							endPos = new TextViewPosition(Math.Max(endPos.Line, 1), Math.Max(endPos.Column, 1));
						// Don't do anything if the number of lines of a rectangular selection would be changed by the deletion.
						if (textArea.Selection is RectangleSelection && startPos.Line != endPos.Line)
							return;
						// Don't select the text to be deleted; just reuse the ReplaceSelectionWithText logic
						// Reuse the existing selection, so that we continue using the same logic
						textArea.Selection.StartSelectionOrSetEndpoint(startPos, endPos)
							.ReplaceSelectionWithText(string.Empty);
					} else {
						textArea.RemoveSelectedText();
					}
					textArea.Caret.BringCaretToView();
					args.Handled = true;
				}
			};
		}

		static void CanDelete(object target, CanExecuteRoutedEventArgs args)
		{
			// HasSomethingSelected for delete command
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				args.CanExecute = !textArea.Selection.IsEmpty;
				args.Handled = true;
			}
		}
		#endregion

		#region Clipboard commands
		static void CanCutOrCopy(object target, CanExecuteRoutedEventArgs args)
		{
			// HasSomethingSelected for copy and cut commands
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				args.CanExecute = textArea.Options.CutCopyWholeLine || !textArea.Selection.IsEmpty;
				args.Handled = true;
			}
		}

		static void OnCopy(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				if (textArea.Selection.IsEmpty && textArea.Options.CutCopyWholeLine) {
					DocumentLine currentLine = textArea.Document.GetLineByNumber(textArea.Caret.Line);
					CopyWholeLine(textArea, currentLine);
				} else {
					CopySelectedText(textArea);
				}
				args.Handled = true;
			}
		}

		static void OnCut(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				if (textArea.Selection.IsEmpty && textArea.Options.CutCopyWholeLine) {
					DocumentLine currentLine = textArea.Document.GetLineByNumber(textArea.Caret.Line);
					if (CopyWholeLine(textArea, currentLine)) {
						ISegment[] segmentsToDelete = textArea.GetDeletableSegments(new SimpleSegment(currentLine.Offset, currentLine.TotalLength));
						for (int i = segmentsToDelete.Length - 1; i >= 0; i--) {
							textArea.Document.Remove(segmentsToDelete[i]);
						}
					}
				} else {
					if (CopySelectedText(textArea))
						textArea.RemoveSelectedText();
				}
				textArea.Caret.BringCaretToView();
				args.Handled = true;
			}
		}

		static bool CopySelectedText(TextArea textArea)
		{
			var data = textArea.Selection.CreateDataObject(textArea);
			var copyingEventArgs = new DataObjectCopyingEventArgs(data, false);
			textArea.RaiseEvent(copyingEventArgs);
			if (copyingEventArgs.CommandCancelled)
				return false;

			try {
				Clipboard.SetDataObject(data, true);
			} catch (ExternalException) {
				// Apparently this exception sometimes happens randomly.
				// The MS controls just ignore it, so we'll do the same.
			}

			string text = textArea.Selection.GetText();
			text = TextUtilities.NormalizeNewLines(text, Environment.NewLine);
			textArea.OnTextCopied(new TextEventArgs(text));
			return true;
		}

		const string LineSelectedType = "MSDEVLineSelect";  // This is the type VS 2003 and 2005 use for flagging a whole line copy

		public static bool ConfirmDataFormat(TextArea textArea, DataObject dataObject, string format)
		{
			var e = new DataObjectSettingDataEventArgs(dataObject, format);
			textArea.RaiseEvent(e);
			return !e.CommandCancelled;
		}

		static bool CopyWholeLine(TextArea textArea, DocumentLine line)
		{
			ISegment wholeLine = new SimpleSegment(line.Offset, line.TotalLength);
			string text = textArea.Document.GetText(wholeLine);
			// Ensure we use the appropriate newline sequence for the OS
			text = TextUtilities.NormalizeNewLines(text, Environment.NewLine);
			DataObject data = new DataObject();
			if (ConfirmDataFormat(textArea, data, DataFormats.UnicodeText))
				data.SetText(text);

			// Also copy text in HTML format to clipboard - good for pasting text into Word
			// or to the SharpDevelop forums.
			if (ConfirmDataFormat(textArea, data, DataFormats.Html)) {
				IHighlighter highlighter = textArea.GetService(typeof(IHighlighter)) as IHighlighter;
				HtmlClipboard.SetHtml(data, HtmlClipboard.CreateHtmlFragment(textArea.Document, highlighter, wholeLine, new HtmlOptions(textArea.Options)));
			}

			if (ConfirmDataFormat(textArea, data, LineSelectedType)) {
				MemoryStream lineSelected = new MemoryStream(1);
				lineSelected.WriteByte(1);
				data.SetData(LineSelectedType, lineSelected, false);
			}

			var copyingEventArgs = new DataObjectCopyingEventArgs(data, false);
			textArea.RaiseEvent(copyingEventArgs);
			if (copyingEventArgs.CommandCancelled)
				return false;

			try {
				Clipboard.SetDataObject(data, true);
			} catch (ExternalException) {
				// Apparently this exception sometimes happens randomly.
				// The MS controls just ignore it, so we'll do the same.
				return false;
			}
			textArea.OnTextCopied(new TextEventArgs(text));
			return true;
		}

		static void CanPaste(object target, CanExecuteRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				args.CanExecute = textArea.ReadOnlySectionProvider.CanInsert(textArea.Caret.Offset)
					&& Clipboard.ContainsText();
				// WPF Clipboard.ContainsText() is safe to call without catching ExternalExceptions
				// because it doesn't try to lock the clipboard - it just peeks inside with IsClipboardFormatAvailable().
				args.Handled = true;
			}
		}

		static void OnPaste(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				IDataObject dataObject;
				try {
					dataObject = Clipboard.GetDataObject();
				} catch (ExternalException) {
					return;
				}
				if (dataObject == null)
					return;

				var pastingEventArgs = new DataObjectPastingEventArgs(dataObject, false, DataFormats.UnicodeText);
				textArea.RaiseEvent(pastingEventArgs);
				if (pastingEventArgs.CommandCancelled)
					return;

				string text = GetTextToPaste(pastingEventArgs, textArea);

				if (!string.IsNullOrEmpty(text)) {
					dataObject = pastingEventArgs.DataObject;
					bool fullLine = textArea.Options.CutCopyWholeLine && dataObject.GetDataPresent(LineSelectedType);
					bool rectangular = dataObject.GetDataPresent(RectangleSelection.RectangularSelectionDataType);

					if (fullLine) {
						DocumentLine currentLine = textArea.Document.GetLineByNumber(textArea.Caret.Line);
						if (textArea.ReadOnlySectionProvider.CanInsert(currentLine.Offset)) {
							textArea.Document.Insert(currentLine.Offset, text);
						}
					} else if (rectangular && textArea.Selection.IsEmpty && !(textArea.Selection is RectangleSelection)) {
						if (!RectangleSelection.PerformRectangularPaste(textArea, textArea.Caret.Position, text, false))
							textArea.ReplaceSelectionWithText(text);
					} else {
						textArea.ReplaceSelectionWithText(text);
					}
				}
				textArea.Caret.BringCaretToView();
				args.Handled = true;
			}
		}

		internal static string GetTextToPaste(DataObjectPastingEventArgs pastingEventArgs, TextArea textArea)
		{
			var dataObject = pastingEventArgs.DataObject;
			if (dataObject == null)
				return null;
			try {
				string text;
				// Try retrieving the text as one of:
				//  - the FormatToApply
				//  - UnicodeText
				//  - Text
				// (but don't try the same format twice)
				if (pastingEventArgs.FormatToApply != null && dataObject.GetDataPresent(pastingEventArgs.FormatToApply))
					text = (string)dataObject.GetData(pastingEventArgs.FormatToApply);
				else if (pastingEventArgs.FormatToApply != DataFormats.UnicodeText && dataObject.GetDataPresent(DataFormats.UnicodeText))
					text = (string)dataObject.GetData(DataFormats.UnicodeText);
				else if (pastingEventArgs.FormatToApply != DataFormats.Text && dataObject.GetDataPresent(DataFormats.Text))
					text = (string)dataObject.GetData(DataFormats.Text);
				else
					return null; // no text data format
								 // convert text back to correct newlines for this document
				string newLine = TextUtilities.GetNewLineFromDocument(textArea.Document, textArea.Caret.Line);
				text = TextUtilities.NormalizeNewLines(text, newLine);
				text = textArea.Options.ConvertTabsToSpaces ? text.Replace("\t", new String(' ', textArea.Options.IndentationSize)) : text;
				return text;
			} catch (OutOfMemoryException) {
				// may happen when trying to paste a huge string
				return null;
			} catch (COMException) {
				// may happen with incorrect data => Data on clipboard is invalid (Exception from HRESULT: 0x800401D3 (CLIPBRD_E_BAD_DATA))
				return null;
			}
		}
		#endregion

		#region Toggle Overstrike
		static void OnToggleOverstrike(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Options.AllowToggleOverstrikeMode)
				textArea.OverstrikeMode = !textArea.OverstrikeMode;
		}
		#endregion

		#region DeleteLine
		static void OnDeleteLine(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null) {
				int firstLineIndex, lastLineIndex;
				if (textArea.Selection.Length == 0) {
					// There is no selection, simply delete current line
					firstLineIndex = lastLineIndex = textArea.Caret.Line;
				} else {
					// There is a selection, remove all lines affected by it (use Min/Max to be independent from selection direction)
					firstLineIndex = Math.Min(textArea.Selection.StartPosition.Line, textArea.Selection.EndPosition.Line);
					lastLineIndex = Math.Max(textArea.Selection.StartPosition.Line, textArea.Selection.EndPosition.Line);
				}
				DocumentLine startLine = textArea.Document.GetLineByNumber(firstLineIndex);
				DocumentLine endLine = textArea.Document.GetLineByNumber(lastLineIndex);
				textArea.Selection = Selection.Create(textArea, startLine.Offset, endLine.Offset + endLine.TotalLength);
				textArea.RemoveSelectedText();
				args.Handled = true;
			}
		}
		#endregion

		#region Remove..Whitespace / Convert Tabs-Spaces
		static void OnRemoveLeadingWhitespace(object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedLines(
				delegate (TextArea textArea, DocumentLine line) {
					textArea.Document.Remove(TextUtilities.GetLeadingWhitespace(textArea.Document, line));
				}, target, args, DefaultSegmentType.WholeDocument);
		}

		static void OnRemoveTrailingWhitespace(object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedLines(
				delegate (TextArea textArea, DocumentLine line) {
					textArea.Document.Remove(TextUtilities.GetTrailingWhitespace(textArea.Document, line));
				}, target, args, DefaultSegmentType.WholeDocument);
		}

		static void OnConvertTabsToSpaces(object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedSegments(ConvertTabsToSpaces, target, args, DefaultSegmentType.WholeDocument);
		}

		static void OnConvertLeadingTabsToSpaces(object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedLines(
				delegate (TextArea textArea, DocumentLine line) {
					ConvertTabsToSpaces(textArea, TextUtilities.GetLeadingWhitespace(textArea.Document, line));
				}, target, args, DefaultSegmentType.WholeDocument);
		}

		static void ConvertTabsToSpaces(TextArea textArea, ISegment segment)
		{
			TextDocument document = textArea.Document;
			int endOffset = segment.EndOffset;
			string indentationString = new string(' ', textArea.Options.IndentationSize);
			for (int offset = segment.Offset; offset < endOffset; offset++) {
				if (document.GetCharAt(offset) == '\t') {
					document.Replace(offset, 1, indentationString, OffsetChangeMappingType.CharacterReplace);
					endOffset += indentationString.Length - 1;
				}
			}
		}

		static void OnConvertSpacesToTabs(object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedSegments(ConvertSpacesToTabs, target, args, DefaultSegmentType.WholeDocument);
		}

		static void OnConvertLeadingSpacesToTabs(object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedLines(
				delegate (TextArea textArea, DocumentLine line) {
					ConvertSpacesToTabs(textArea, TextUtilities.GetLeadingWhitespace(textArea.Document, line));
				}, target, args, DefaultSegmentType.WholeDocument);
		}

		static void ConvertSpacesToTabs(TextArea textArea, ISegment segment)
		{
			TextDocument document = textArea.Document;
			int endOffset = segment.EndOffset;
			int indentationSize = textArea.Options.IndentationSize;
			int spacesCount = 0;
			for (int offset = segment.Offset; offset < endOffset; offset++) {
				if (document.GetCharAt(offset) == ' ') {
					spacesCount++;
					if (spacesCount == indentationSize) {
						document.Replace(offset - (indentationSize - 1), indentationSize, "\t", OffsetChangeMappingType.CharacterReplace);
						spacesCount = 0;
						offset -= indentationSize - 1;
						endOffset -= indentationSize - 1;
					}
				} else {
					spacesCount = 0;
				}
			}
		}
		#endregion

		#region Convert...Case
		static void ConvertCase(Func<string, string> transformText, object target, ExecutedRoutedEventArgs args)
		{
			TransformSelectedSegments(
				delegate (TextArea textArea, ISegment segment) {
					string oldText = textArea.Document.GetText(segment);
					string newText = transformText(oldText);
					textArea.Document.Replace(segment.Offset, segment.Length, newText, OffsetChangeMappingType.CharacterReplace);
				}, target, args, DefaultSegmentType.WholeDocument);
		}

		static void OnConvertToUpperCase(object target, ExecutedRoutedEventArgs args)
		{
			ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToUpper, target, args);
		}

		static void OnConvertToLowerCase(object target, ExecutedRoutedEventArgs args)
		{
			ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToLower, target, args);
		}

		static void OnConvertToTitleCase(object target, ExecutedRoutedEventArgs args)
		{
			ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToTitleCase, target, args);
		}

		static void OnInvertCase(object target, ExecutedRoutedEventArgs args)
		{
			ConvertCase(InvertCase, target, args);
		}

		static string InvertCase(string text)
		{
			CultureInfo culture = CultureInfo.CurrentCulture;
			char[] buffer = text.ToCharArray();
			for (int i = 0; i < buffer.Length; ++i) {
				char c = buffer[i];
				buffer[i] = char.IsUpper(c) ? char.ToLower(c, culture) : char.ToUpper(c, culture);
			}
			return new string(buffer);
		}
		#endregion

		static void OnIndentSelection(object target, ExecutedRoutedEventArgs args)
		{
			TextArea textArea = GetTextArea(target);
			if (textArea != null && textArea.Document != null && textArea.IndentationStrategy != null) {
				using (textArea.Document.RunUpdate()) {
					int start, end;
					if (textArea.Selection.IsEmpty) {
						start = 1;
						end = textArea.Document.LineCount;
					} else {
						start = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.Offset).LineNumber;
						end = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.EndOffset).LineNumber;
					}
					textArea.IndentationStrategy.IndentLines(textArea.Document, start, end);
				}
				textArea.Caret.BringCaretToView();
				args.Handled = true;
			}
		}
	}
}
