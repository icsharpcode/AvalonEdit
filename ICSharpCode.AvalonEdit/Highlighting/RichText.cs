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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using AcAvalonEdit.Document;

namespace AcAvalonEdit.Highlighting
{
   /// <summary>
   /// Represents a immutable piece text with highlighting information.
   /// </summary>
   public class RichText
   {
      /// <summary>
      /// The empty string without any formatting information.
      /// </summary>
      public static readonly RichText Empty = new RichText(string.Empty);

      readonly string text;
      internal readonly int[] stateChangeOffsets;
      internal readonly HighlightingColor[] stateChanges;

      /// <summary>
      /// Creates a RichText instance with the given text and RichTextModel.
      /// </summary>
      /// <param name="text">
      /// The text to use in this RichText instance.
      /// </param>
      /// <param name="model">
      /// The model that contains the formatting to use for this RichText instance.
      /// <c>model.DocumentLength</c> should correspond to <c>text.Length</c>.
      /// This parameter may be null, in which case the RichText instance just holds plain text.
      /// </param>
      public RichText(string text, RichTextModel model = null)
      {
         if (text == null)
            throw new ArgumentNullException("text");
         this.text = text;
         if (model != null)
         {
            var sections = model.GetHighlightedSections(0, text.Length).ToArray();
            stateChangeOffsets = new int[sections.Length];
            stateChanges = new HighlightingColor[sections.Length];
            for (int i = 0; i < sections.Length; i++)
            {
               stateChangeOffsets[i] = sections[i].Offset;
               stateChanges[i] = sections[i].Color;
            }
         }
         else
         {
            stateChangeOffsets = new int[] { 0 };
            stateChanges = new HighlightingColor[] { HighlightingColor.Empty };
         }
      }

      internal RichText(string text, int[] offsets, HighlightingColor[] states)
      {
         this.text = text;
         Debug.Assert(offsets[0] == 0);
         Debug.Assert(offsets.Last() <= text.Length);
         this.stateChangeOffsets = offsets;
         this.stateChanges = states;
      }

      /// <summary>
      /// Gets the text.
      /// </summary>
      public string Text
      {
         get { return text; }
      }

      /// <summary>
      /// Gets the text length.
      /// </summary>
      public int Length
      {
         get { return text.Length; }
      }

      int GetIndexForOffset(int offset)
      {
         if (offset < 0 || offset > text.Length)
            throw new ArgumentOutOfRangeException("offset");
         int index = Array.BinarySearch(stateChangeOffsets, offset);
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
         if (index + 1 < stateChangeOffsets.Length)
            return stateChangeOffsets[index + 1];
         else
            return text.Length;
      }

      /// <summary>
      /// Gets the HighlightingColor for the specified offset.
      /// </summary>
      public HighlightingColor GetHighlightingAt(int offset)
      {
         return stateChanges[GetIndexForOffset(offset)];
      }

      /// <summary>
      /// Retrieves the highlighted sections in the specified range.
      /// The highlighted sections will be sorted by offset, and there will not be any nested or overlapping sections.
      /// </summary>
      public IEnumerable<HighlightedSection> GetHighlightedSections(int offset, int length)
      {
         int index = GetIndexForOffset(offset);
         int pos = offset;
         int endOffset = offset + length;
         while (pos < endOffset)
         {
            int endPos = Math.Min(endOffset, GetEnd(index));
            yield return new HighlightedSection
            {
               Offset = pos,
               Length = endPos - pos,
               Color = stateChanges[index]
            };
            pos = endPos;
            index++;
         }
      }

      /// <summary>
      /// Creates a new RichTextModel with the formatting from this RichText.
      /// </summary>
      public RichTextModel ToRichTextModel()
      {
         return new RichTextModel(stateChangeOffsets, stateChanges.Select(ch => ch.Clone()).ToArray());
      }

      /// <summary>
      /// Gets the text.
      /// </summary>
      public override string ToString()
      {
         return text;
      }

