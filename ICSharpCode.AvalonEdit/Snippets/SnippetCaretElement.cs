// SPDX-License-Identifier: MIT

using System;
using System.Runtime.Serialization;

using ICSharpCode.AvalonEdit.Document;

namespace ICSharpCode.AvalonEdit.Snippets
{
	/// <summary>
	/// Sets the caret position after interactive mode has finished.
	/// </summary>
	[Serializable]
	public class SnippetCaretElement : SnippetElement
	{
		[OptionalField]
		bool setCaretOnlyIfTextIsSelected;

		/// <summary>
		/// Creates a new SnippetCaretElement.
		/// </summary>
		public SnippetCaretElement()
		{
		}

		/// <summary>
		/// Creates a new SnippetCaretElement.
		/// </summary>
		/// <param name="setCaretOnlyIfTextIsSelected">
		/// If set to true, the caret is set only when some text was selected.
		/// This is useful when both SnippetCaretElement and SnippetSelectionElement are used in the same snippet.
		/// </param>
		public SnippetCaretElement(bool setCaretOnlyIfTextIsSelected)
		{
			this.setCaretOnlyIfTextIsSelected = setCaretOnlyIfTextIsSelected;
		}

		/// <inheritdoc/>
		public override void Insert(InsertionContext context)
		{
			if (!setCaretOnlyIfTextIsSelected || !string.IsNullOrEmpty(context.SelectedText))
				SetCaret(context);
		}

		internal static void SetCaret(InsertionContext context)
		{
			TextAnchor pos = context.Document.CreateAnchor(context.InsertionPosition);
			pos.MovementType = AnchorMovementType.BeforeInsertion;
			pos.SurviveDeletion = true;
			context.Deactivated += (sender, e) => {
				if (e.Reason == DeactivateReason.ReturnPressed || e.Reason == DeactivateReason.NoActiveElements) {
					context.TextArea.Caret.Offset = pos.Offset;
				}
			};
		}
	}
}
