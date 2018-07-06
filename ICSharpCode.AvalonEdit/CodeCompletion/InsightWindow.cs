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

using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
	/// <summary>
	/// A popup-like window that is attached to a text segment.
	/// </summary>
	public class InsightWindow : CompletionWindowBase
	{
		static InsightWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(InsightWindow),
													 new FrameworkPropertyMetadata(typeof(InsightWindow)));
			AllowsTransparencyProperty.OverrideMetadata(typeof(InsightWindow),
														new FrameworkPropertyMetadata(Boxes.True));
		}

		/// <summary>
		/// Creates a new InsightWindow.
		/// </summary>
		public InsightWindow(TextArea textArea) : base(textArea)
		{
			this.CloseAutomatically = true;
			AttachEvents();
		}

		/// <inheritdoc/>
		protected override void OnSourceInitialized(EventArgs e)
		{
			Rect caret = this.TextArea.Caret.CalculateCaretRectangle();
			Point pointOnScreen = this.TextArea.TextView.PointToScreen(caret.Location - this.TextArea.TextView.ScrollOffset);
			Rect workingArea = System.Windows.Forms.Screen.FromPoint(pointOnScreen.ToSystemDrawing()).WorkingArea.ToWpf().TransformFromDevice(this);

			MaxHeight = workingArea.Height;
			MaxWidth = Math.Min(workingArea.Width, Math.Max(1000, workingArea.Width * 0.6));

			base.OnSourceInitialized(e);
		}

		/// <summary>
		/// Gets/Sets whether the insight window should close automatically.
		/// The default value is true.
		/// </summary>
		public bool CloseAutomatically { get; set; }

		/// <inheritdoc/>
		protected override bool CloseOnFocusLost
		{
			get { return this.CloseAutomatically; }
		}

		private void AttachEvents()
		{
			this.TextArea.Caret.PositionChanged += CaretPositionChanged;
		}

		/// <inheritdoc/>
		protected override void DetachEvents()
		{
			this.TextArea.Caret.PositionChanged -= CaretPositionChanged;
			base.DetachEvents();
		}

		private void CaretPositionChanged(object sender, EventArgs e)
		{
			if (this.CloseAutomatically)
			{
				int offset = this.TextArea.Caret.Offset;
				if (offset < this.StartOffset || offset > this.EndOffset)
				{
					Close();
				}
			}
		}
	}

	/// <summary>
	/// TemplateSelector for InsightWindow to replace plain string content by a TextBlock with TextWrapping.
	/// </summary>
	internal sealed class InsightWindowTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is string)
				return (DataTemplate)((FrameworkElement)container).FindResource("TextBlockTemplate");

			return null;
		}
	}
}