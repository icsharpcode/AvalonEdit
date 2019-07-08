// SPDX-License-Identifier: MIT

using System;
using System.Windows.Documents;

namespace ICSharpCode.AvalonEdit.Snippets
{
	/// <summary>
	/// An element inside a snippet.
	/// </summary>
	[Serializable]
	public abstract class SnippetElement
	{
		/// <summary>
		/// Performs insertion of the snippet.
		/// </summary>
		public abstract void Insert(InsertionContext context);

		/// <summary>
		/// Converts the snippet to text, with replaceable fields in italic.
		/// </summary>
		public virtual Inline ToTextRun()
		{
			return null;
		}
	}
}
