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

using ICSharpCode.AvalonEdit.Rendering;

using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Document
{
	/// <summary>
	/// A randomized test for the line manager.
	/// </summary>
	[TestFixture]
	public class RandomizedLineManagerTest
	{
		TextDocument document;
		Random rnd;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			int seed = Environment.TickCount;
			Console.WriteLine("RandomizedLineManagerTest Seed: " + seed);
			rnd = new Random(seed);
		}

		[SetUp]
		public void Setup()
		{
			document = new TextDocument();
		}

		[Test]
		public void ShortReplacements()
		{
			char[] chars = { 'a', 'b', '\r', '\n' };
			char[] buffer = new char[20];
			for (int i = 0; i < 2500; i++) {
				int offset = rnd.Next(0, document.TextLength);
				int length = rnd.Next(0, document.TextLength - offset);
				int newTextLength = rnd.Next(0, 20);
				for (int j = 0; j < newTextLength; j++) {
					buffer[j] = chars[rnd.Next(0, chars.Length)];
				}

				document.Replace(offset, length, new string(buffer, 0, newTextLength));
				CheckLines();
			}
		}

		[Test]
		public void LargeReplacements()
		{
			char[] chars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', '\r', '\n' };
			char[] buffer = new char[1000];
			for (int i = 0; i < 20; i++) {
				int offset = rnd.Next(0, document.TextLength);
				int length = rnd.Next(0, (document.TextLength - offset) / 4);
				int newTextLength = rnd.Next(0, 1000);
				for (int j = 0; j < newTextLength; j++) {
					buffer[j] = chars[rnd.Next(0, chars.Length)];
				}

				string newText = new string(buffer, 0, newTextLength);
				string expectedText = document.Text.Remove(offset, length).Insert(offset, newText);
				document.Replace(offset, length, newText);
				Assert.AreEqual(expectedText, document.Text);
				CheckLines();
			}
		}

		void CheckLines()
		{
			string text = document.Text;
			int lineNumber = 1;
			int lineStart = 0;
			for (int i = 0; i < text.Length; i++) {
				char c = text[i];
				if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') {
					DocumentLine line = document.GetLineByNumber(lineNumber);
					Assert.AreEqual(lineNumber, line.LineNumber);
					Assert.AreEqual(2, line.DelimiterLength);
					Assert.AreEqual(lineStart, line.Offset);
					Assert.AreEqual(i - lineStart, line.Length);
					i++; // consume \n
					lineNumber++;
					lineStart = i + 1;
				} else if (c == '\r' || c == '\n') {
					DocumentLine line = document.GetLineByNumber(lineNumber);
					Assert.AreEqual(lineNumber, line.LineNumber);
					Assert.AreEqual(1, line.DelimiterLength);
					Assert.AreEqual(lineStart, line.Offset);
					Assert.AreEqual(i - lineStart, line.Length);
					lineNumber++;
					lineStart = i + 1;
				}
			}
			Assert.AreEqual(lineNumber, document.LineCount);
		}

		[Test]
		public void CollapsingTest()
		{
			char[] chars = { 'a', 'b', '\r', '\n' };
			char[] buffer = new char[20];
			HeightTree heightTree = new HeightTree(document, 10);
			List<CollapsedLineSection> collapsedSections = new List<CollapsedLineSection>();
			for (int i = 0; i < 25000; i++) {
				//				Debug.WriteLine("Iteration " + i);
				//				Debug.WriteLine(heightTree.GetTreeAsString());
				//				foreach (CollapsedLineSection cs in collapsedSections) {
				//					Debug.WriteLine(cs);
				//				}

				int command = rnd.Next(0, 10);
				switch (command) {
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
						int offset = rnd.Next(0, document.TextLength);
						int length;
						if (command == 0) {
							length = rnd.Next(0, document.TextLength - offset);
						} else if (command == 1) {
							length = 0;
						} else {
							length = rnd.Next(0, Math.Min(15, document.TextLength - offset));
						}
						int newTextLength = rnd.Next(0, 20);
						for (int j = 0; j < newTextLength; j++) {
							buffer[j] = chars[rnd.Next(0, chars.Length)];
						}

						document.Replace(offset, length, new string(buffer, 0, newTextLength));
						break;
					case 6:
					case 7:
						int startLine = rnd.Next(1, document.LineCount + 1);
						int endLine = rnd.Next(startLine, document.LineCount + 1);
						collapsedSections.Add(heightTree.CollapseText(document.GetLineByNumber(startLine), document.GetLineByNumber(endLine)));
						break;
					case 8:
						if (collapsedSections.Count > 0) {
							CollapsedLineSection cs = collapsedSections[rnd.Next(0, collapsedSections.Count)];
							// unless the text section containing the CollapsedSection was deleted:
							if (cs.Start != null) {
								cs.Uncollapse();
							}
							collapsedSections.Remove(cs);
						}
						break;
					case 9:
						foreach (DocumentLine ls in document.Lines) {
							heightTree.SetHeight(ls.LineNumber, ls.LineNumber);
						}
						break;
				}
				var treeSections = new HashSet<CollapsedLineSection>(heightTree.GetAllCollapsedSections());
				int expectedCount = 0;
				foreach (CollapsedLineSection cs in collapsedSections) {
					if (cs.Start != null) {
						expectedCount++;
						Assert.IsTrue(treeSections.Contains(cs));
					}
				}
				Assert.AreEqual(expectedCount, treeSections.Count);
				CheckLines();
				HeightTests.CheckHeights(document, heightTree);
			}
		}
	}
}
