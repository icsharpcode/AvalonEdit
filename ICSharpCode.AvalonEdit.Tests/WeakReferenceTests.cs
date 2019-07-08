// SPDX-License-Identifier: MIT

using System;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using NUnit.Framework;

namespace ICSharpCode.AvalonEdit
{
	[TestFixture]
	public class WeakReferenceTests
	{
		[Test]
		public void TextViewCanBeCollectedTest()
		{
			WeakReference wr = TextViewCanBeCollectedTest_CreateTextView();
			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
		}

		// Use separate no-inline method so that the JIT can't keep a strong
		// reference to the text view alive past this method.
		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference TextViewCanBeCollectedTest_CreateTextView()
		{
			return new WeakReference(new TextView());
		}

		[Test]
		public void DocumentDoesNotHoldReferenceToTextView()
		{
			TextDocument textDocument = new TextDocument();
			Assert.AreEqual(0, textDocument.LineTrackers.Count);
			
			WeakReference wr = DocumentDoesNotHoldReferenceToTextView_CreateTextView(textDocument);
			Assert.AreEqual(1, textDocument.LineTrackers.Count);
			
			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
			// document cannot immediately clear the line tracker
			Assert.AreEqual(1, textDocument.LineTrackers.Count);
			
			// but it should clear it on the next change
			textDocument.Insert(0, "a");
			Assert.AreEqual(0, textDocument.LineTrackers.Count);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference DocumentDoesNotHoldReferenceToTextView_CreateTextView(TextDocument textDocument)
		{
			TextView textView = new TextView();
			textView.Document = textDocument;
			return new WeakReference(textView);
		}

		[Test]
		public void DocumentDoesNotHoldReferenceToTextArea()
		{
			TextDocument textDocument = new TextDocument();
			WeakReference wr = DocumentDoesNotHoldReferenceToTextArea_CreateTextArea(textDocument);
			
			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
			GC.KeepAlive(textDocument);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference DocumentDoesNotHoldReferenceToTextArea_CreateTextArea(TextDocument textDocument)
		{
			TextArea textArea = new TextArea();
			textArea.Document = textDocument;
			return new WeakReference(textArea);
		}

		[Test]
		public void DocumentDoesNotHoldReferenceToTextEditor()
		{
			TextDocument textDocument = new TextDocument();
			WeakReference wr = DocumentDoesNotHoldReferenceToTextEditor_CreateTextEditor(textDocument);
			
			GarbageCollect();
			Assert.IsFalse(wr.IsAlive);
			GC.KeepAlive(textDocument);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference DocumentDoesNotHoldReferenceToTextEditor_CreateTextEditor(TextDocument textDocument)
		{
			TextEditor textEditor = new TextEditor();
			textEditor.Document = textDocument;
			return new WeakReference(textEditor);
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
		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference DocumentDoesNotHoldReferenceToLineMargin_CreateMargin(TextDocument textDocument)
		{
			TextView textView = new TextView() {
				Document = textDocument
			};
			LineNumberMargin margin = new LineNumberMargin() {
				TextView = textView
			};
			return new WeakReference(textView);
		}
		
		static void GarbageCollect()
		{
			for (int i = 0; i < 3; i++) {
				GC.WaitForPendingFinalizers();
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				// pump WPF messages so that WeakEventManager can unregister
				Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {}));
			}
		}
	}
}
