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

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using NUnit.Framework;
using System;
using System.Windows.Threading;

namespace ICSharpCode.AvalonEdit
{
	[TestFixture]
	public class WeakReferenceTests
	{
		[Test]
		public void TextViewCanBeCollectedTest()
		{
			TextView textView = new TextView();
			WeakReference wr = new WeakReference(textView);
			textView = null;
			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
		}

		[Test]
		public void DocumentDoesNotHoldReferenceToTextView()
		{
			TextDocument textDocument = new TextDocument();
			Assert.AreEqual(0, textDocument.LineTrackers.Count);

			TextView textView = new TextView();
			WeakReference wr = new WeakReference(textView);
			textView.Document = textDocument;
			Assert.AreEqual(1, textDocument.LineTrackers.Count);
			textView = null;

			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
			// document cannot immediately clear the line tracker
			Assert.AreEqual(1, textDocument.LineTrackers.Count);

			// but it should clear it on the next change
			textDocument.Insert(0, "a");
			Assert.AreEqual(0, textDocument.LineTrackers.Count);
		}

		[Test]
		public void DocumentDoesNotHoldReferenceToTextArea()
		{
			TextDocument textDocument = new TextDocument();

			TextArea textArea = new TextArea();
			WeakReference wr = new WeakReference(textArea);
			textArea.Document = textDocument;
			textArea = null;

			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
			GC.KeepAlive(textDocument);
		}

		[Test]
		public void DocumentDoesNotHoldReferenceToTextEditor()
		{
			TextDocument textDocument = new TextDocument();

			TextEditor textEditor = new TextEditor();
			WeakReference wr = new WeakReference(textEditor);
			textEditor.Document = textDocument;
			textEditor = null;

			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
			GC.KeepAlive(textDocument);
		}

		[Test]
		public void DocumentDoesNotHoldReferenceToLineMargin()
		{
			TextDocument textDocument = new TextDocument();

			WeakReference wr = DocumentDoesNotHoldReferenceToLineMargin_CreateMargin(textDocument);

			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
			GC.KeepAlive(textDocument);
		}

		// using a method to ensure the local variables can be garbage collected after the method returns
		private WeakReference DocumentDoesNotHoldReferenceToLineMargin_CreateMargin(TextDocument textDocument)
		{
			TextView textView = new TextView()
			{
				Document = textDocument
			};
			LineNumberMargin margin = new LineNumberMargin()
			{
				TextView = textView
			};
			return new WeakReference(textView);
		}

		private static void GarbageCollect()
		{
			for (int i = 0; i < 3; i++)
			{
				GC.WaitForPendingFinalizers();
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				// pump WPF messages so that WeakEventManager can unregister
				Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
			}
		}
	}
}