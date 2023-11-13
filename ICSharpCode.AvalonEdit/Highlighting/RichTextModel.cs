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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using AcAvalonEdit.Document;

namespace AcAvalonEdit.Highlighting
{
   /// <summary>
   /// Stores rich-text formatting.
   /// </summary>
   public sealed class RichTextModel
   {
      List<int> stateChangeOffsets = new List<int>();
      List<HighlightingColor> stateChanges = new List<HighlightingColor>();

      int GetIndexForOffset(int offset)
      {
         if (offset < 0)
            throw new ArgumentOutOfRangeException("offset");
         int index = stateChangeOffsets.BinarySearch(offset);
         if (index < 0)
         {
            // If no color change exists directly at offset,
            // create a new one.
            index = ~index;
            stateChanges.Insert(index, stateChanges[index - 1].Clone());
            stateChangeOffsets.Insert(index, offset);
         }
         return index;
      }

      int GetIndexForOffsetUseExistingSegment(int offset)
      {
         if (offset < 0)
            throw new ArgumentOutOfRangeException("offset");
         int index = stateChangeOffsets.BinarySearch(offset);
         if (index < 0)
         {
            // If no color change exists directly at offset,
            // return the index of the color segment that contains offset.
            index = ~index - 1;
         }
         return index;
      }

      int GetEnd(int index)
      {
         // Gets the end of the color segment no. index.
         if (index + 1 < stateChangeOffsets.Count)
            return stateChangeOffsets[index + 1];
         else
            return int.MaxValue;
      }

      /// <summary>
      /// Creates a new RichTextModel.
      /// </summary>
      public RichTextModel()
      {
         stateChangeOffsets.Add(0);
         stateChanges.Add(new HighlightingColor());
      }

      /// <summary>
      /// Creates a RichTextModel from a CONTIGUOUS list of HighlightedSections.
      /// </summary>
      internal RichTextModel(int[] stateChangeOffsets, HighlightingColor[] stateChanges)
      {
         Debug.Assert(stateChangeOffsets[0] == 0);
         this.stateChangeOffsets.AddRange(stateChangeOffsets);
         this.stateChanges.AddRange(stateChanges);
      }

      #region UpdateOffsets
      /// <summary>
      /// Updates the start and end offsets of all segments stored in this collection.
      /// </summary>
      /// <param name="e">TextChangeEventArgs instance describing the change to the document.</param>
      public void UpdateOffsets(TextChangeEventArgs e)
      {
         if (e == null)
            throw new ArgumentNullException("e");
         UpdateOffsets(e.GetNewOffset);
      }

      /// <summary>
      /// Updates the start and end offsets of all segments stored in this collection.
      /// </summary>
      /// <param name="change">OffsetChangeMap instance describing the change to the document.</param>
      public void UpdateOffsets(OffsetChangeMap change)
      {
         if (change == null)
            throw new ArgumentNullException("change");
         UpdateOffsets(change.GetNewOffset);
      }

      /// <summary>
      /// Updates the start and end offsets of all segments stored in this collection.
      /// </summary>
      /// <param name="change">OffsetChangeMapEntry instance describing the change to the document.</param>
      public void UpdateOffsets(OffsetChangeMapEntry change)
      {
         UpdateOffsets(change.GetNewOffset);
      }

      void UpdateOffsets(Func<int, AnchorMovementType, int> updateOffset)
      {
         int readPos = 1;
         int writePos = 1;
         while (readPos < stateChangeOffsets.Count)
         {
            Debug.Assert(writePos <= readPos);
            int newOffset = updateOffset(stateChangeOffsets[readPos], AnchorMovementType.Default);
            if (newOffset == stateChangeOffsets[writePos - 1])
            {
               //Handling of EOL Marker
               if (stateChanges[writePos - 1].EOLMarker.HasValue)
               {
                  if (stateChanges[readPos].EOLMarker.HasValue)
                     stateChanges[readPos].EOLMarker += stateChanges[writePos - 1].EOLMarker.Value;
                  else
                     stateChanges[readPos].EOLMarker = stateChanges[writePos - 1].EOLMarker.Value;
               }
               // offset moved to same position as previous offset
               // -> previous segment has length 0 and gets overwritten with this segment
               stateChanges[writePos - 1] = stateChanges[readPos];
            }
            else
            {
               stateChangeOffsets[writePos] = newOffset;
               stateChanges[writePos] = stateChanges[readPos];
               writePos++;
            }
            readPos++;
         }
         // Delete all entries that were not written to
         stateChangeOffsets.RemoveRange(writePos, stateChangeOffsets.Count - writePos);
         stateChanges.RemoveRange(writePos, stateChanges.Count - writePos);
      }
      #endregion

