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

using System.Text.RegularExpressions;

namespace ICSharpCode.AvalonEdit.Highlighting
{
	/// <summary>
	/// And object that contains information about a rule's match
	/// </summary>
	public class RuleMatch
	{
		/// <summary>
		/// Creates a new RuleMatch instance.
		/// </summary>
		public RuleMatch() { }

		/// <summary>
		/// Gets a value indicating whether the match was successful.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The position in the original string where the first character of captured substring was found.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// The length of the captured substring.
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// Creates a new RuleMatch instance from a <see cref="Match"/> instance.
		/// </summary>
		/// <param name="match">Match to use</param>
		/// <returns>RuleMatch instance built from match parameter</returns>
		public static RuleMatch FromRegexMatch(Match match)
		{
			return new RuleMatch() {
				Success = match.Success,
				Index = match.Index,
				Length = match.Length,
			};
		}
	}
}
