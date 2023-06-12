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

using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Utils;

using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Highlighting
{
	[TestFixture]
	public class RichTextTests
	{
		[Test]
		public void ConcatTest()
		{
			var textModel = new RichTextModel();
			textModel.SetHighlighting(0, 5, new HighlightingColor { Name = "text1" });
			var text1 = new RichText("text1", textModel);

			var textModel2 = new RichTextModel();
			textModel2.SetHighlighting(0, 5, new HighlightingColor { Name = "text2" });
			var text2 = new RichText("text2", textModel2);

			RichText text3 = RichText.Concat(text1, RichText.Empty, text2);
			Assert.AreEqual(text1.GetHighlightingAt(0), text3.GetHighlightingAt(0));
			Assert.AreNotEqual(text1.GetHighlightingAt(0), text3.GetHighlightingAt(5));
			Assert.AreEqual(text2.GetHighlightingAt(0), text3.GetHighlightingAt(5));
		}

		[Test]
		public void ToHtmlTest()
		{
			var textModel = new RichTextModel();
			textModel.SetBackground(5, 3, new SimpleHighlightingBrush(Colors.Yellow));
			textModel.SetForeground(9, 6, new SimpleHighlightingBrush(Colors.Blue));
			textModel.SetFontWeight(15, 1, FontWeights.Bold);
			var text = new RichText("This has spaces!", textModel);
			var html = text.ToHtml(new HtmlOptions());
			Assert.AreEqual("This&nbsp;<span style=\"background-color: #ffff00; \">has</span>&nbsp;<span style=\"color: #0000ff; \">spaces</span><span style=\"font-weight: bold; \">!</span>", html);
		}

		[Test]
		public void ToRunTest()
		{
			var textModel = new RichTextModel();
			textModel.SetForeground(0,4, new SimpleHighlightingBrush(Colors.Yellow));
			textModel.SetStrikethrough(0, 4, true);
			textModel.SetFontSize(0, 4, 55);
			textModel.SetForeground(4, 4, new SimpleHighlightingBrush(Colors.Black));
			textModel.SetUnderline(4, 4, false);
			textModel.SetFontFamily(4, 4, new FontFamily("Comic Sans MS"));
			textModel.SetForeground(8,99, new SimpleHighlightingBrush(Colors.Blue));
			var text = new RichText("ab\r\nTest as\r\n fast",textModel);
			var runs = text.CreateRuns();

			Run[] expected = new Run[3];
			Run run1 = new Run("ab\r\n");
			run1.Foreground = (new SimpleHighlightingBrush(Colors.Yellow)).GetBrush(null);
			run1.FontSize = 55;
			run1.TextDecorations.Add(TextDecorations.Strikethrough);
			Run run2 = new Run("Test");
			run2.FontFamily = new FontFamily("Comic Sans MS");
			run2.Foreground = (new SimpleHighlightingBrush(Colors.Black)).GetBrush(null);
			Run run3 = new Run(" as\r\n fast");
			run3.Foreground = (new SimpleHighlightingBrush(Colors.Blue)).GetBrush(null);
			expected[0] = run1;
			expected[1] = run2;
			expected[2] = run3;
			Assert.AreEqual(expected.Length, runs.Length);	
			for(int i= 0; i<expected.Length; i++) {
				Assert.AreEqual(expected[i].Foreground.ToString(), runs[i].Foreground.ToString());
				Assert.AreEqual(expected[i].FontSize, runs[i].FontSize);
				Assert.AreEqual(expected[i].Text, runs[i].Text);
				Assert.AreEqual(expected[i].TextDecorations, runs[i].TextDecorations);
			}
		}
		[Test]
		public void ToRunOnLineBreakTest()
		{
			var textModel = new RichTextModel();
			textModel.SetForeground(0, 4, new SimpleHighlightingBrush(Colors.Yellow));
			textModel.SetStrikethrough(0, 4, true);
			textModel.SetFontSize(0, 4, 55);
			textModel.SetForeground(4, 4, new SimpleHighlightingBrush(Colors.Black));
			textModel.SetUnderline(4, 4, false);
			textModel.SetFontFamily(4, 4, new FontFamily("Comic Sans MS"));
			textModel.SetForeground(8,99, new SimpleHighlightingBrush(Colors.Blue));
			var text = new RichText("ab\r\nTest as\r\n fast", textModel);
			var runs = text.CreateRunsOnLineBreaks();

			Run[] expected = new Run[3];
			Run run1 = new Run("ab");
			run1.Foreground = (new SimpleHighlightingBrush(Colors.Yellow)).GetBrush(null);
			run1.FontSize = 55;
			run1.TextDecorations.Add(TextDecorations.Strikethrough);
			Run run2 = new Run("Test as");
			run2.FontFamily = new FontFamily("Comic Sans MS");
			run2.Foreground = (new SimpleHighlightingBrush(Colors.Black)).GetBrush(null);
			Run run3 = new Run(" fast");
			run3.Foreground = (new SimpleHighlightingBrush(Colors.Blue)).GetBrush(null);
			expected[0] = run1;
			expected[1] = run2;
			expected[2] = run3;
			Assert.AreEqual(expected.Length, runs.Length);	
			for (int i = 0; i < expected.Length; i++) {
				Assert.AreEqual(expected[i].Foreground.ToString(), runs[i].Foreground.ToString());
				Assert.AreEqual(expected[i].FontSize, runs[i].FontSize);
				Assert.AreEqual(expected[i].Text, runs[i].Text);
				Assert.AreEqual(expected[i].TextDecorations, runs[i].TextDecorations);
			}
		}

	}
}
