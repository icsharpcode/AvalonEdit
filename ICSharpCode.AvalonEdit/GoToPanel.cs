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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;

namespace ICSharpCode.AvalonEdit.Search
{
	/// <summary>
	/// Provides go to functionality for AvalonEdit. It is displayed in the top-right corner of the TextArea.
	/// </summary>
	public class GoToPanel : Control
	{
		TextArea textArea;
		GoToInputHandler handler;
		TextDocument currentDocument;
		SearchResultBackgroundRenderer renderer;
		TextBox searchTextBox;
		Popup dropdownPopup;
        GoToPanelAdorner adorner;

        #region

        /// <summary>
        /// Dependency property for <see cref="SearchPattern"/>.
        /// </summary>
        public static readonly DependencyProperty SearchPatternProperty =
            DependencyProperty.Register("SearchPattern", typeof(string), typeof(GoToPanel));
		
		/// <summary>
		/// Gets/sets the search pattern.
		/// </summary>
		public string SearchPattern {
			get { return (string)GetValue(SearchPatternProperty); }
			set { SetValue(SearchPatternProperty, value); }
		}
		
		/// <summary>
		/// Dependency property for <see cref="MarkerBrush"/>.
		/// </summary>
		public static readonly DependencyProperty MarkerBrushProperty =
			DependencyProperty.Register("MarkerBrush", typeof(Brush), typeof(GoToPanel),
			                            new FrameworkPropertyMetadata(Brushes.LightGreen, MarkerBrushChangedCallback));
		
		/// <summary>
		/// Gets/sets the Brush used for marking search results in the TextView.
		/// </summary>
		public Brush MarkerBrush {
			get { return (Brush)GetValue(MarkerBrushProperty); }
			set { SetValue(MarkerBrushProperty, value); }
		}
		
		/// <summary>
		/// Dependency property for <see cref="Localization"/>.
		/// </summary>
		public static readonly DependencyProperty LocalizationProperty =
			DependencyProperty.Register("Localization", typeof(Localization), typeof(GoToPanel),
			                            new FrameworkPropertyMetadata(new Localization()));
		
		/// <summary> 
		/// Gets/sets the localization for the GoToPanel.
		/// </summary>
		public Localization Localization {
			get { return (Localization)GetValue(LocalizationProperty); }
			set { SetValue(LocalizationProperty, value); }
		}
		#endregion
		
		static void MarkerBrushChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            GoToPanel panel = d as GoToPanel;
			if (panel != null) {
				panel.renderer.MarkerBrush = (Brush)e.NewValue;
			}
		}
		
