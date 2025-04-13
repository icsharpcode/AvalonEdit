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

namespace ICSharpCode.AvalonEdit.Highlighting
{
	/// <summary>
	/// Interface of a highlighting rule
	/// </summary>
	public interface IHighlightingRule
	{
		/// <summary>
		/// Gets the first match for the rule
		/// </summary>
		/// <param name="input">The string to search for a match.</param>
		/// <param name="beginning">The zero-based character position in the input string that defines the leftmost 
		/// position to be searched.</param>
		/// <param name="length">The number of characters in the substring to include in the search.</param>
		/// <param name="lineNumber">The line number of the <paramref name="input"/> string.</param>
		/// <returns>An object that contains information about the match.</returns>
		RuleMatch GetMatch(string input, int beginning, int length, int lineNumber);

		/// <summary>
		/// Gets the highlighting color.
		/// </summary>
		HighlightingColor Color { get; }

		/// <summary>
		/// Info about rule. Used to help figure out why rule failed
		/// </summary>
		string RuleInfo { get; }
	}
}
