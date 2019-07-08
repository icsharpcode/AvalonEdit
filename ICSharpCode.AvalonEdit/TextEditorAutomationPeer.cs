// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit
{
	/// <summary>
	/// Exposes <see cref="ICSharpCode.AvalonEdit.TextEditor"/> to automation.
	/// </summary>
	public class TextEditorAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
	{
		/// <summary>
		/// Creates a new TextEditorAutomationPeer instance.
		/// </summary>
		public TextEditorAutomationPeer(TextEditor owner) : base(owner)
		{
			Debug.WriteLine("TextEditorAutomationPeer was created");
		}

		private TextEditor TextEditor {
			get { return (TextEditor)base.Owner; }
		}

		void IValueProvider.SetValue(string value)
		{
			this.TextEditor.Text = value;
		}

		string IValueProvider.Value {
			get { return this.TextEditor.Text; }
		}

		bool IValueProvider.IsReadOnly {
			get { return this.TextEditor.IsReadOnly; }
		}

		/// <inheritdoc/>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Document;
		}

		/// <inheritdoc/>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Value)
				return this;

			if (patternInterface == PatternInterface.Scroll) {
				ScrollViewer scrollViewer = this.TextEditor.ScrollViewer;
				if (scrollViewer != null)
					return UIElementAutomationPeer.FromElement(scrollViewer);
			}

			if (patternInterface == PatternInterface.Text) {
				return FromElement(this.TextEditor.TextArea);
			}

			return base.GetPattern(patternInterface);
		}

		internal void RaiseIsReadOnlyChanged(bool oldValue, bool newValue)
		{
			RaisePropertyChangedEvent(ValuePatternIdentifiers.IsReadOnlyProperty, Boxes.Box(oldValue), Boxes.Box(newValue));
		}
	}
}
