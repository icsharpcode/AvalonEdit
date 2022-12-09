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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Folding
{
	/// <summary>
	/// A margin that shows markers for foldings and allows to expand/collapse the foldings.
	/// </summary>
	public class FoldingMargin : AbstractMargin
	{
		/// <summary>
		/// Gets/Sets the folding manager from which the foldings should be shown.
		/// </summary>
		public FoldingManager FoldingManager { get; set; }

		internal const double SizeFactor = Constants.PixelPerPoint;

		#region Brushes
		/// <summary>
		/// FoldingMarkerBrush dependency property.
		/// </summary>
		public static readonly DependencyProperty FoldingMarkerBrushProperty =
			DependencyProperty.RegisterAttached(nameof(FoldingMarkerBrush), typeof(Brush), typeof(FoldingMargin),
												new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.Inherits, OnUpdateBrushes));

		/// <summary>
		/// Gets the value of the <see cref="FoldingMarkerBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static Brush GetFoldingMarkerBrush(DependencyObject obj)
		{
			return (Brush)obj.GetValue(FoldingMarkerBrushProperty);
		}

		/// <summary>
		/// Sets the value of the <see cref="FoldingMarkerBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static void SetFoldingMarkerBrush(DependencyObject obj, Brush value)
		{
			obj.SetValue(FoldingMarkerBrushProperty, value);
		}

		/// <summary>
		/// Gets/sets the Brush used for displaying the lines of folding markers.
		/// </summary>
		public Brush FoldingMarkerBrush {
			get { return (Brush)GetValue(FoldingMarkerBrushProperty); }
			set { SetValue(FoldingMarkerBrushProperty, value); }
		}

		/// <summary>
		/// FoldingMarkerBackgroundBrush dependency property.
		/// </summary>
		public static readonly DependencyProperty FoldingMarkerBackgroundBrushProperty =
			DependencyProperty.RegisterAttached(nameof(FoldingMarkerBackgroundBrush), typeof(Brush), typeof(FoldingMargin),
												new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.Inherits, OnUpdateBrushes));

		/// <summary>
		/// Gets the value of the <see cref="FoldingMarkerBackgroundBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static Brush GetFoldingMarkerBackgroundBrush(DependencyObject obj)
		{
			return (Brush)obj.GetValue(FoldingMarkerBackgroundBrushProperty);
		}

		/// <summary>
		/// Sets the value of the <see cref="FoldingMarkerBackgroundBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static void SetFoldingMarkerBackgroundBrush(DependencyObject obj, Brush value)
		{
			obj.SetValue(FoldingMarkerBackgroundBrushProperty, value);
		}

		/// <summary>
		/// Gets/sets the Brush used for displaying the background of folding markers.
		/// </summary>
		public Brush FoldingMarkerBackgroundBrush {
			get { return (Brush)GetValue(FoldingMarkerBackgroundBrushProperty); }
			set { SetValue(FoldingMarkerBackgroundBrushProperty, value); }
		}

		/// <summary>
		/// SelectedFoldingMarkerBrush dependency property.
		/// </summary>
		public static readonly DependencyProperty SelectedFoldingMarkerBrushProperty =
			DependencyProperty.RegisterAttached(nameof(SelectedFoldingMarkerBrush), typeof(Brush), typeof(FoldingMargin),
												new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.Inherits, OnUpdateBrushes));

		/// <summary>
		/// Gets the value of the <see cref="SelectedFoldingMarkerBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static Brush GetSelectedFoldingMarkerBrush(DependencyObject obj)
		{
			return (Brush)obj.GetValue(SelectedFoldingMarkerBrushProperty);
		}

		/// <summary>
		/// Sets the value of the <see cref="SelectedFoldingMarkerBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static void SetSelectedFoldingMarkerBrush(DependencyObject obj, Brush value)
		{
			obj.SetValue(SelectedFoldingMarkerBrushProperty, value);
		}

		/// <summary>
		/// Gets/sets the Brush used for displaying the lines of selected folding markers.
		/// </summary>
		public Brush SelectedFoldingMarkerBrush {
			get { return (Brush)GetValue(SelectedFoldingMarkerBrushProperty); }
			set { SetValue(SelectedFoldingMarkerBrushProperty, value); }
		}

		/// <summary>
		/// SelectedFoldingMarkerBackgroundBrush dependency property.
		/// </summary>
		public static readonly DependencyProperty SelectedFoldingMarkerBackgroundBrushProperty =
			DependencyProperty.RegisterAttached(nameof(SelectedFoldingMarkerBackgroundBrush), typeof(Brush), typeof(FoldingMargin),
												new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.Inherits, OnUpdateBrushes));

		/// <summary>
		/// Gets the value of the <see cref="SelectedFoldingMarkerBackgroundBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static Brush GetSelectedFoldingMarkerBackgroundBrush(DependencyObject obj)
		{
			return (Brush)obj.GetValue(SelectedFoldingMarkerBackgroundBrushProperty);
		}

		/// <summary>
		/// Sets the value of the <see cref="SelectedFoldingMarkerBackgroundBrush"/> attached property for a specified <see cref="DependencyObject"/>.
		/// </summary>
		public static void SetSelectedFoldingMarkerBackgroundBrush(DependencyObject obj, Brush value)
		{
			obj.SetValue(SelectedFoldingMarkerBackgroundBrushProperty, value);
		}

		/// <summary>
		/// Gets/sets the Brush used for displaying the background of selected folding markers.
		/// </summary>
		public Brush SelectedFoldingMarkerBackgroundBrush {
			get { return (Brush)GetValue(SelectedFoldingMarkerBackgroundBrushProperty); }
			set { SetValue(SelectedFoldingMarkerBackgroundBrushProperty, value); }
		}

		static void OnUpdateBrushes(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FoldingMargin m = null;
			if (d is FoldingMargin)
				m = (FoldingMargin)d;
			else if (d is TextEditor)
				m = ((TextEditor)d).TextArea.LeftMargins.FirstOrDefault(c => c is FoldingMargin) as FoldingMargin;
			if (m == null) return;
			if (e.Property.Name == FoldingMarkerBrushProperty.Name)
				m.foldingControlPen = MakeFrozenPen((Brush)e.NewValue);
			if (e.Property.Name == SelectedFoldingMarkerBrushProperty.Name)
				m.selectedFoldingControlPen = MakeFrozenPen((Brush)e.NewValue);
		}
		#endregion

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size availableSize)
		{
			foreach (FoldingMarginMarker m in markers) {
				m.Measure(availableSize);
			}
			double width = SizeFactor * (double)GetValue(TextBlock.FontSizeProperty);
			return new Size(PixelSnapHelpers.RoundToOdd(width, PixelSnapHelpers.GetPixelSize(this).Width), 0);
		}

		/// <inheritdoc/>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
			foreach (FoldingMarginMarker m in markers) {
				int visualColumn = m.VisualLine.GetVisualColumn(m.FoldingSection.StartOffset - m.VisualLine.FirstDocumentLine.Offset);
				TextLine textLine = m.VisualLine.GetTextLine(visualColumn);
				double yPos = m.VisualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.TextMiddle) - TextView.VerticalOffset;
				yPos -= m.DesiredSize.Height / 2;
				double xPos = (finalSize.Width - m.DesiredSize.Width) / 2;
				m.Arrange(new Rect(PixelSnapHelpers.Round(new Point(xPos, yPos), pixelSize), m.DesiredSize));
			}
			return base.ArrangeOverride(finalSize);
		}

		/// <inheritdoc/>
		protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
		{
			if (oldTextView != null) {
				oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
			}
			base.OnTextViewChanged(oldTextView, newTextView);
			if (newTextView != null) {
				newTextView.VisualLinesChanged += TextViewVisualLinesChanged;
			}
			TextViewVisualLinesChanged(null, null);
		}

		List<FoldingMarginMarker> markers = new List<FoldingMarginMarker>();

		void TextViewVisualLinesChanged(object sender, EventArgs e)
		{
			foreach (FoldingMarginMarker m in markers) {
				RemoveVisualChild(m);
			}
			markers.Clear();
			InvalidateVisual();
			if (TextView != null && FoldingManager != null && TextView.VisualLinesValid) {
				foreach (VisualLine line in TextView.VisualLines) {
					FoldingSection fs = FoldingManager.GetNextFolding(line.FirstDocumentLine.Offset);
					if (fs == null)
						continue;
					if (fs.StartOffset <= line.LastDocumentLine.Offset + line.LastDocumentLine.Length) {
						FoldingMarginMarker m = new FoldingMarginMarker {
							IsExpanded = !fs.IsFolded,
							VisualLine = line,
							FoldingSection = fs
						};

						markers.Add(m);
						AddVisualChild(m);

						m.IsMouseDirectlyOverChanged += delegate { InvalidateVisual(); };

						InvalidateMeasure();
						continue;
					}
				}
			}
		}

		/// <inheritdoc/>
		protected override int VisualChildrenCount {
			get { return markers.Count; }
		}

		/// <inheritdoc/>
		protected override Visual GetVisualChild(int index)
		{
			return markers[index];
		}

		Pen foldingControlPen = MakeFrozenPen((Brush)FoldingMarkerBrushProperty.DefaultMetadata.DefaultValue);
		Pen selectedFoldingControlPen = MakeFrozenPen((Brush)SelectedFoldingMarkerBrushProperty.DefaultMetadata.DefaultValue);

		static Pen MakeFrozenPen(Brush brush)
		{
			Pen pen = new Pen(brush, 1);
			pen.Freeze();
			return pen;
		}

		/// <inheritdoc/>
		protected override void OnRender(DrawingContext drawingContext)
		{
			if (TextView == null || !TextView.VisualLinesValid)
				return;
			if (TextView.VisualLines.Count == 0 || FoldingManager == null)
				return;

			var allTextLines = TextView.VisualLines.SelectMany(vl => vl.TextLines).ToList();
			Pen[] colors = new Pen[allTextLines.Count + 1];
			Pen[] endMarker = new Pen[allTextLines.Count];

			CalculateFoldLinesForFoldingsActiveAtStart(allTextLines, colors, endMarker);
			CalculateFoldLinesForMarkers(allTextLines, colors, endMarker);
			DrawFoldLines(drawingContext, colors, endMarker);

			base.OnRender(drawingContext);
		}

		/// <summary>
		/// Calculates fold lines for all folding sections that start in front of the current view
		/// and run into the current view.
		/// </summary>
		void CalculateFoldLinesForFoldingsActiveAtStart(List<TextLine> allTextLines, Pen[] colors, Pen[] endMarker)
		{
			int viewStartOffset = TextView.VisualLines[0].FirstDocumentLine.Offset;
			int viewEndOffset = TextView.VisualLines.Last().LastDocumentLine.EndOffset;
			var foldings = FoldingManager.GetFoldingsContaining(viewStartOffset);
			int maxEndOffset = 0;
			foreach (FoldingSection fs in foldings) {
				int end = fs.EndOffset;
				if (end <= viewEndOffset && !fs.IsFolded) {
					int textLineNr = GetTextLineIndexFromOffset(allTextLines, end);
					if (textLineNr >= 0) {
						endMarker[textLineNr] = foldingControlPen;
					}
				}
				if (end > maxEndOffset && fs.StartOffset < viewStartOffset) {
					maxEndOffset = end;
				}
			}
			if (maxEndOffset > 0) {
				if (maxEndOffset > viewEndOffset) {
					for (int i = 0; i < colors.Length; i++) {
						colors[i] = foldingControlPen;
					}
				} else {
					int maxTextLine = GetTextLineIndexFromOffset(allTextLines, maxEndOffset);
					for (int i = 0; i <= maxTextLine; i++) {
						colors[i] = foldingControlPen;
					}
				}
			}
		}

		/// <summary>
		/// Calculates fold lines for all folding sections that start inside the current view
		/// </summary>
		void CalculateFoldLinesForMarkers(List<TextLine> allTextLines, Pen[] colors, Pen[] endMarker)
		{
			foreach (FoldingMarginMarker marker in markers) {
				int end = marker.FoldingSection.EndOffset;
				int endTextLineNr = GetTextLineIndexFromOffset(allTextLines, end);
				if (!marker.FoldingSection.IsFolded && endTextLineNr >= 0) {
					if (marker.IsMouseDirectlyOver)
						endMarker[endTextLineNr] = selectedFoldingControlPen;
					else if (endMarker[endTextLineNr] == null)
						endMarker[endTextLineNr] = foldingControlPen;
				}
				int startTextLineNr = GetTextLineIndexFromOffset(allTextLines, marker.FoldingSection.StartOffset);
				if (startTextLineNr >= 0) {
					for (int i = startTextLineNr + 1; i < colors.Length && i - 1 != endTextLineNr; i++) {
						if (marker.IsMouseDirectlyOver)
							colors[i] = selectedFoldingControlPen;
						else if (colors[i] == null)
							colors[i] = foldingControlPen;
					}
				}
			}
		}

		/// <summary>
		/// Draws the lines for the folding sections (vertical line with 'color', horizontal lines with 'endMarker')
		/// Each entry in the input arrays corresponds to one TextLine.
		/// </summary>
		void DrawFoldLines(DrawingContext drawingContext, Pen[] colors, Pen[] endMarker)
		{
			// Because we are using PenLineCap.Flat (the default), for vertical lines,
			// Y coordinates must be on pixel boundaries, whereas the X coordinate must be in the
			// middle of a pixel. (and the other way round for horizontal lines)
			Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
			double markerXPos = PixelSnapHelpers.PixelAlign(RenderSize.Width / 2, pixelSize.Width);
			double startY = 0;
			Pen currentPen = colors[0];
			int tlNumber = 0;
			foreach (VisualLine vl in TextView.VisualLines) {
				foreach (TextLine tl in vl.TextLines) {
					if (endMarker[tlNumber] != null) {
						double visualPos = GetVisualPos(vl, tl, pixelSize.Height);
						drawingContext.DrawLine(endMarker[tlNumber], new Point(markerXPos - pixelSize.Width / 2, visualPos), new Point(RenderSize.Width, visualPos));
					}
					if (colors[tlNumber + 1] != currentPen) {
						double visualPos = GetVisualPos(vl, tl, pixelSize.Height);
						if (currentPen != null) {
							drawingContext.DrawLine(currentPen, new Point(markerXPos, startY + pixelSize.Height / 2), new Point(markerXPos, visualPos - pixelSize.Height / 2));
						}
						currentPen = colors[tlNumber + 1];
						startY = visualPos;
					}
					tlNumber++;
				}
			}
			if (currentPen != null) {
				drawingContext.DrawLine(currentPen, new Point(markerXPos, startY + pixelSize.Height / 2), new Point(markerXPos, RenderSize.Height));
			}
		}

		double GetVisualPos(VisualLine vl, TextLine tl, double pixelHeight)
		{
			double pos = vl.GetTextLineVisualYPosition(tl, VisualYPosition.TextMiddle) - TextView.VerticalOffset;
			return PixelSnapHelpers.PixelAlign(pos, pixelHeight);
		}

		int GetTextLineIndexFromOffset(List<TextLine> textLines, int offset)
		{
			int lineNumber = TextView.Document.GetLineByOffset(offset).LineNumber;
			VisualLine vl = TextView.GetVisualLine(lineNumber);
			if (vl != null) {
				int relOffset = offset - vl.FirstDocumentLine.Offset;
				TextLine line = vl.GetTextLine(vl.GetVisualColumn(relOffset));
				return textLines.IndexOf(line);
			}
			return -1;
		}
	}
}