      /// <summary>
      /// Creates WPF Run instances that can be used for TextBlock.Inlines.
      /// </summary>
      public Run[] CreateRuns()
      {
         Run[] runs = new Run[stateChanges.Length];
         for (int i = 0; i < runs.Length; i++)
         {
            int startOffset = stateChangeOffsets[i];
            int endOffset = i + 1 < stateChangeOffsets.Length ? stateChangeOffsets[i + 1] : text.Length;
            Run r = new Run(text.Substring(startOffset, endOffset - startOffset));
            HighlightingColor state = stateChanges[i];
            ApplyColorToTextElement(r, state);
            runs[i] = r;
         }
         return runs;
      }

      /// <summary>
      /// Creates WPF Run instances on each new Line, using the active Highlighting at the start of the line for the whole Run. 
      /// </summary>
      public Run[] CreateRunsOnLineBreaks()
      {
         if (Text.Length == 0)
            return Array.Empty<Run>();
         var lines = text.Split(Environment.NewLine);
         int relevantIndex = 0;
         Run[] FormattedRuns = new Run[lines.Length];

         Span<int> CountOfLineBreaks = stackalloc int[stateChanges.Length];
         int linebreaks = FindStateChangesWithLinebreaks(CountOfLineBreaks);

         Span<int> Linebreakindices = stackalloc int[linebreaks];

         Linebreakindices = GetHiddenLinebreakIndices(Linebreakindices, CountOfLineBreaks);

         CountOfLineBreaks = stackalloc int[linebreaks];

         CountOfLineBreaks = GetNumberOfLinebreaksAtPosition(CountOfLineBreaks);

         for (int i = 0; i < FormattedRuns.Length; i++)
         {
            int lengthUntil = lines.Take(i).Sum(x => x.Length) + (i * Environment.NewLine.Length);
            for (int j = 0; j < stateChangeOffsets.Length; j++)
            {
               if (stateChangeOffsets[j] == lengthUntil)
               {
                  relevantIndex = j;
                  break;
               }
               if (j == stateChangeOffsets.Length - 1)
               {
                  relevantIndex = j;
                  break;
               }
               if (stateChangeOffsets[j] > lengthUntil)
               {
                  relevantIndex = j - 1;
                  break;
               }
            }

            string OutputLineString = InsertLinebreaks(lines, CountOfLineBreaks, Linebreakindices, i, lengthUntil);

            Run run = new(OutputLineString);
            HighlightingColor state = stateChanges[relevantIndex];
            ApplyColorToTextElement(run, state);
            FormattedRuns[i] = run;
         }

         return FormattedRuns;
      }

      private static string InsertLinebreaks(string[] lines, Span<int> CountOfLineBreaks, Span<int> Linebreakindices, int i, int lengthUntil)
      {
         string OutputLineString = lines[i];
         int InsertedOffset = 0;
         for (int k = 0; k < Linebreakindices.Length; k++)
         {
            if (Linebreakindices[k] >= lengthUntil && Linebreakindices[k] < lengthUntil + lines[i].Length)
            {
               for (int x = 0; x < CountOfLineBreaks[k]; x++)
               {
                  OutputLineString = OutputLineString.Insert((InsertedOffset + Linebreakindices[k]) - lengthUntil, Environment.NewLine);
                  InsertedOffset += Environment.NewLine.Length;
               }
            }
         }

         return OutputLineString;
      }

      private Span<int> GetNumberOfLinebreaksAtPosition(Span<int> CountOfLineBreaks)
      {
         int index = 0;
         for (int i = 0; i < stateChanges.Length; i++)
         {
            if (stateChanges[i].EOLMarker.HasValue)
               CountOfLineBreaks[index++] = stateChanges[i].EOLMarker.Value;
         }
         return CountOfLineBreaks;
      }

      private Span<int> GetHiddenLinebreakIndices(Span<int> Linebreakindices, Span<int> CountOfLineBreaks)
      {
         int index = 0;

         for (int i = 0; i < CountOfLineBreaks.Length; i++)
         {
            if (CountOfLineBreaks[i] >= 0)
               Linebreakindices[index++] = stateChangeOffsets[i];
         }
         return Linebreakindices;
      }

