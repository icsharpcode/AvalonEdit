﻿// Copyright (c) 2016 Daniel Grunwald
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
using System;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ICSharpCode.AvalonEdit.Editing
{
	internal class TextAreaAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, ITextProvider
	{
		public TextAreaAutomationPeer(TextArea owner)
			: base(owner)
		{
			owner.Caret.PositionChanged += OnSelectionChanged;
			owner.SelectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
		}

		private TextArea TextArea { get { return (TextArea)base.Owner; } }

		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Document;
		}

		internal IRawElementProviderSimple Provider
		{
			get { return ProviderFromPeer(this); }
		}

		public bool IsReadOnly
		{
			get { return TextArea.ReadOnlySectionProvider == ReadOnlySectionDocument.Instance; }
		}

		public void SetValue(string value)
		{
			TextArea.Document.Text = value;
		}

		public string Value
		{
			get { return TextArea.Document.Text; }
		}

		public ITextRangeProvider DocumentRange
		{
			get { return new TextRangeProvider(TextArea, TextArea.Document, 0, TextArea.Document.TextLength); }
		}

		public ITextRangeProvider[] GetSelection()
		{
			if (TextArea.Selection.IsEmpty)
			{
				var anchor = TextArea.Document.CreateAnchor(TextArea.Caret.Offset);
				anchor.SurviveDeletion = true;
				return new ITextRangeProvider[] { new TextRangeProvider(TextArea, TextArea.Document, new AnchorSegment(anchor, anchor)) };
			}
			return TextArea.Selection.Segments.Select(s => new TextRangeProvider(TextArea, TextArea.Document, s)).ToArray();
		}

		public ITextRangeProvider[] GetVisibleRanges()
		{
			throw new NotImplementedException();
		}

		public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement)
		{
			throw new NotImplementedException();
		}

		public ITextRangeProvider RangeFromPoint(System.Windows.Point screenLocation)
		{
			throw new NotImplementedException();
		}

		public SupportedTextSelection SupportedTextSelection
		{
			get { return SupportedTextSelection.Single; }
		}

		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Text)
				return this;
			if (patternInterface == PatternInterface.Value)
				return this;
			if (patternInterface == PatternInterface.Scroll)
			{
				TextEditor editor = TextArea.GetService(typeof(TextEditor)) as TextEditor;
				if (editor != null)
					return FromElement(editor).GetPattern(patternInterface);
			}
			return base.GetPattern(patternInterface);
		}
	}
}