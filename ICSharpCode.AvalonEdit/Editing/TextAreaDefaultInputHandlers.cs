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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ICSharpCode.AvalonEdit.Editing
{
	/// <summary>
	/// Contains the predefined input handlers.
	/// </summary>
	public class TextAreaDefaultInputHandler : TextAreaInputHandler
	{
		/// <summary>
		/// Gets the caret navigation input handler.
		/// </summary>
		public TextAreaInputHandler CaretNavigation { get; private set; }

		/// <summary>
		/// Gets the editing input handler.
		/// </summary>
		public TextAreaInputHandler Editing { get; private set; }

		/// <summary>
		/// Gets the mouse selection input handler.
		/// </summary>
		public ITextAreaInputHandler MouseSelection { get; private set; }

		/// <summary>
		/// Creates a new TextAreaDefaultInputHandler instance.
		/// </summary>
		public TextAreaDefaultInputHandler(TextArea textArea) : base(textArea)
		{
			this.NestedInputHandlers.Add(CaretNavigation = CaretNavigationCommandHandler.Create(textArea));
			this.NestedInputHandlers.Add(Editing = EditingCommandHandler.Create(textArea));
			this.NestedInputHandlers.Add(MouseSelection = new SelectionMouseHandler(textArea));

			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, ExecuteUndo, CanExecuteUndo));
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, ExecuteRedo, CanExecuteRedo));
		}

		internal static KeyBinding CreateFrozenKeyBinding(ICommand command, ModifierKeys modifiers, Key key)
		{
			KeyBinding kb = new KeyBinding(command, key, modifiers);
			// Mark KeyBindings as frozen because they're shared between multiple editor instances.
			// KeyBinding derives from Freezable only in .NET 4, so we have to use this little trick:
			Freezable f = ((object)kb) as Freezable;
			if (f != null)
				f.Freeze();
			return kb;
		}

		internal static void WorkaroundWPFMemoryLeak(List<InputBinding> inputBindings)
		{
			// Work around WPF memory leak:
			// KeyBinding retains a reference to whichever UIElement it is used in first.
			// Using a dummy element for this purpose ensures that we don't leak
			// a real text editor (with a potentially large document).
			UIElement dummyElement = new UIElement();
			dummyElement.InputBindings.AddRange(inputBindings);
		}

		#region Undo / Redo

		private UndoStack GetUndoStack()
		{
			TextDocument document = this.TextArea.Document;
			if (document != null)
				return document.UndoStack;
			else
				return null;
		}

		private void ExecuteUndo(object sender, ExecutedRoutedEventArgs e)
		{
			var undoStack = GetUndoStack();
			if (undoStack != null)
			{
				if (undoStack.CanUndo)
				{
					undoStack.Undo();
					this.TextArea.Caret.BringCaretToView();
				}
				e.Handled = true;
			}
		}

		private void CanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
		{
			var undoStack = GetUndoStack();
			if (undoStack != null)
			{
				e.Handled = true;
				e.CanExecute = undoStack.CanUndo;
			}
		}

		private void ExecuteRedo(object sender, ExecutedRoutedEventArgs e)
		{
			var undoStack = GetUndoStack();
			if (undoStack != null)
			{
				if (undoStack.CanRedo)
				{
					undoStack.Redo();
					this.TextArea.Caret.BringCaretToView();
				}
				e.Handled = true;
			}
		}

		private void CanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
		{
			var undoStack = GetUndoStack();
			if (undoStack != null)
			{
				e.Handled = true;
				e.CanExecute = undoStack.CanRedo;
			}
		}

		#endregion Undo / Redo
	}
}