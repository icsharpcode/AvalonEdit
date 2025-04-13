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

using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Document
{
	[TestFixture]
	public class LineManagerTests
	{
		TextDocument document;

		[SetUp]
		public void SetUp()
		{
			document = new TextDocument();
		}

		[Test]
		public void CheckEmptyDocument()
		{
			Assert.That(document.Text, Is.Empty);
			Assert.That(document.TextLength, Is.EqualTo(0));
			Assert.That(document.LineCount, Is.EqualTo(1));
		}

		[Test]
		public void CheckClearingDocument()
		{
			document.Text = "Hello,\nWorld!";
			Assert.That(document.LineCount, Is.EqualTo(2));
			var oldLines = document.Lines.ToArray();
			document.Text = "";
			Assert.That(document.Text, Is.Empty);
			Assert.That(document.TextLength, Is.EqualTo(0));
			Assert.That(document.LineCount, Is.EqualTo(1));
			Assert.That(document.Lines.Single(), Is.SameAs(oldLines[0]));
			Assert.That(oldLines[0].IsDeleted, Is.False);
			Assert.That(oldLines[1].IsDeleted, Is.True);
			Assert.That(oldLines[0].NextLine, Is.Null);
			Assert.That(oldLines[1].PreviousLine, Is.Null);
		}

		[Test]
		public void CheckGetLineInEmptyDocument()
		{
			Assert.That(document.Lines.Count, Is.EqualTo(1));
			List<DocumentLine> lines = new List<DocumentLine>(document.Lines);
			Assert.That(lines.Count, Is.EqualTo(1));
			DocumentLine line = document.Lines[0];
			Assert.That(lines[0], Is.SameAs(line));
			Assert.That(document.GetLineByNumber(1), Is.SameAs(line));
			Assert.That(document.GetLineByOffset(0), Is.SameAs(line));
		}

		[Test]
		public void CheckLineSegmentInEmptyDocument()
		{
			DocumentLine line = document.GetLineByNumber(1);
			Assert.That(line.LineNumber, Is.EqualTo(1));
			Assert.That(line.Offset, Is.EqualTo(0));
			Assert.That(line.IsDeleted, Is.False);
			Assert.That(line.Length, Is.EqualTo(0));
			Assert.That(line.TotalLength, Is.EqualTo(0));
			Assert.That(line.DelimiterLength, Is.EqualTo(0));
		}

		[Test]
		public void LineIndexOfTest()
		{
			DocumentLine line = document.GetLineByNumber(1);
			Assert.That(document.Lines.IndexOf(line), Is.EqualTo(0));
			DocumentLine lineFromOtherDocument = new TextDocument().GetLineByNumber(1);
			Assert.That(document.Lines.IndexOf(lineFromOtherDocument), Is.EqualTo(-1));
			document.Text = "a\nb\nc";
			DocumentLine middleLine = document.GetLineByNumber(2);
			Assert.That(document.Lines.IndexOf(middleLine), Is.EqualTo(1));
			document.Remove(1, 3);
			Assert.That(middleLine.IsDeleted, Is.True);
			Assert.That(document.Lines.IndexOf(middleLine), Is.EqualTo(-1));
		}

		[Test]
		public void InsertInEmptyDocument()
		{
			document.Insert(0, "a");
			Assert.That(document.LineCount, Is.EqualTo(1));
			DocumentLine line = document.GetLineByNumber(1);
			Assert.That(document.GetText(line), Is.EqualTo("a"));
		}

		[Test]
		public void SetText()
		{
			document.Text = "a";
			Assert.That(document.LineCount, Is.EqualTo(1));
			DocumentLine line = document.GetLineByNumber(1);
			Assert.That(document.GetText(line), Is.EqualTo("a"));
		}

		[Test]
		public void InsertNothing()
		{
			document.Insert(0, "");
			Assert.That(document.LineCount, Is.EqualTo(1));
			Assert.That(document.TextLength, Is.EqualTo(0));
		}

		[Test]
		public void InsertNull()
		{
			Assert.Throws<ArgumentNullException>(() => document.Insert(0, (string)null));
		}

		[Test]
		public void SetTextNull()
		{
			Assert.Throws<ArgumentNullException>(() => document.Text = null);
		}

		[Test]
		public void RemoveNothing()
		{
			document.Remove(0, 0);
			Assert.That(document.LineCount, Is.EqualTo(1));
			Assert.That(document.TextLength, Is.EqualTo(0));
		}

		[Test]
		public void GetCharAt0EmptyDocument()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => document.GetCharAt(0));
		}

		[Test]
		public void GetCharAtNegativeOffset()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.GetCharAt(-1);
			});
		}

		[Test]
		public void GetCharAtEndOffset()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.GetCharAt(document.TextLength);
			});
		}

		[Test]
		public void InsertAtNegativeOffset()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.Insert(-1, "text");
			});
		}

		[Test]
		public void InsertAfterEndOffset()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.Insert(4, "text");
			});
		}

		[Test]
		public void RemoveNegativeAmount()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "abcd";
				document.Remove(2, -1);
			});
		}

		[Test]
		public void RemoveTooMuch()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "abcd";
				document.Remove(2, 10);
			});
		}

		[Test]
		public void GetLineByNumberNegative()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.GetLineByNumber(-1);
			});
		}

		[Test]
		public void GetLineByNumberTooHigh()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.GetLineByNumber(3);
			});
		}

		[Test]
		public void GetLineByOffsetNegative()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.GetLineByOffset(-1);
			});
		}


		[Test]
		public void GetLineByOffsetToHigh()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				document.Text = "a\nb";
				document.GetLineByOffset(10);
			});
		}

		[Test]
		public void InsertAtEndOffset()
		{
			document.Text = "a\nb";
			CheckDocumentLines("a",
							   "b");
			document.Insert(3, "text");
			CheckDocumentLines("a",
							   "btext");
		}

		[Test]
		public void GetCharAt()
		{
			document.Text = "a\r\nb";
			Assert.That(document.GetCharAt(0), Is.EqualTo('a'));
			Assert.That(document.GetCharAt(1), Is.EqualTo('\r'));
			Assert.That(document.GetCharAt(2), Is.EqualTo('\n'));
			Assert.That(document.GetCharAt(3), Is.EqualTo('b'));
		}

		[Test]
		public void CheckMixedNewLineTest()
		{
			const string mixedNewlineText = "line 1\nline 2\r\nline 3\rline 4";
			document.Text = mixedNewlineText;
			Assert.That(document.Text, Is.EqualTo(mixedNewlineText));
			Assert.That(document.LineCount, Is.EqualTo(4));
			for (int i = 1; i < 4; i++) {
				DocumentLine line = document.GetLineByNumber(i);
				Assert.That(line.LineNumber, Is.EqualTo(i));
				Assert.That(document.GetText(line), Is.EqualTo("line " + i));
			}
			Assert.That(document.GetLineByNumber(1).DelimiterLength, Is.EqualTo(1));
			Assert.That(document.GetLineByNumber(2).DelimiterLength, Is.EqualTo(2));
			Assert.That(document.GetLineByNumber(3).DelimiterLength, Is.EqualTo(1));
			Assert.That(document.GetLineByNumber(4).DelimiterLength, Is.EqualTo(0));
		}

		[Test]
		public void LfCrIsTwoNewLinesTest()
		{
			document.Text = "a\n\rb";
			Assert.That(document.Text, Is.EqualTo("a\n\rb"));
			CheckDocumentLines("a",
							   "",
							   "b");
		}

		[Test]
		public void RemoveFirstPartOfDelimiter()
		{
			document.Text = "a\r\nb";
			document.Remove(1, 1);
			Assert.That(document.Text, Is.EqualTo("a\nb"));
			CheckDocumentLines("a",
							   "b");
		}

		[Test]
		public void RemoveLineContentAndJoinDelimiters()
		{
			document.Text = "a\rb\nc";
			document.Remove(2, 1);
			Assert.That(document.Text, Is.EqualTo("a\r\nc"));
			CheckDocumentLines("a",
							   "c");
		}

		[Test]
		public void RemoveLineContentAndJoinDelimiters2()
		{
			document.Text = "a\rb\nc\nd";
			document.Remove(2, 3);
			Assert.That(document.Text, Is.EqualTo("a\r\nd"));
			CheckDocumentLines("a",
							   "d");
		}

		[Test]
		public void RemoveLineContentAndJoinDelimiters3()
		{
			document.Text = "a\rb\r\nc";
			document.Remove(2, 2);
			Assert.That(document.Text, Is.EqualTo("a\r\nc"));
			CheckDocumentLines("a",
							   "c");
		}

		[Test]
		public void RemoveLineContentAndJoinNonMatchingDelimiters()
		{
			document.Text = "a\nb\nc";
			document.Remove(2, 1);
			Assert.That(document.Text, Is.EqualTo("a\n\nc"));
			CheckDocumentLines("a",
							   "",
							   "c");
		}

		[Test]
		public void RemoveLineContentAndJoinNonMatchingDelimiters2()
		{
			document.Text = "a\nb\rc";
			document.Remove(2, 1);
			Assert.That(document.Text, Is.EqualTo("a\n\rc"));
			CheckDocumentLines("a",
							   "",
							   "c");
		}

		[Test]
		public void RemoveMultilineUpToFirstPartOfDelimiter()
		{
			document.Text = "0\n1\r\n2";
			document.Remove(1, 3);
			Assert.That(document.Text, Is.EqualTo("0\n2"));
			CheckDocumentLines("0",
							   "2");
		}

		[Test]
		public void RemoveSecondPartOfDelimiter()
		{
			document.Text = "a\r\nb";
			document.Remove(2, 1);
			Assert.That(document.Text, Is.EqualTo("a\rb"));
			CheckDocumentLines("a",
							   "b");
		}

		[Test]
		public void RemoveFromSecondPartOfDelimiter()
		{
			document.Text = "a\r\nb\nc";
			document.Remove(2, 3);
			Assert.That(document.Text, Is.EqualTo("a\rc"));
			CheckDocumentLines("a",
							   "c");
		}

		[Test]
		public void RemoveFromSecondPartOfDelimiterToDocumentEnd()
		{
			document.Text = "a\r\nb";
			document.Remove(2, 2);
			Assert.That(document.Text, Is.EqualTo("a\r"));
			CheckDocumentLines("a",
							   "");
		}

		[Test]
		public void RemoveUpToMatchingDelimiter1()
		{
			document.Text = "a\r\nb\nc";
			document.Remove(2, 2);
			Assert.That(document.Text, Is.EqualTo("a\r\nc"));
			CheckDocumentLines("a",
							   "c");
		}

		[Test]
		public void RemoveUpToMatchingDelimiter2()
		{
			document.Text = "a\r\nb\r\nc";
			document.Remove(2, 3);
			Assert.That(document.Text, Is.EqualTo("a\r\nc"));
			CheckDocumentLines("a",
							   "c");
		}

		[Test]
		public void RemoveUpToNonMatchingDelimiter()
		{
			document.Text = "a\r\nb\rc";
			document.Remove(2, 2);
			Assert.That(document.Text, Is.EqualTo("a\r\rc"));
			CheckDocumentLines("a",
							   "",
							   "c");
		}

		[Test]
		public void RemoveTwoCharDelimiter()
		{
			document.Text = "a\r\nb";
			document.Remove(1, 2);
			Assert.That(document.Text, Is.EqualTo("ab"));
			CheckDocumentLines("ab");
		}

		[Test]
		public void RemoveOneCharDelimiter()
		{
			document.Text = "a\nb";
			document.Remove(1, 1);
			Assert.That(document.Text, Is.EqualTo("ab"));
			CheckDocumentLines("ab");
		}

		void CheckDocumentLines(params string[] lines)
		{
			Assert.That(document.LineCount, Is.EqualTo(lines.Length), "LineCount");
			for (int i = 0; i < lines.Length; i++) {
				Assert.That(document.GetText(document.Lines[i]), Is.EqualTo(lines[i]), "Text of line " + (i + 1));
			}
		}

		[Test]
		public void FixUpFirstPartOfDelimiter()
		{
			document.Text = "a\n\nb";
			document.Replace(1, 1, "\r");
			Assert.That(document.Text, Is.EqualTo("a\r\nb"));
			CheckDocumentLines("a",
							   "b");
		}

		[Test]
		public void FixUpSecondPartOfDelimiter()
		{
			document.Text = "a\r\rb";
			document.Replace(2, 1, "\n");
			Assert.That(document.Text, Is.EqualTo("a\r\nb"));
			CheckDocumentLines("a",
							   "b");
		}

		[Test]
		public void InsertInsideDelimiter()
		{
			document.Text = "a\r\nc";
			document.Insert(2, "b");
			Assert.That(document.Text, Is.EqualTo("a\rb\nc"));
			CheckDocumentLines("a",
							   "b",
							   "c");
		}

		[Test]
		public void InsertInsideDelimiter2()
		{
			document.Text = "a\r\nd";
			document.Insert(2, "b\nc");
			Assert.That(document.Text, Is.EqualTo("a\rb\nc\nd"));
			CheckDocumentLines("a",
							   "b",
							   "c",
							   "d");
		}

		[Test]
		public void InsertInsideDelimiter3()
		{
			document.Text = "a\r\nc";
			document.Insert(2, "b\r");
			Assert.That(document.Text, Is.EqualTo("a\rb\r\nc"));
			CheckDocumentLines("a",
							   "b",
							   "c");
		}

		[Test]
		public void ExtendDelimiter1()
		{
			document.Text = "a\nc";
			document.Insert(1, "b\r");
			Assert.That(document.Text, Is.EqualTo("ab\r\nc"));
			CheckDocumentLines("ab",
							   "c");
		}

		[Test]
		public void ExtendDelimiter2()
		{
			document.Text = "a\rc";
			document.Insert(2, "\nb");
			Assert.That(document.Text, Is.EqualTo("a\r\nbc"));
			CheckDocumentLines("a",
							   "bc");
		}

		[Test]
		public void ReplaceLineContentBetweenMatchingDelimiters()
		{
			document.Text = "a\rb\nc";
			document.Replace(2, 1, "x");
			Assert.That(document.Text, Is.EqualTo("a\rx\nc"));
			CheckDocumentLines("a",
							   "x",
							   "c");
		}

		[Test]
		public void GetOffset()
		{
			document.Text = "Hello,\nWorld!";
			Assert.That(document.GetOffset(1, 1), Is.EqualTo(0));
			Assert.That(document.GetOffset(1, 2), Is.EqualTo(1));
			Assert.That(document.GetOffset(1, 6), Is.EqualTo(5));
			Assert.That(document.GetOffset(1, 7), Is.EqualTo(6));
			Assert.That(document.GetOffset(2, 1), Is.EqualTo(7));
			Assert.That(document.GetOffset(2, 2), Is.EqualTo(8));
			Assert.That(document.GetOffset(2, 6), Is.EqualTo(12));
			Assert.That(document.GetOffset(2, 7), Is.EqualTo(13));
		}

		[Test]
		public void GetOffsetIgnoreNegativeColumns()
		{
			document.Text = "Hello,\nWorld!";
			Assert.That(document.GetOffset(1, -1), Is.EqualTo(0));
			Assert.That(document.GetOffset(1, -100), Is.EqualTo(0));
			Assert.That(document.GetOffset(1, 0), Is.EqualTo(0));
			Assert.That(document.GetOffset(2, -1), Is.EqualTo(7));
			Assert.That(document.GetOffset(2, -100), Is.EqualTo(7));
			Assert.That(document.GetOffset(2, 0), Is.EqualTo(7));
		}

		[Test]
		public void GetOffsetIgnoreTooHighColumns()
		{
			document.Text = "Hello,\nWorld!";
			Assert.That(document.GetOffset(1, 8), Is.EqualTo(6));
			Assert.That(document.GetOffset(1, 100), Is.EqualTo(6));
			Assert.That(document.GetOffset(2, 8), Is.EqualTo(13));
			Assert.That(document.GetOffset(2, 100), Is.EqualTo(13));
		}
	}
}