      /// <summary>
      /// Appends another RichTextModel after this one.
      /// </summary>
      internal void Append(int offset, int[] newOffsets, HighlightingColor[] newColors)
      {
         Debug.Assert(newOffsets.Length == newColors.Length);
         Debug.Assert(newOffsets[0] == 0);
         // remove everything not before offset:
         while (stateChangeOffsets.Count > 0 && stateChangeOffsets.Last() >= offset)
         {
            stateChangeOffsets.RemoveAt(stateChangeOffsets.Count - 1);
            stateChanges.RemoveAt(stateChanges.Count - 1);
         }
         // Append the new segments
         for (int i = 0; i < newOffsets.Length; i++)
         {
            stateChangeOffsets.Add(offset + newOffsets[i]);
            stateChanges.Add(newColors[i]);
         }
      }
      /// <summary>
      /// Clears all offsets and HLC
      /// </summary>
      public void Clear()
      {
         stateChangeOffsets.Clear();
         stateChanges.Clear();
         stateChangeOffsets.Add(0);
         stateChanges.Add(new HighlightingColor());
      }

      /// <summary>
      /// Gets a copy of the HighlightingColor for the specified offset.
      /// </summary>
      public HighlightingColor GetHighlightingAt(int offset)
      {
         return stateChanges[GetIndexForOffsetUseExistingSegment(offset)].Clone();
      }

      /// <summary>
      /// Gets a copy of the HighlightingColors for the specified area.
      /// </summary>
      public List<(HighlightingColor, int)> GetHighlightingsAt(int offset, int endOffset)
      {
         List<(HighlightingColor, int)> result = new();
         for (int i = 0; i < stateChangeOffsets.Count; i++)
         {
            if (stateChangeOffsets[i] >= offset && stateChangeOffsets[i] <= endOffset)
            {
               result.Add((stateChanges[i].Clone(), stateChangeOffsets[i]));
            }
         }
         return result;
      }

