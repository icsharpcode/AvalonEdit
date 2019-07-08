// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using ICSharpCode.AvalonEdit.Document;

namespace ICSharpCode.AvalonEdit.Editing
{
	class TextAreaAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, ITextProvider
	{
		public TextAreaAutomationPeer(TextArea owner)
			: base(owner)
		{
			owner.Caret.PositionChanged += OnSelectionChanged;
			owner.SelectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			Debug.WriteLine("RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged)");
			RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
		}

		private TextArea TextArea { get { return (TextArea)base.Owner; } }

		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Document;
		}

		internal IRawElementProviderSimple Provider {
			get { return ProviderFromPeer(this); }
		}

		public bool IsReadOnly {
			get { return TextArea.ReadOnlySectionProvider == ReadOnlySectionDocument.Instance; }
		}

		public void SetValue(string value)
		{
			TextArea.Document.Text = value;
		}

		public string Value {
			get { return TextArea.Document.Text; }
		}

		public ITextRangeProvider DocumentRange {
			get {
				Debug.WriteLine("TextAreaAutomationPeer.get_DocumentRange()");
				return new TextRangeProvider(TextArea, TextArea.Document, 0, TextArea.Document.TextLength);
			}
		}

		public ITextRangeProvider[] GetSelection()
		{
			Debug.WriteLine("TextAreaAutomationPeer.GetSelection()");
			if (TextArea.Selection.IsEmpty) {
				var anchor = TextArea.Document.CreateAnchor(TextArea.Caret.Offset);
				anchor.SurviveDeletion = true;
				return new ITextRangeProvider[] { new TextRangeProvider(TextArea, TextArea.Document, new AnchorSegment(anchor, anchor)) };
			}
			return TextArea.Selection.Segments.Select(s => new TextRangeProvider(TextArea, TextArea.Document, s)).ToArray();
		}

		public ITextRangeProvider[] GetVisibleRanges()
		{
			Debug.WriteLine("TextAreaAutomationPeer.GetVisibleRanges()");
			throw new NotImplementedException();
		}

		public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement)
		{
			Debug.WriteLine("TextAreaAutomationPeer.RangeFromChild()");
			throw new NotImplementedException();
		}

		public ITextRangeProvider RangeFromPoint(System.Windows.Point screenLocation)
		{
			Debug.WriteLine("TextAreaAutomationPeer.RangeFromPoint()");
			throw new NotImplementedException();
		}

		public SupportedTextSelection SupportedTextSelection {
			get { return SupportedTextSelection.Single; }
		}

		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Text)
				return this;
			if (patternInterface == PatternInterface.Value)
				return this;
			if (patternInterface == PatternInterface.Scroll) {
				TextEditor editor = TextArea.GetService(typeof(TextEditor)) as TextEditor;
				if (editor != null)
					return FromElement(editor).GetPattern(patternInterface);
			}
			return base.GetPattern(patternInterface);
		}
	}
}
