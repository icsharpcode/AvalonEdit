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
using System.Windows;
using System.Windows.Media;

namespace AcAvalonEdit.Utils
{
	/// <summary>
	/// Contains static helper methods for aligning stuff on a whole number of pixels.
	/// </summary>
	public static class PixelSnapHelpers
	{
		/// <summary>
		/// Gets the pixel size on the screen containing visual.
		/// This method does not take transforms on visual into account.
		/// </summary>
		public static Size GetPixelSize(Visual visual)
		{
			if (visual == null)
				throw new ArgumentNullException("visual");
			PresentationSource source = PresentationSource.FromVisual(visual);
			if (source != null) {
				Matrix matrix = source.CompositionTarget.TransformFromDevice;
				return new Size(matrix.M11, matrix.M22);
			} else {
				return new Size(1, 1);
			}
		}

		/// <summary>
		/// Aligns <paramref name="value"/> on the next middle of a pixel.
		/// </summary>
		/// <param name="value">The value that should be aligned</param>
		/// <param name="pixelSize">The size of one pixel</param>
		public static double PixelAlign(double value, double pixelSize)
		{
			// 0 -> 0.5
			// 0.1 -> 0.5
			// 0.5 -> 0.5
			// 0.9 -> 0.5
			// 1 -> 1.5
			return pixelSize * (Math.Round((value / pixelSize) + 0.5, MidpointRounding.AwayFromZero) - 0.5);
		}

		/// <summary>
		/// Aligns the borders of rect on the middles of pixels.
		/// </summary>
		public static Rect PixelAlign(Rect rect, Size pixelSize)
		{
			rect.X = PixelAlign(rect.X, pixelSize.Width);
			rect.Y = PixelAlign(rect.Y, pixelSize.Height);
			rect.Width = Round(rect.Width, pixelSize.Width);
			rect.Height = Round(rect.Height, pixelSize.Height);
			return rect;
		}

		/// <summary>
		/// Rounds <paramref name="point"/> to whole number of pixels.
		/// </summary>
		public static Point Round(Point point, Size pixelSize)
		{
			return new Point(Round(point.X, pixelSize.Width), Round(point.Y, pixelSize.Height));
		}

		/// <summary>
		/// Rounds val to whole number of pixels.
		/// </summary>
		public static Rect Round(Rect rect, Size pixelSize)
		{
			return new Rect(Round(rect.X, pixelSize.Width), Round(rect.Y, pixelSize.Height),
							Round(rect.Width, pixelSize.Width), Round(rect.Height, pixelSize.Height));
		}

		/// <summary>
		/// Rounds <paramref name="value"/> to a whole number of pixels.
		/// </summary>
		public static double Round(double value, double pixelSize)
		{
			return pixelSize * Math.Round(value / pixelSize, MidpointRounding.AwayFromZero);
		}

		/// <summary>
		/// Rounds <paramref name="value"/> to an whole odd number of pixels.
		/// </summary>
		public static double RoundToOdd(double value, double pixelSize)
		{
			return Round(value - pixelSize, pixelSize * 2) + pixelSize;
		}
	}
}
