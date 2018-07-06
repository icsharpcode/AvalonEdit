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

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Linq;
using System.Windows.Media;

namespace ICSharpCode.AvalonEdit.Search
{
	internal class SearchResultBackgroundRenderer : IBackgroundRenderer
	{
		private TextSegmentCollection<SearchResult> currentResults = new TextSegmentCollection<SearchResult>();

		public TextSegmentCollection<SearchResult> CurrentResults
		{
			get { return currentResults; }
		}

		public KnownLayer Layer
		{
			get
			{
				// draw behind selection
				return KnownLayer.Selection;
			}
		}

		public SearchResultBackgroundRenderer()
		{
			markerBrush = Brushes.LightGreen;
			markerPen = new Pen(markerBrush, 1);
		}

		private Brush markerBrush;
		private Pen markerPen;

		public Brush MarkerBrush
		{
			get { return markerBrush; }
			set
			{
				this.markerBrush = value;
				markerPen = new Pen(markerBrush, 1);
			}
		}

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			if (textView == null)
				throw new ArgumentNullException("textView");
			if (drawingContext == null)
				throw new ArgumentNullException("drawingContext");

			if (currentResults == null || !textView.VisualLinesValid)
				return;

			var visualLines = textView.VisualLines;
			if (visualLines.Count == 0)
				return;

			int viewStart = visualLines.First().FirstDocumentLine.Offset;
			int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;

			foreach (SearchResult result in currentResults.FindOverlappingSegments(viewStart, viewEnd - viewStart))
			{
				BackgroundGeometryBuilder geoBuilder = new BackgroundGeometryBuilder();
				geoBuilder.AlignToWholePixels = true;
				geoBuilder.BorderThickness = markerPen != null ? markerPen.Thickness : 0;
				geoBuilder.CornerRadius = 3;
				geoBuilder.AddSegment(textView, result);
				Geometry geometry = geoBuilder.CreateGeometry();
				if (geometry != null)
				{
					drawingContext.DrawGeometry(markerBrush, markerPen, geometry);
				}
			}
		}
	}
}