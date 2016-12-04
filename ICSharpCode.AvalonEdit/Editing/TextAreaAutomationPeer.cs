using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Editing
{
	class TextAreaAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, ITextProvider
	{
		public TextAreaAutomationPeer(TextArea owner)
			: base(owner)
		{
		}
		private TextArea TextArea { get { return (TextArea)base.Owner; } }
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Document;
		}

		public bool IsReadOnly
		{
			get { return (TextArea.ReadOnlySectionProvider != NoReadOnlySections.Instance);  }
		}

		public void SetValue(string value)
		{
		}

		public string Value
		{
			get { return ""; }
		}

		public ITextRangeProvider DocumentRange
		{
			get { return new TextRangeProvider(TextArea.Document, 0, TextArea.Document.TextLength); }
		}

		public ITextRangeProvider[] GetSelection()
		{
			if (TextArea.Selection.IsEmpty)
			{
				return new ITextRangeProvider[] { new TextRangeProvider(TextArea.Document, TextArea.Caret.Offset, TextArea.Caret.Offset) };
			}
			var segments = TextArea.Selection.Segments;
			return null;
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
			return base.GetPattern(patternInterface);
		}
	}
}
