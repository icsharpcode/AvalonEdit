// SPDX-License-Identifier: MIT

namespace ICSharpCode.AvalonEdit.Editing
{
	/// <summary>
	/// Enumeration of possible states of mouse selection.
	/// </summary>
	public enum MouseSelectionMode
	{
		/// <summary>
		/// no selection (no mouse button down)
		/// </summary>
		None,
		/// <summary>
		/// left mouse button down on selection, might be normal click
		/// or might be drag'n'drop
		/// </summary>
		PossibleDragStart,
		/// <summary>
		/// dragging text
		/// </summary>
		Drag,
		/// <summary>
		/// normal selection (click+drag)
		/// </summary>
		Normal,
		/// <summary>
		/// whole-word selection (double click+drag or ctrl+click+drag)
		/// </summary>
		WholeWord,
		/// <summary>
		/// whole-line selection (triple click+drag)
		/// </summary>
		WholeLine,
		/// <summary>
		/// rectangular selection (alt+click+drag)
		/// </summary>
		Rectangular
	}
}
