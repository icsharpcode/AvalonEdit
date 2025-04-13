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
using System.IO;
using NUnit.Framework;
using System.Text;

namespace ICSharpCode.AvalonEdit.Utils
{
	[TestFixture]
	public class RopeTests
	{
		[Test]
		public void EmptyRope()
		{
			Rope<char> empty = new Rope<char>();
			Assert.That(empty.Length, Is.EqualTo(0));
			Assert.That(empty.ToString(), Is.Empty);
		}
		
		[Test]
		public void EmptyRopeFromString()
		{
			Rope<char> empty = new Rope<char>(string.Empty);
			Assert.That(empty.Length, Is.EqualTo(0));
			Assert.That(empty.ToString(), Is.Empty);
		}
		
		[Test]
		public void InitializeRopeFromShortString()
		{
			Rope<char> rope = new Rope<char>("Hello, World");
			Assert.That(rope.Length, Is.EqualTo(12));
			Assert.That(rope.ToString(), Is.EqualTo("Hello, World"));
		}
		
		string BuildLongString(int lines)
		{
			StringWriter w = new StringWriter();
			w.NewLine = "\n";
			for (int i = 1; i <= lines; i++) {
				w.WriteLine(i.ToString());
			}
			return w.ToString();
		}
		
		[Test]
		public void InitializeRopeFromLongString()
		{
			string text = BuildLongString(1000);
			Rope<char> rope = new Rope<char>(text);
			Assert.That(rope.Length, Is.EqualTo(text.Length));
			Assert.That(rope.ToString(), Is.EqualTo(text));
			Assert.That(rope.ToArray(), Is.EqualTo(text.ToCharArray()));
		}
		
		[Test]
		public void TestToArrayAndToStringWithParts()
		{
			string text = BuildLongString(1000);
			Rope<char> rope = new Rope<char>(text);
			
			string textPart = text.Substring(1200, 600);
			char[] arrayPart = textPart.ToCharArray();
			Assert.That(rope.ToString(1200, 600), Is.EqualTo(textPart));
			Assert.That(rope.ToArray(1200, 600), Is.EqualTo(arrayPart));
			
			Rope<char> partialRope = rope.GetRange(1200, 600);
			Assert.That(partialRope.ToString(), Is.EqualTo(textPart));
			Assert.That(partialRope.ToArray(), Is.EqualTo(arrayPart));
		}
		
		[Test]
		public void ConcatenateStringToRope()
		{
			StringBuilder b = new StringBuilder();
			Rope<char> rope = new Rope<char>();
			for (int i = 1; i <= 1000; i++) {
				b.Append(i.ToString());
				rope.AddText(i.ToString());
				b.Append(' ');
				rope.Add(' ');
			}
			Assert.That(rope.ToString(), Is.EqualTo(b.ToString()));
		}
		
		[Test]
		public void ConcatenateSmallRopesToRope()
		{
			StringBuilder b = new StringBuilder();
			Rope<char> rope = new Rope<char>();
			for (int i = 1; i <= 1000; i++) {
				b.Append(i.ToString());
				b.Append(' ');
				rope.AddRange(CharRope.Create(i.ToString() + " "));
			}
			Assert.That(rope.ToString(), Is.EqualTo(b.ToString()));
		}
		
		[Test]
		public void AppendLongTextToEmptyRope()
		{
			string text = BuildLongString(1000);
			Rope<char> rope = new Rope<char>();
			rope.AddText(text);
			Assert.That(rope.ToString(), Is.EqualTo(text));
		}
		
		[Test]
		public void ConcatenateStringToRopeBackwards()
		{
			StringBuilder b = new StringBuilder();
			Rope<char> rope = new Rope<char>();
			for (int i = 1; i <= 1000; i++) {
				b.Append(i.ToString());
				b.Append(' ');
			}
			for (int i = 1000; i >= 1; i--) {
				rope.Insert(0, ' ');
				rope.InsertText(0, i.ToString());
			}
			Assert.That(rope.ToString(), Is.EqualTo(b.ToString()));
		}
		
		[Test]
		public void ConcatenateSmallRopesToRopeBackwards()
		{
			StringBuilder b = new StringBuilder();
			Rope<char> rope = new Rope<char>();
			for (int i = 1; i <= 1000; i++) {
				b.Append(i.ToString());
				b.Append(' ');
			}
			for (int i = 1000; i >= 1; i--) {
				rope.InsertRange(0, CharRope.Create(i.ToString() + " "));
			}
			Assert.That(rope.ToString(), Is.EqualTo(b.ToString()));
		}
		
		[Test]
		public void ConcatenateStringToRopeByInsertionInMiddle()
		{
			StringBuilder b = new StringBuilder();
			Rope<char> rope = new Rope<char>();
			for (int i = 1; i <= 998; i++) {
				b.Append(i.ToString("d3"));
				b.Append(' ');
			}
			int middle = 0;
			for (int i = 1; i <= 499; i++) {
				rope.InsertText(middle, i.ToString("d3"));
				middle += 3;
				rope.Insert(middle, ' ');
				middle++;
				rope.InsertText(middle, (999-i).ToString("d3"));
				rope.Insert(middle + 3, ' ');
			}
			Assert.That(rope.ToString(), Is.EqualTo(b.ToString()));
		}
		
		[Test]
		public void ConcatenateSmallRopesByInsertionInMiddle()
		{
			StringBuilder b = new StringBuilder();
			Rope<char> rope = new Rope<char>();
			for (int i = 1; i <= 1000; i++) {
				b.Append(i.ToString("d3"));
				b.Append(' ');
			}
			int middle = 0;
			for (int i = 1; i <= 500; i++) {
				rope.InsertRange(middle, CharRope.Create(i.ToString("d3") + " "));
				middle += 4;
				rope.InsertRange(middle, CharRope.Create((1001-i).ToString("d3") + " "));
			}
			Assert.That(rope.ToString(), Is.EqualTo(b.ToString()));
		}
	}
}
