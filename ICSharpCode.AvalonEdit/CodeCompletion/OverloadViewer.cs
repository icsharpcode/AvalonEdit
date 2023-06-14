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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AcAvalonEdit.CodeCompletion
{
	/// <summary>
	/// Represents a text between "Up" and "Down" buttons.
	/// </summary>
	public class OverloadViewer : Control
	{
		static OverloadViewer()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(OverloadViewer),
													 new FrameworkPropertyMetadata(typeof(OverloadViewer)));
		}

		/// <summary>
		/// The text property.
		/// </summary>
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(OverloadViewer));

		/// <summary>
		/// Gets/Sets the text between the Up and Down buttons.
		/// </summary>
		public string Text {
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <inheritdoc/>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Button upButton = (Button)this.Template.FindName("PART_UP", this);
			upButton.Click += (sender, e) => {
				e.Handled = true;
				ChangeIndex(-1);
			};

			Button downButton = (Button)this.Template.FindName("PART_DOWN", this);
			downButton.Click += (sender, e) => {
				e.Handled = true;
				ChangeIndex(+1);
			};
		}

		/// <summary>
		/// The ItemProvider property.
		/// </summary>
		public static readonly DependencyProperty ProviderProperty =
			DependencyProperty.Register("Provider", typeof(IOverloadProvider), typeof(OverloadViewer));

		/// <summary>
		/// Gets/Sets the item provider.
		/// </summary>
		public IOverloadProvider Provider {
			get { return (IOverloadProvider)GetValue(ProviderProperty); }
			set { SetValue(ProviderProperty, value); }
		}

		/// <summary>
		/// Changes the selected index.
		/// </summary>
		/// <param name="relativeIndexChange">The relative index change - usual values are +1 or -1.</param>
		public void ChangeIndex(int relativeIndexChange)
		{
			IOverloadProvider p = this.Provider;
			if (p != null) {
				int newIndex = p.SelectedIndex + relativeIndexChange;
				if (newIndex < 0)
					newIndex = p.Count - 1;
				if (newIndex >= p.Count)
					newIndex = 0;
				p.SelectedIndex = newIndex;
			}
		}
	}

	sealed class CollapseIfSingleOverloadConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((int)value < 2) ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
