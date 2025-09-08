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
using System.Linq;

using ICSharpCode.AvalonEdit.Document;

using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Search
{
	[TestFixture]
	public class FindTests
	{
		[Test]
		public void SkipWordBorderSimple()
		{
			var strategy = SearchStrategyFactory.Create("All", false, true, SearchMode.Normal);
			var text = new StringTextSource(" FindAllTests ");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();


			Assert.That(results, Is.Empty, "No results should be found!");
		}

		[Test]
		public void SkipWordBorder()
		{
			var strategy = SearchStrategyFactory.Create("AllTests", false, true, SearchMode.Normal);
			var text = new StringTextSource("name=\"{FindAllTests}\"");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results, Is.Empty, "No results should be found!");
		}

		[Test]
		public void SkipWordBorder2()
		{
			var strategy = SearchStrategyFactory.Create("AllTests", false, true, SearchMode.Normal);
			var text = new StringTextSource("name=\"FindAllTests ");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results, Is.Empty, "No results should be found!");
		}

		[Test]
		public void SkipWordBorder3()
		{
			var strategy = SearchStrategyFactory.Create("// find", false, true, SearchMode.Normal);
			var text = new StringTextSource("            // findtest");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results, Is.Empty, "No results should be found!");
		}

		[Test]
		public void WordBorderTest()
		{
			var strategy = SearchStrategyFactory.Create("// find", false, true, SearchMode.Normal);
			var text = new StringTextSource("            // find me");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results.Length, Is.EqualTo(1), "One result should be found!");
			Assert.That(results[0].Offset, Is.EqualTo("            ".Length));
			Assert.That(results[0].Length, Is.EqualTo("// find".Length));
		}

		[Test]
		public void ResultAtStart()
		{
			var strategy = SearchStrategyFactory.Create("result", false, true, SearchMode.Normal);
			var text = new StringTextSource("result           // find me");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results.Length, Is.EqualTo(1), "One result should be found!");
			Assert.That(results[0].Offset, Is.EqualTo(0));
			Assert.That(results[0].Length, Is.EqualTo("result".Length));
		}

		[Test]
		public void ResultAtEnd()
		{
			var strategy = SearchStrategyFactory.Create("me", false, true, SearchMode.Normal);
			var text = new StringTextSource("result           // find me");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results.Length, Is.EqualTo(1), "One result should be found!");
			Assert.That(results[0].Offset, Is.EqualTo("result           // find ".Length));
			Assert.That(results[0].Length, Is.EqualTo("me".Length));
		}

		[Test]
		public void TextWithDots()
		{
			var strategy = SearchStrategyFactory.Create("Text", false, true, SearchMode.Normal);
			var text = new StringTextSource(".Text.");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results.Length, Is.EqualTo(1), "One result should be found!");
			Assert.That(results[0].Offset, Is.EqualTo(".".Length));
			Assert.That(results[0].Length, Is.EqualTo("Text".Length));
		}

		[Test]
		public void SimpleTest()
		{
			var strategy = SearchStrategyFactory.Create("AllTests", false, false, SearchMode.Normal);
			var text = new StringTextSource("name=\"FindAllTests ");
			var results = strategy.FindAll(text, 0, text.TextLength).ToArray();

			Assert.That(results.Length, Is.EqualTo(1), "One result should be found!");
			Assert.That(results[0].Offset, Is.EqualTo("name=\"Find".Length));
			Assert.That(results[0].Length, Is.EqualTo("AllTests".Length));
		}
	}
}
