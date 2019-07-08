// SPDX-License-Identifier: MIT

using System;
using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Document
{
	public class UndoStackTests
	{
		[Test]
		public void ContinueUndoGroup()
		{
			var doc = new TextDocument();
			doc.Insert(0, "a");
			doc.UndoStack.StartContinuedUndoGroup();
			doc.Insert(1, "b");
			doc.UndoStack.EndUndoGroup();
			doc.UndoStack.Undo();
			Assert.AreEqual("", doc.Text);
		}
		
		[Test]
		public void ContinueEmptyUndoGroup()
		{
			var doc = new TextDocument();
			doc.Insert(0, "a");
			doc.UndoStack.StartUndoGroup();
			doc.UndoStack.EndUndoGroup();
			doc.UndoStack.StartContinuedUndoGroup();
			doc.Insert(1, "b");
			doc.UndoStack.EndUndoGroup();
			doc.UndoStack.Undo();
			Assert.AreEqual("a", doc.Text);
		}
		
		[Test]
		public void ContinueEmptyUndoGroup_WithOptionalEntries()
		{
			var doc = new TextDocument();
			doc.Insert(0, "a");
			doc.UndoStack.StartUndoGroup();
			doc.UndoStack.PushOptional(new StubUndoableAction());
			doc.UndoStack.EndUndoGroup();
			doc.UndoStack.StartContinuedUndoGroup();
			doc.Insert(1, "b");
			doc.UndoStack.EndUndoGroup();
			doc.UndoStack.Undo();
			Assert.AreEqual("a", doc.Text);
		}
		
		[Test]
		public void EmptyContinuationGroup()
		{
			var doc = new TextDocument();
			doc.Insert(0, "a");
			doc.UndoStack.StartContinuedUndoGroup();
			doc.UndoStack.EndUndoGroup();
			doc.UndoStack.StartContinuedUndoGroup();
			doc.Insert(1, "b");
			doc.UndoStack.EndUndoGroup();
			doc.UndoStack.Undo();
			Assert.AreEqual("", doc.Text);
		}
		
		class StubUndoableAction : IUndoableOperation
		{
			public void Undo()
			{
			}
			
			public void Redo()
			{
			}
		}
	}
}
