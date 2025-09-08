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
using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Document
{
	[TestFixture]
	public class TextUtilitiesTests
	{
		#region GetWhitespaceAfter
		[Test]
		public void TestGetWhitespaceAfter()
		{
			Assert.That(TextUtilities.GetWhitespaceAfter(new StringTextSource("a \t \tb"), 2), Is.EqualTo(new SimpleSegment(2, 3)));
		}
		
		[Test]
		public void TestGetWhitespaceAfterDoesNotSkipNewLine()
		{
			Assert.That(TextUtilities.GetWhitespaceAfter(new StringTextSource("a \t \tb"), 2), Is.EqualTo(new SimpleSegment(2, 3)));
		}
		
		[Test]
		public void TestGetWhitespaceAfterEmptyResult()
		{
			Assert.That(TextUtilities.GetWhitespaceAfter(new StringTextSource("a b"), 2), Is.EqualTo(new SimpleSegment(2, 0)));
		}
		
		[Test]
		public void TestGetWhitespaceAfterEndOfString()
		{
			Assert.That(TextUtilities.GetWhitespaceAfter(new StringTextSource("a "), 2), Is.EqualTo(new SimpleSegment(2, 0)));
		}
		
		[Test]
		public void TestGetWhitespaceAfterUntilEndOfString()
		{
			Assert.That(TextUtilities.GetWhitespaceAfter(new StringTextSource("a \t \t"), 2), Is.EqualTo(new SimpleSegment(2, 3)));
		}
		#endregion
		
		#region GetWhitespaceBefore
		[Test]
		public void TestGetWhitespaceBefore()
		{
			Assert.That(TextUtilities.GetWhitespaceBefore(new StringTextSource("a\t \t b"), 4), Is.EqualTo(new SimpleSegment(1, 3)));
		}
		
		[Test]
		public void TestGetWhitespaceBeforeDoesNotSkipNewLine()
		{
			Assert.That(TextUtilities.GetWhitespaceBefore(new StringTextSource("a\n b"), 3), Is.EqualTo(new SimpleSegment(2, 1)));
		}
		
		[Test]
		public void TestGetWhitespaceBeforeEmptyResult()
		{
			Assert.That(TextUtilities.GetWhitespaceBefore(new StringTextSource(" a b"), 2), Is.EqualTo(new SimpleSegment(2, 0)));
		}
		
		[Test]
		public void TestGetWhitespaceBeforeStartOfString()
		{
			Assert.That(TextUtilities.GetWhitespaceBefore(new StringTextSource(" a"), 0), Is.EqualTo(new SimpleSegment(0, 0)));
		}
		
		[Test]
		public void TestGetWhitespaceBeforeUntilStartOfString()
		{
			Assert.That(TextUtilities.GetWhitespaceBefore(new StringTextSource(" \t a"), 2), Is.EqualTo(new SimpleSegment(0, 2)));
		}
		#endregion
	}
}