		static GoToPanel()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(GoToPanel), new FrameworkPropertyMetadata(typeof(GoToPanel)));
		}
		
		ISearchStrategy strategy;
		
        /// <summary>
        /// Creates a new GoToPanel.
        /// </summary>
        GoToPanel()
		{
		}

        /// <summary>
        /// Attaches this GoToPanel to a TextArea instance.
        /// </summary>
        [Obsolete("Use the Install method instead")]
		public void Attach(TextArea textArea)
		{
			if (textArea == null)
				throw new ArgumentNullException("textArea");
			AttachInternal(textArea);
		}
		
		/// <summary>
		/// Creates a GoToPanel and installs it to the TextEditor's TextArea.
		/// </summary>
		/// <remarks>This is a convenience wrapper.</remarks>
		public static GoToPanel Install(TextEditor editor)
		{
			if (editor == null)
				throw new ArgumentNullException("editor");
			return Install(editor.TextArea);
		}
		
		/// <summary>
		/// Creates a GoToPanel and installs it to the TextArea.
		/// </summary>
		public static GoToPanel Install(TextArea textArea)
		{
			if (textArea == null)
				throw new ArgumentNullException("textArea");
            GoToPanel panel = new GoToPanel();
			panel.AttachInternal(textArea);
			panel.handler = new GoToInputHandler(textArea, panel);
			textArea.DefaultInputHandler.NestedInputHandlers.Add(panel.handler);
			return panel;
		}

        /// <summary>
        /// Adds the commands used by GoToPanel to the given CommandBindingCollection.
        /// </summary>
        public void RegisterCommands(CommandBindingCollection commandBindings)
		{
			handler.RegisterGlobalCommands(commandBindings);
		}
		
		/// <summary>
		/// Removes the GoToPanel from the TextArea.
		/// </summary>
		public void Uninstall()
		{
			CloseAndRemove();
			textArea.DefaultInputHandler.NestedInputHandlers.Remove(handler);
		}
		
		void AttachInternal(TextArea textArea)
		{
			this.textArea = textArea;
			adorner = new GoToPanelAdorner(textArea, this);
			DataContext = this;
			
			renderer = new SearchResultBackgroundRenderer();
			currentDocument = textArea.Document;
			if (currentDocument != null)
				currentDocument.TextChanged += textArea_Document_TextChanged;
			textArea.DocumentChanged += textArea_DocumentChanged;
			KeyDown += SearchLayerKeyDown;

            this.CommandBindings.Add(new CommandBinding(SearchCommands.GoTo, (sender, e) => GoToNext()));
			this.CommandBindings.Add(new CommandBinding(SearchCommands.CloseGoToPanel, (sender, e) => Close()));
			IsClosed = true;
		}

		void textArea_DocumentChanged(object sender, EventArgs e)
		{
			if (currentDocument != null)
				currentDocument.TextChanged -= textArea_Document_TextChanged;
			currentDocument = textArea.Document;
			if (currentDocument != null) {
				currentDocument.TextChanged += textArea_Document_TextChanged;
				//DoSearch(false);
			}
		}

		void textArea_Document_TextChanged(object sender, EventArgs e)
		{
			//DoSearch(false);
		}
		
		/// <inheritdoc/>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			searchTextBox = Template.FindName("PART_searchTextBox", this) as TextBox;
			dropdownPopup = Template.FindName("PART_dropdownPopup", this) as Popup;
		}
		
		
		/// <summary>
		/// Reactivates the GoToPanel by setting the focus on the search box and selecting all text.
		/// </summary>
		public void Reactivate()
		{
			if (searchTextBox == null)
				return;
			searchTextBox.Focus();
			searchTextBox.SelectAll();
		}
        
		/// <summary>
		/// Moves to the found line in the file.
		/// </summary>
		public void GoToNext()
		{
            if (SearchPattern == "" || SearchPattern == null)
            {
                SearchPattern = "1";
                messageView.Content = Localization.Null;
                return;
            }

            else if (SearchPattern.All(char.IsDigit))
            { 
                int Line = Int32.Parse(SearchPattern);

                //messageView.Content = null;

                if (Line > textArea.Document.LineCount)
                {
                    messageView.IsOpen = true;
                    messageView.Content = Localization.OverBounds;
                    messageView.PlacementTarget = searchTextBox;
                    Line = 1;
                    return;
                }

                else if (SearchPattern.All(char.IsDigit))
                {
                    messageView.Content = null;
                    textArea.Caret.Offset = textArea.Document.GetOffset(Line, 0);
                    textArea.Caret.BringCaretToView();
                    textArea.Caret.Show();
                }
                
            }
            else
            {
                return;
            }
        }
		
		ToolTip messageView = new ToolTip { Placement = PlacementMode.Bottom, StaysOpen = true, Focusable = false };

		void SelectResult(SearchResult result)
		{
			textArea.Caret.Offset = result.StartOffset;
			textArea.Selection = Selection.Create(textArea, result.StartOffset, result.EndOffset);
			textArea.Caret.BringCaretToView();
			// show caret even if the editor does not have the Keyboard Focus
			textArea.Caret.Show();
		}
		
		void SearchLayerKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key) {
				case Key.Enter:
					e.Handled = true;
						GoToNext();
					if (searchTextBox != null) {
						var error = Validation.GetErrors(searchTextBox).FirstOrDefault();
						if (error != null) {
							//messageView.Content = Localization.ErrorText + " " + error.ErrorContent;
							messageView.PlacementTarget = searchTextBox;
							messageView.IsOpen = true;
						}
					}
					break;
				case Key.Escape:
					e.Handled = true;
					Close();
					break;
			}
		}
		
		/// <summary>
		/// Gets whether the Panel is already closed.
		/// </summary>
		public bool IsClosed { get; private set; }
		
		/// <summary>
		/// Closes the GoToPanel.
		/// </summary>
		public void Close()
		{
			bool hasFocus = this.IsKeyboardFocusWithin;
			
			var layer = AdornerLayer.GetAdornerLayer(textArea);
			if (layer != null)
				layer.Remove(adorner);
			if (dropdownPopup != null)
				dropdownPopup.IsOpen = false;
			messageView.IsOpen = false;
			textArea.TextView.BackgroundRenderers.Remove(renderer);
			if (hasFocus)
				textArea.Focus();
			IsClosed = true;
			
			// Clear existing search results so that the segments don't have to be maintained
			renderer.CurrentResults.Clear();
		}
		
		/// <summary>
		/// Closes the GoToPanel and removes it.
		/// </summary>
		[Obsolete("Use the Uninstall method instead!")]
		public void CloseAndRemove()
		{
			Close();
			textArea.DocumentChanged -= textArea_DocumentChanged;
			if (currentDocument != null)
				currentDocument.TextChanged -= textArea_Document_TextChanged;
		}
		
		/// <summary>
		/// Opens the an existing GoTo panel.
		/// </summary>
		public void Open()
		{
			if (!IsClosed) return;
			var layer = AdornerLayer.GetAdornerLayer(textArea);
			if (layer != null)
				layer.Add(adorner);
			textArea.TextView.BackgroundRenderers.Add(renderer);
			IsClosed = false;
			//DoSearch(false);
		}
		
		/// <summary>
		/// Fired when GoToSearchOptions are changed inside the GoToPanel.
		/// </summary>
		public event EventHandler<GoToSearchOptionsChangedEventArgs> GoToSearchOptionsChanged;
		
		/// <summary>
		/// Raises the <see cref="GoToPanel.SearchOptionsChanged" /> event.
		/// </summary>
		protected virtual void GoToOnSearchOptionsChanged(GoToSearchOptionsChangedEventArgs e)
		{
			if (GoToSearchOptionsChanged != null) {
				GoToSearchOptionsChanged(this, e);
			}
		}
	}

    /// <summary>
    /// Creats a new instance of GoToOptionsChangedEventArgs
    /// </summary>
    public class GoToSearchOptionsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the search pattern.
        /// </summary>
        public string SearchPattern { get; private set; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted case-sensitive.
        /// </summary>
        public bool MatchCase { get; private set; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted as regular expression.
        /// </summary>
        public bool UseRegex { get; private set; }

        /// <summary>
        /// Gets whether the search pattern should only match whole words.
        /// </summary>
        public bool WholeWords { get; private set; }

        /// <summary>
        /// Creates a new GoToSearchOptionsChangedEventArgs instance.
        /// </summary>
        public GoToSearchOptionsChangedEventArgs(string searchPattern, bool matchCase, bool useRegex, bool wholeWords)
        {
            this.SearchPattern = searchPattern;
            this.MatchCase = matchCase;
            this.UseRegex = useRegex;
            this.WholeWords = wholeWords;
        }
    }

    class GoToPanelAdorner : Adorner
	{
		GoToPanel panel;
		
		public GoToPanelAdorner(TextArea textArea, GoToPanel panel)
			: base(textArea)
		{
			this.panel = panel;
			AddVisualChild(panel);
		}
		
		protected override int VisualChildrenCount {
			get { return 1; }
		}

		protected override Visual GetVisualChild(int index)
		{
			if (index != 0)
				throw new ArgumentOutOfRangeException();
			return panel;
		}
		
		protected override Size ArrangeOverride(Size finalSize)
		{
			panel.Arrange(new Rect(new Point(0, 0), finalSize));
			return new Size(panel.ActualWidth, panel.ActualHeight);
		}
	}
}
