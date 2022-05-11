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

using System.Windows;
using System.Windows.Media;

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
	}
}