      private int FindStateChangesWithLinebreaks(Span<int> CountOfLineBreaks)
      {
         int counter = 0;
         CountOfLineBreaks.Fill(-1);
         for (int i = 0; i < CountOfLineBreaks.Length; i++)
         {
            if (stateChanges[i].EOLMarker.HasValue)
            {
               CountOfLineBreaks[i] = i;
               counter++;
            }
         }
         return counter;
      }

      internal static void ApplyColorToTextElement(Run r, HighlightingColor state)
      {
         if (state.Foreground != null)
            r.Foreground = state.Foreground.GetBrush(null);
         if (state.Background != null)
            r.Background = state.Background.GetBrush(null);
         if (state.FontWeight != null)
            r.FontWeight = state.FontWeight.Value;
         if (state.FontStyle != null)
            r.FontStyle = state.FontStyle.Value;
         if (state.FontFamily != null)
            r.FontFamily = state.FontFamily;
         if (state.FontSize != null)
         {
            r.FontSize = state.FontSize.Value;
         }
         if (state.Strikethrough != null && state.Strikethrough.Value)
         {
            r.TextDecorations.Add(System.Windows.TextDecorations.Strikethrough);
         }
         if (state.Underline != null && state.Underline.Value)
         {
            r.TextDecorations.Add(System.Windows.TextDecorations.Underline);
         }
         if (state.FontStretch != null)
         {
            r.FontStretch = state.FontStretch.Value;
         }
         if(state.TextAlignment != null)
         {
            r.BaselineAlignment = (BaselineAlignment)(int)state.TextAlignment;
         }

      }

      /// <summary>
      /// Produces HTML code for the line, with &lt;span style="..."&gt; tags.
      /// </summary>
      public string ToHtml(HtmlOptions options = null)
      {
         StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
         using (var htmlWriter = new HtmlRichTextWriter(stringWriter, options))
         {
            htmlWriter.Write(this);
         }
         return stringWriter.ToString();
      }

      /// <summary>
      /// Produces HTML code for a section of the line, with &lt;span style="..."&gt; tags.
      /// </summary>
      public string ToHtml(int offset, int length, HtmlOptions options = null)
      {
         StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
         using (var htmlWriter = new HtmlRichTextWriter(stringWriter, options))
         {
            htmlWriter.Write(this, offset, length);
         }
         return stringWriter.ToString();
      }

      /// <summary>
      /// Creates a substring of this rich text.
      /// </summary>
      public RichText Substring(int offset, int length)
      {
         if (offset == 0 && length == this.Length)
            return this;
         string newText = text.Substring(offset, length);
         RichTextModel model = ToRichTextModel();
         OffsetChangeMap map = new OffsetChangeMap(2);
         map.Add(new OffsetChangeMapEntry(offset + length, text.Length - offset - length, 0));
         map.Add(new OffsetChangeMapEntry(0, offset, 0));
         model.UpdateOffsets(map);
         return new RichText(newText, model);
      }

      /// <summary>
      /// Concatenates the specified rich texts.
      /// </summary>
      public static RichText Concat(params RichText[] texts)
      {
         if (texts == null || texts.Length == 0)
            return Empty;
         else if (texts.Length == 1)
            return texts[0];
         string newText = string.Concat(texts.Select(txt => txt.text));
         RichTextModel model = texts[0].ToRichTextModel();
         int offset = texts[0].Length;
         for (int i = 1; i < texts.Length; i++)
         {
            model.Append(offset, texts[i].stateChangeOffsets, texts[i].stateChanges);
            offset += texts[i].Length;
         }
         return new RichText(newText, model);
      }

      /// <summary>
      /// Concatenates the specified rich texts.
      /// </summary>
      public static RichText operator +(RichText a, RichText b)
      {
         return RichText.Concat(a, b);
      }

      /// <summary>
      /// Implicit conversion from string to RichText.
      /// </summary>
      public static implicit operator RichText(string text)
      {
         if (text != null)
            return new RichText(text);
         else
            return null;
      }
   }
}
