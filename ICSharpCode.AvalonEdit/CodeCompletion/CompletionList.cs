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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using AcAvalonEdit.Utils;

namespace AcAvalonEdit.CodeCompletion
{
	/// <summary>
	/// The listbox used inside the CompletionWindow, contains CompletionListBox.
	/// </summary>
	public class CompletionList : Control
	{
		static CompletionList()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CompletionList),
													 new FrameworkPropertyMetadata(typeof(CompletionList)));
		}

		bool isFiltering = false;
		/// <summary>
		/// If true, the CompletionList is filtered to show only matching items. Also enables search by substring.
		/// If false, enables the old behavior: no filtering, search by string.StartsWith.
		/// </summary>
		public bool IsFiltering {
			get { return isFiltering; }
			set { isFiltering = value; }
		}


		/// <summary>
		/// Dependency property for <see cref="EmptyTemplate" />.
		/// </summary>
		public static readonly DependencyProperty EmptyTemplateProperty =
			DependencyProperty.Register("EmptyTemplate", typeof(ControlTemplate), typeof(CompletionList),
										new FrameworkPropertyMetadata());

		/// <summary>
		/// Content of EmptyTemplate will be shown when CompletionList contains no items.
		/// If EmptyTemplate is null, nothing will be shown.
		/// </summary>
		public ControlTemplate EmptyTemplate {
			get { return (ControlTemplate)GetValue(EmptyTemplateProperty); }
			set { SetValue(EmptyTemplateProperty, value); }
		}

		/// <summary>
		/// Is raised when the completion list indicates that the user has chosen
		/// an entry to be completed.
		/// </summary>
		public event EventHandler InsertionRequested;

		/// <summary>
		/// Raises the InsertionRequested event.
		/// </summary>
		public void RequestInsertion(EventArgs e)
		{
			if (InsertionRequested != null)
				InsertionRequested(this, e);
		}

		CompletionListBox listBox;

		/// <inheritdoc/>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			listBox = GetTemplateChild("PART_ListBox") as CompletionListBox;
			if (listBox != null) {
				listBox.ItemsSource = completionData;
			}
		}

		/// <summary>
		/// Gets the list box.
		/// </summary>
		public CompletionListBox ListBox {
			get {
				if (listBox == null)
					ApplyTemplate();
				return listBox;
			}
		}

		/// <summary>
		/// Gets the scroll viewer used in this list box.
		/// </summary>
		public ScrollViewer ScrollViewer {
			get { return listBox != null ? listBox.scrollViewer : null; }
		}

		ObservableCollection<ICompletionData> completionData = new ObservableCollection<ICompletionData>();

		/// <summary>
		/// Gets the list to which completion data can be added.
		/// </summary>
		public IList<ICompletionData> CompletionData {
			get { return completionData; }
		}

		/// <inheritdoc/>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled) {
				HandleKey(e);
			}
		}
		/// <summary>
		/// Handles a key press. Used to let the completion list handle key presses while the
		/// focus is still on the text editor.
		/// </summary>
		public void HandleKey(KeyEventArgs e)
		{
			if (listBox == null)
				return;

			// We have to do some key handling manually, because the default doesn't work with
			// our simulated events.
			// Also, the default PageUp/PageDown implementation changes the focus, so we avoid it.
			switch (e.Key) {
				case Key.Down:
					e.Handled = true;
					listBox.SelectIndex(listBox.SelectedIndex + 1);
					break;
				case Key.Up:
					e.Handled = true;
					listBox.SelectIndex(listBox.SelectedIndex - 1);
					break;
				case Key.PageDown:
					e.Handled = true;
					listBox.SelectIndex(listBox.SelectedIndex + listBox.VisibleItemCount);
					break;
				case Key.PageUp:
					e.Handled = true;
					listBox.SelectIndex(listBox.SelectedIndex - listBox.VisibleItemCount);
					break;
				case Key.Home:
					e.Handled = true;
					listBox.SelectIndex(0);
					break;
				case Key.End:
					e.Handled = true;
					listBox.SelectIndex(listBox.Items.Count - 1);
					break;
				case Key.Tab:
					break;
				case Key.Enter:
					e.Handled = true;
					RequestInsertion(e);
					break;
			}
		}

		/// <inheritdoc/>
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if (e.ChangedButton == MouseButton.Left) {
				// only process double clicks on the ListBoxItems, not on the scroll bar
				if (ExtensionMethods.VisualAncestorsAndSelf(e.OriginalSource as DependencyObject).TakeWhile(obj => obj != this).Any(obj => obj is ListBoxItem)) {
					e.Handled = true;
					RequestInsertion(e);
				}
			}
		}

		/// <summary>
		/// Gets/Sets the selected item.
		/// </summary>
		/// <remarks>
		/// The setter of this property does not scroll to the selected item.
		/// You might want to also call <see cref="ScrollIntoView"/>.
		/// </remarks>
		public ICompletionData SelectedItem {
			get {
				return (listBox != null ? listBox.SelectedItem : null) as ICompletionData;
			}
			set {
				if (listBox == null && value != null)
					ApplyTemplate();
				if (listBox != null) // may still be null if ApplyTemplate fails, or if listBox and value both are null
					listBox.SelectedItem = value;
			}
		}

		/// <summary>
		/// Scrolls the specified item into view.
		/// </summary>
		public void ScrollIntoView(ICompletionData item)
		{
			if (listBox == null)
				ApplyTemplate();
			if (listBox != null)
				listBox.ScrollIntoView(item);
		}

		/// <summary>
		/// Occurs when the SelectedItem property changes.
		/// </summary>
		public event SelectionChangedEventHandler SelectionChanged {
			add { AddHandler(Selector.SelectionChangedEvent, value); }
			remove { RemoveHandler(Selector.SelectionChangedEvent, value); }
		}

		// SelectItem gets called twice for every typed character (once from FormatLine), this helps execute SelectItem only once
		string currentText;
		ObservableCollection<ICompletionData> currentList;


		/// <summary>
		/// Number of currently visible Completions
		/// </summary>
		public int VisibleCompletionsCount {
			get {
				if(currentList == null)
					return 0;
				return currentList.Count;
			}
		}

		/// <summary>
		/// Selects the best match, and filter the items if turned on using <see cref="IsFiltering" />.
		/// </summary>
		public void SelectItem(string text,bool force=false)
		{
			if (text == currentText && !force)
				return;
			if (listBox == null)
				ApplyTemplate();

			if (true) {
				SelectItemFiltering(text,force);
			}/* else {
				SelectItemWithStart(text);
			}*/
			currentText = text;
		}

		/// <summary>
		/// Filters CompletionList items to show only those matching given query, and selects the best match.
		/// </summary>
		void SelectItemFiltering(string query,bool force)
		{
			if(query == ".") {
				this.currentList?.Clear();
				return;
			}
			// if the user just typed one more character, don't filter all data but just filter what we are already displaying
			var listToFilter = (this.currentList != null && (!string.IsNullOrEmpty(this.currentText)) && (!string.IsNullOrEmpty(query)) &&
								query.StartsWith(this.currentText, StringComparison.Ordinal) && !force) ?
				this.currentList : this.completionData;

			var matchingItems =
				from item in listToFilter
				let quality = GetMatchQuality(item.Text, query)
				where quality > 0
				select new { Item = item, Quality = quality };

			// e.g. "DateTimeKind k = (*cc here suggests DateTimeKind*)"
			ICompletionData suggestedItem = listBox.SelectedIndex != -1 ? (ICompletionData)(listBox.Items[listBox.SelectedIndex]) : null;

			var listBoxItems = new ObservableCollection<ICompletionData>();
			int bestIndex = -1;
			int bestQuality = -1;
			double bestPriority = 0;
			int i = 0;
			foreach (var matchingItem in matchingItems) {
				double priority = matchingItem.Item == suggestedItem ? double.PositiveInfinity : matchingItem.Item.Priority;
				int quality = matchingItem.Quality;
				if (quality > bestQuality || (quality == bestQuality && (priority > bestPriority))) {
					bestIndex = i;
					bestPriority = priority;
					bestQuality = quality;
				}
				listBoxItems.Add(matchingItem.Item);
				i++;
			}
			this.currentList = listBoxItems;
			listBox.ItemsSource = listBoxItems;
			SelectIndexCentered(bestIndex);
		}

		/// <summary>
		/// Selects the item that starts with the specified query.
		/// </summary>
		void SelectItemWithStart(string query)
		{
			if (string.IsNullOrEmpty(query))
				return;

			int suggestedIndex = listBox.SelectedIndex;

			int bestIndex = -1;
			int bestQuality = -1;
			double bestPriority = 0;
			for (int i = 0; i < completionData.Count; ++i) {
				int quality = GetMatchQuality(completionData[i].Text, query);
				if (quality < 0)
					continue;

				double priority = completionData[i].Priority;
				bool useThisItem;
				if (bestQuality < quality) {
					useThisItem = true;
				} else {
					if (bestIndex == suggestedIndex) {
						useThisItem = false;
					} else if (i == suggestedIndex) {
						// prefer recommendedItem, regardless of its priority
						useThisItem = bestQuality == quality;
					} else {
						useThisItem = bestQuality == quality && bestPriority < priority;
					}
				}
				if (useThisItem) {
					bestIndex = i;
					bestPriority = priority;
					bestQuality = quality;
				}
			}
			SelectIndexCentered(bestIndex);
		}

		void SelectIndexCentered(int bestIndex)
		{
			if (bestIndex < 0) {
				listBox.ClearSelection();
			} else {
				int firstItem = listBox.FirstVisibleItem;
				if (bestIndex < firstItem || firstItem + listBox.VisibleItemCount <= bestIndex) {
					// CenterViewOn does nothing as CompletionListBox.ScrollViewer is null
					listBox.CenterViewOn(bestIndex);
					listBox.SelectIndex(bestIndex);
				} else {
					listBox.SelectIndex(bestIndex);
				}
			}
		}

		int GetMatchQuality(string itemText, string query)
		{
			if (itemText == null)
				throw new ArgumentNullException("itemText", "ICompletionData.Text returned null");

			// Qualities:
			//  	8 = full match case sensitive
			// 		7 = full match
			// 		6 = match start case sensitive
			//		5 = match start
			//		4 = match CamelCase when length of query is 1 or 2 characters
			// 		3 = match substring case sensitive
			//		2 = match substring
			//		1 = match CamelCase
			//		-1 = no match
			if (query == itemText)
				return 8;
			if (string.Equals(itemText, query, StringComparison.InvariantCultureIgnoreCase))
				return 7;

			if (itemText.StartsWith(query, StringComparison.InvariantCulture))
				return 6;
			if (itemText.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
				return 5;

			bool? camelCaseMatch = null;
			if (query.Length <= 2) {
				camelCaseMatch = CamelCaseMatch(itemText, query);
				if (camelCaseMatch == true) return 4;
			}

			// search by substring, if filtering (i.e. new behavior) turned on
			if (IsFiltering) {
				if (itemText.IndexOf(query, StringComparison.InvariantCulture) >= 0)
					return 3;
				if (itemText.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
					return 2;
			}

			if (!camelCaseMatch.HasValue)
				camelCaseMatch = CamelCaseMatch(itemText, query);
			if (camelCaseMatch == true)
				return 1;

			return -1;
		}


		static bool CamelCaseMatch(string text, string query)
		{
			// We take the first letter of the text regardless of whether or not it's upper case so we match
			// against camelCase text as well as PascalCase text ("cct" matches "camelCaseText")
			var theFirstLetterOfEachWord = text.Take(1).Concat(text.Skip(1).Where(char.IsUpper));

			int i = 0;
			foreach (var letter in theFirstLetterOfEachWord) {
				if (i > query.Length - 1)
					return true;    // return true here for CamelCase partial match ("CQ" matches "CodeQualityAnalysis")
				if (char.ToUpperInvariant(query[i]) != char.ToUpperInvariant(letter))
					return false;
				i++;
			}
			if (i >= query.Length)
				return true;
			return false;
		}
	}
}
