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

using System.Windows;
using System.Windows.Input;

using AcAvalonEdit.Editing;

namespace AcAvalonEdit.CodeCompletion
{
	/// <summary>
	/// Insight window that shows an OverloadViewer.
	/// </summary>
	public class OverloadInsightWindow : InsightWindow
	{
		OverloadViewer overloadViewer = new OverloadViewer();

		/// <summary>
		/// Creates a new OverloadInsightWindow.
		/// </summary>
		public OverloadInsightWindow(TextArea textArea) : base(textArea)
		{
			overloadViewer.Margin = new Thickness(2, 0, 0, 0);
			this.Content = overloadViewer;
		}

		/// <summary>
		/// Gets/Sets the item provider.
		/// </summary>
		public IOverloadProvider Provider {
			get { return overloadViewer.Provider; }
			set { overloadViewer.Provider = value; }
		}

		/// <inheritdoc/>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled && this.Provider != null && this.Provider.Count > 1) {
				switch (e.Key) {
					case Key.Up:
						e.Handled = true;
						overloadViewer.ChangeIndex(-1);
						break;
					case Key.Down:
						e.Handled = true;
						overloadViewer.ChangeIndex(+1);
						break;
				}
				if (e.Handled) {
					UpdateLayout();
					UpdatePosition();
				}
			}
		}
	}
}
