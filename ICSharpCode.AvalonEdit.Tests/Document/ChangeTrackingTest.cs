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

using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Document
{
	[TestFixture]
	public class ChangeTrackingTest
	{
		[Test]
		public void NoChanges()
		{
			TextDocument document = new TextDocument("initial text");
			ITextSource snapshot1 = document.CreateSnapshot();
			ITextSource snapshot2 = document.CreateSnapshot();
			Assert.That(snapshot1.Version.CompareAge(snapshot2.Version), Is.EqualTo(0));
			Assert.That(snapshot1.Version.GetChangesTo(snapshot2.Version).Count(), Is.EqualTo(0));
			Assert.That(snapshot1.Text, Is.EqualTo(document.Text));
			Assert.That(snapshot2.Text, Is.EqualTo(document.Text));
		}

		[Test]
		public void ForwardChanges()
		{
			TextDocument document = new TextDocument("initial text");
			ITextSource snapshot1 = document.CreateSnapshot();
			document.Replace(0, 7, "nw");
			document.Insert(1, "e");
			ITextSource snapshot2 = document.CreateSnapshot();
			Assert.That(snapshot1.Version.CompareAge(snapshot2.Version), Is.EqualTo(-1));
			TextChangeEventArgs[] arr = snapshot1.Version.GetChangesTo(snapshot2.Version).ToArray();
			Assert.That(arr.Length, Is.EqualTo(2));
			Assert.That(arr[0].InsertedText.Text, Is.EqualTo("nw"));
			Assert.That(arr[1].InsertedText.Text, Is.EqualTo("e"));

			Assert.That(snapshot1.Text, Is.EqualTo("initial text"));
			Assert.That(snapshot2.Text, Is.EqualTo("new text"));
		}

		[Test]
		public void BackwardChanges()
		{
			TextDocument document = new TextDocument("initial text");
			ITextSource snapshot1 = document.CreateSnapshot();
			document.Replace(0, 7, "nw");
			document.Insert(1, "e");
			ITextSource snapshot2 = document.CreateSnapshot();
			Assert.That(snapshot2.Version.CompareAge(snapshot1.Version), Is.EqualTo(1));
			TextChangeEventArgs[] arr = snapshot2.Version.GetChangesTo(snapshot1.Version).ToArray();
			Assert.That(arr.Length, Is.EqualTo(2));
			Assert.That(arr[0].InsertedText.Text, Is.Empty);
			Assert.That(arr[1].InsertedText.Text, Is.EqualTo("initial"));

			Assert.That(snapshot1.Text, Is.EqualTo("initial text"));
			Assert.That(snapshot2.Text, Is.EqualTo("new text"));
		}
	}
}
