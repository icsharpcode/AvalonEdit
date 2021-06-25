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
using ICSharpCode.AvalonEdit.Document;

namespace ICSharpCode.AvalonEdit.Editing
{
	/// <summary>
	/// Base class used by the CaretPreserver and SelectionPreserver
	/// </summary>
	abstract class CaretSelectionPreserver
			: IDisposable
	{
		protected CaretSelectionPreserver(TextArea _textArea)
		{
			isDisposed = false;
			textArea = _textArea;
		}

		public static CaretSelectionPreserver Create(TextArea textArea)
		{
			if (textArea.Selection.IsEmpty) {
				return new CaretPreserver(textArea);
			} else {
				return new SelectionPreserver(textArea);
			}
		}

		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			Restore();
		}

		public abstract void Restore();
		public abstract void MoveLine(int i);

		private bool isDisposed;

		private TextArea textArea;
		public TextArea TextArea { get { return textArea; } }
	}

	/// <summary>
	/// This class moves the current caret position when a line is moved up or down
	/// </summary>
	class CaretPreserver
		: CaretSelectionPreserver
	{
		public CaretPreserver(TextArea textArea)
			: base(textArea)
		{
			caretLocation = textArea.Caret.Location;
		}

		public override void Restore()
		{
			TextArea.Caret.Location = caretLocation;
		}

		public override void MoveLine(int i)
		{
			caretLocation = new TextLocation(caretLocation.Line + i, caretLocation.Column);
		}

		TextLocation caretLocation;
	}

	/// <summary>
	/// This class moves the current selection when the lines containing that selection
	/// are moved up or down.
	///
	/// NOTE: The SelectionPreserver must inherit from the CaretPreserver as if the caret 
	/// is not within the selection then the selection is not considered valid and is 
	/// cleared. By inheriting from the CaretPreserver we move both the selection and 
	/// the caret at the same time.
	/// </summary> 
	class SelectionPreserver
		: CaretPreserver
	{
		public SelectionPreserver(TextArea textArea)
			: base(textArea)
		{
			startLocation = textArea.Selection.StartPosition.Location;
			endLocation = textArea.Selection.EndPosition.Location;
		}

		public override void Restore()
		{
			base.Restore();
			TextArea.Selection = Selection.Create(TextArea, new TextViewPosition(startLocation), new TextViewPosition(endLocation));
		}


		public override void MoveLine(int i)
		{
			base.MoveLine(i);
			startLocation = new TextLocation(startLocation.Line + i, startLocation.Column);
			endLocation = new TextLocation(endLocation.Line + i, endLocation.Column);
		}

		TextLocation startLocation, endLocation;
	}
}
