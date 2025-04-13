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
using System.Windows.Documents;
using ICSharpCode.AvalonEdit.Document;
using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Utils
{
	[TestFixture]
	public class CaretNavigationTests
	{
		int GetNextCaretStop(string text, int offset, CaretPositioningMode mode)
		{
			return TextUtilities.GetNextCaretPosition(new StringTextSource(text), offset, LogicalDirection.Forward, mode);
		}
		
		int GetPrevCaretStop(string text, int offset, CaretPositioningMode mode)
		{
			return TextUtilities.GetNextCaretPosition(new StringTextSource(text), offset, LogicalDirection.Backward, mode);
		}
		
		[Test]
		public void CaretStopInEmptyString()
		{
			Assert.That(GetNextCaretStop("", -1, CaretPositioningMode.Normal), Is.EqualTo(0));
			Assert.That(GetNextCaretStop("", 0, CaretPositioningMode.Normal), Is.EqualTo(-1));
			Assert.That(GetPrevCaretStop("", 0, CaretPositioningMode.Normal), Is.EqualTo(-1));
			Assert.That(GetPrevCaretStop("", 1, CaretPositioningMode.Normal), Is.EqualTo(0));

			Assert.That(GetNextCaretStop("", -1, CaretPositioningMode.WordStart), Is.EqualTo(-1));
			Assert.That(GetNextCaretStop("", -1, CaretPositioningMode.WordBorder), Is.EqualTo(-1));
			Assert.That(GetPrevCaretStop("", 1, CaretPositioningMode.WordStart), Is.EqualTo(-1));
			Assert.That(GetPrevCaretStop("", 1, CaretPositioningMode.WordBorder), Is.EqualTo(-1));
		}
		
		[Test]
		public void StartOfDocumentWithWordStart()
		{
			Assert.That(GetNextCaretStop("word", -1, CaretPositioningMode.Normal), Is.EqualTo(0));
			Assert.That(GetNextCaretStop("word", -1, CaretPositioningMode.WordStart), Is.EqualTo(0));
			Assert.That(GetNextCaretStop("word", -1, CaretPositioningMode.WordBorder), Is.EqualTo(0));

			Assert.That(GetPrevCaretStop("word", 1, CaretPositioningMode.Normal), Is.EqualTo(0));
			Assert.That(GetPrevCaretStop("word", 1, CaretPositioningMode.WordStart), Is.EqualTo(0));
			Assert.That(GetPrevCaretStop("word", 1, CaretPositioningMode.WordBorder), Is.EqualTo(0));
		}
		
		[Test]
		public void StartOfDocumentNoWordStart()
		{
			Assert.That(GetNextCaretStop(" word", -1, CaretPositioningMode.Normal), Is.EqualTo(0));
			Assert.That(GetNextCaretStop(" word", -1, CaretPositioningMode.WordStart), Is.EqualTo(1));
			Assert.That(GetNextCaretStop(" word", -1, CaretPositioningMode.WordBorder), Is.EqualTo(1));

			Assert.That(GetPrevCaretStop(" word", 1, CaretPositioningMode.Normal), Is.EqualTo(0));
			Assert.That(GetPrevCaretStop(" word", 1, CaretPositioningMode.WordStart), Is.EqualTo(-1));
			Assert.That(GetPrevCaretStop(" word", 1, CaretPositioningMode.WordBorder), Is.EqualTo(-1));
		}
		
		[Test]
		public void EndOfDocumentWordBorder()
		{
			Assert.That(GetNextCaretStop("word", 3, CaretPositioningMode.Normal), Is.EqualTo(4));
			Assert.That(GetNextCaretStop("word", 3, CaretPositioningMode.WordStart), Is.EqualTo(-1));
			Assert.That(GetNextCaretStop("word", 3, CaretPositioningMode.WordBorder), Is.EqualTo(4));

			Assert.That(GetPrevCaretStop("word", 5, CaretPositioningMode.Normal), Is.EqualTo(4));
			Assert.That(GetPrevCaretStop("word", 5, CaretPositioningMode.WordStart), Is.EqualTo(0));
			Assert.That(GetPrevCaretStop("word", 5, CaretPositioningMode.WordBorder), Is.EqualTo(4));
		}
		
		[Test]
		public void EndOfDocumentNoWordBorder()
		{
			Assert.That(GetNextCaretStop("txt ", 3, CaretPositioningMode.Normal), Is.EqualTo(4));
			Assert.That(GetNextCaretStop("txt ", 3, CaretPositioningMode.WordStart), Is.EqualTo(-1));
			Assert.That(GetNextCaretStop("txt ", 3, CaretPositioningMode.WordBorder), Is.EqualTo(-1));

			Assert.That(GetPrevCaretStop("txt ", 5, CaretPositioningMode.Normal), Is.EqualTo(4));
			Assert.That(GetPrevCaretStop("txt ", 5, CaretPositioningMode.WordStart), Is.EqualTo(0));
			Assert.That(GetPrevCaretStop("txt ", 5, CaretPositioningMode.WordBorder), Is.EqualTo(3));
		}
		
		[Test]
		public void SingleCharacterOutsideBMP()
		{
			string c = "\U0001D49E";
			Assert.That(GetNextCaretStop(c, 0, CaretPositioningMode.Normal), Is.EqualTo(2));
			Assert.That(GetPrevCaretStop(c, 2, CaretPositioningMode.Normal), Is.EqualTo(0));
		}
		
		[Test]
		public void DetectWordBordersOutsideBMP()
		{
			string c = " a\U0001D49Eb ";
			Assert.That(GetNextCaretStop(c, 0, CaretPositioningMode.WordBorder), Is.EqualTo(1));
			Assert.That(GetNextCaretStop(c, 1, CaretPositioningMode.WordBorder), Is.EqualTo(5));

			Assert.That(GetPrevCaretStop(c, 6, CaretPositioningMode.WordBorder), Is.EqualTo(5));
			Assert.That(GetPrevCaretStop(c, 5, CaretPositioningMode.WordBorder), Is.EqualTo(1));
		}
		
		[Test]
		public void DetectWordBordersOutsideBMP2()
		{
			string c = " \U0001D49E\U0001D4AA ";
			Assert.That(GetNextCaretStop(c, 0, CaretPositioningMode.WordBorder), Is.EqualTo(1));
			Assert.That(GetNextCaretStop(c, 1, CaretPositioningMode.WordBorder), Is.EqualTo(5));

			Assert.That(GetPrevCaretStop(c, 6, CaretPositioningMode.WordBorder), Is.EqualTo(5));
			Assert.That(GetPrevCaretStop(c, 5, CaretPositioningMode.WordBorder), Is.EqualTo(1));
		}
		
		[Test]
		public void CombiningMark()
		{
			string str = " x͆ ";
			Assert.That(GetNextCaretStop(str, 1, CaretPositioningMode.Normal), Is.EqualTo(3));
			Assert.That(GetPrevCaretStop(str, 3, CaretPositioningMode.Normal), Is.EqualTo(1));
		}
		
		[Test]
		public void StackedCombiningMark()
		{
			string str = " x͆͆͆͆ ";
			Assert.That(GetNextCaretStop(str, 1, CaretPositioningMode.Normal), Is.EqualTo(6));
			Assert.That(GetPrevCaretStop(str, 6, CaretPositioningMode.Normal), Is.EqualTo(1));
		}
		
		[Test]
		public void SingleClosingBraceAtLineEnd()
		{
			string str = "\t\t}";
			Assert.That(GetNextCaretStop(str, 1, CaretPositioningMode.WordStart), Is.EqualTo(2));
			Assert.That(GetPrevCaretStop(str, 1, CaretPositioningMode.WordStart), Is.EqualTo(-1));
		}
	}
}