      /// <summary>
      /// Applies the HighlightingColor to the specified range of text.
      /// If the color specifies <c>null</c> for some properties, existing highlighting is preserved.
      /// </summary>
      public void ApplyHighlighting(int offset, int length, HighlightingColor color)
      {
         if (color == null || color.IsEmptyForMerge)
         {
            // Optimization: don't split the HighlightingState when we're not changing
            // any property. For example, the "Punctuation" color in C# is
            // empty by default.
            return;
         }
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].MergeWith(color);
         }
      }

      /// <summary>
      /// Sets the HighlightingColor for the specified range of text,
      /// completely replacing the existing highlighting in that area.
      /// </summary>
      public void SetHighlighting(int offset, int length, HighlightingColor color)
      {
         if (length <= 0)
            return;
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         stateChanges[startIndex] = color != null ? color.Clone() : new HighlightingColor();
         stateChanges.RemoveRange(startIndex + 1, endIndex - (startIndex + 1));
         stateChangeOffsets.RemoveRange(startIndex + 1, endIndex - (startIndex + 1));
      }

      /// <summary>
      /// Sets the foreground brush on the specified text segment.
      /// </summary>
      public void SetForeground(int offset, int length, HighlightingBrush brush)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].Foreground = brush;
         }
      }

      /// <summary>
      /// Sets the background brush on the specified text segment.
      /// </summary>
      public void SetBackground(int offset, int length, HighlightingBrush brush)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].Background = brush;
         }
      }
      /// <summary>
      /// Sets the fontsize on the specified text segment 
      /// </summary>
      public void SetFontSize(int offset, int length, int fontSize)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].FontSize = fontSize;
         }
      }

      /// <summary>
      /// Sets the FontFamily for the specified text segment
      /// </summary>
      public void SetFontFamily(int offset, int length, FontFamily family)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].FontFamily = family;
         }
      }

      /// <summary>
      /// Sets the font weight on the specified text segment.
      /// </summary>
      public void SetFontWeight(int offset, int length, FontWeight weight)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].FontWeight = weight;
         }
      }

      /// <summary>
      /// Sets the font style on the specified text segment.
      /// </summary>
      public void SetFontStyle(int offset, int length, FontStyle style)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].FontStyle = style;
         }
      }

      /// <summary>
      /// Sets the Underline property for the specified text segment
      /// </summary>
      public void SetUnderline(int offset, int length, bool underline, SimpleHighlightingBrush? brush = null)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].Underline = underline;
            stateChanges[i].UnderlineBrush = brush;
         }
      }

      /// <summary>
      /// Sets Overline
      /// </summary>
      public void SetOverline(int offset, int length, bool overline, SimpleHighlightingBrush brush)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].Overline = overline;
            stateChanges[i].OverlineBrush = brush;
         }
      }

      /// <summary>
      /// Sets the Strikthrough property for the specified text segment
      /// </summary>
      public void SetStrikethrough(int offset, int length, bool strikethrough)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].Strikethrough = strikethrough;
         }
      }

      /// <summary>
      /// Sets the Strikthrough property for the specified text segment
      /// </summary>
      public void SetStretch(int offset, int length, FontStretch stretch)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for (int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].FontStretch = stretch;
         }
      }
      /// <summary>
      /// Sets the EOL Marker for the specified text segment
      /// </summary>
      /// <param name="offset"></param>
      public void SetEOLMarker(int offset)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + 1);
         for (int i = startIndex; i < endIndex; i++)
         {
            if (stateChanges[i].EOLMarker.HasValue)
               stateChanges[i].EOLMarker++;
            else
               stateChanges[i].EOLMarker = 1;
         }
      }

      /// <summary>
      /// Sets the Horizontal alignment for the specified text segment
      /// </summary>
      /// <param name="offset"></param>
      /// <param name="length"></param>
      /// <param name="alignment"></param>
      public void SetTextAlignment(int offset, int length, TextAlignment alignment)
      {
         int startIndex = GetIndexForOffset(offset);
         int endIndex = GetIndexForOffset(offset + length);
         for(int i = startIndex; i < endIndex; i++)
         {
            stateChanges[i].TextAlignment = alignment;
         }
      }

      /// <summary>
      /// Retrieves the highlighted sections in the specified range.
      /// The highlighted sections will be sorted by offset, and there will not be any nested or overlapping sections.
      /// </summary>
      public IEnumerable<HighlightedSection> GetHighlightedSections(int offset, int length)
      {
         int index = GetIndexForOffsetUseExistingSegment(offset);
         int pos = offset;
         int endOffset = offset + length;
         while (pos < endOffset)
         {
            int endPos = Math.Min(endOffset, GetEnd(index));
            yield return new HighlightedSection
            {
               Offset = pos,
               Length = endPos - pos,
               Color = stateChanges[index].Clone()
            };
            pos = endPos;
            index++;
         }
      }

      /// <summary>
      /// Creates WPF Run instances that can be used for TextBlock.Inlines.
      /// </summary>
      /// <param name="textSource">The text source that holds the text for this RichTextModel.</param>
      public Run[] CreateRuns(ITextSource textSource)
      {
         Run[] runs = new Run[stateChanges.Count];
         for (int i = 0; i < runs.Length; i++)
         {
            int startOffset = stateChangeOffsets[i];
            int endOffset = i + 1 < stateChangeOffsets.Count ? stateChangeOffsets[i + 1] : textSource.TextLength;
            Run r = new Run(textSource.GetText(startOffset, endOffset - startOffset));
            HighlightingColor state = stateChanges[i];
            RichText.ApplyColorToTextElement(r, state);
            runs[i] = r;
         }
         return runs;
      }
   }
}
