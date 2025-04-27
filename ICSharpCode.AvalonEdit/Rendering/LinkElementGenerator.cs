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

using System;
using System.Text.RegularExpressions;

using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Rendering
{
	// This class is public because it can be used as a base class for custom links.

	/// <summary>
	/// Detects hyperlinks and makes them clickable.
	/// </summary>
	/// <remarks>
	/// This element generator can be easily enabled and configured using the
	/// <see cref="TextEditorOptions"/>.
	/// </remarks>
	public class LinkElementGenerator : VisualLineElementGenerator, IBuiltinElementGenerator
	{
		// a link starts with a protocol (or just with www), followed by 0 or more 'link characters', followed by a link end character
		// (this allows accepting punctuation inside links but not at the end)
		internal readonly static Regex defaultLinkRegex = new Regex(@"\b(https?://|ftp://|www\.)[\w\d\._/\-~%@()+:?&=#!]*[\w\d/]");

		// try to detect email addresses
		internal readonly static Regex defaultMailRegex = new Regex(@"\b[\w\d\.\-]+\@[\w\d\.\-]+\.[a-z]{2,6}\b");

		/// <summary>
		/// The regex used to parse the type of link this class represents.
		/// </summary>
		protected Regex linkRegex;

		/// <summary>
		/// Gets/Sets whether the user needs to press Control to click the link.
		/// The default value is true.
		/// </summary>
		public bool RequireControlModifierForClick { get; set; }

		/// <summary>
		/// Creates a new LinkElementGenerator.
		/// </summary>
		public LinkElementGenerator()
		{
			this.linkRegex = defaultLinkRegex;
			this.RequireControlModifierForClick = true;
		}

		/// <summary>
		/// Creates a new LinkElementGenerator using the specified regex.
		/// </summary>
		protected LinkElementGenerator(Regex regex) : this()
		{
			if (regex == null)
				throw new ArgumentNullException("regex");
			this.linkRegex = regex;
		}

		void IBuiltinElementGenerator.FetchOptions(TextEditorOptions options)
		{
			this.RequireControlModifierForClick = options.RequireControlModifierForHyperlinkClick;
			this.linkRegex = new Regex(options.LinkRegex);
		}

		Match GetMatch(int startOffset, out int matchOffset)
		{
			int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
			StringSegment relevantText = CurrentContext.GetText(startOffset, endOffset - startOffset);
			Match m = linkRegex.Match(relevantText.Text, relevantText.Offset, relevantText.Count);
			matchOffset = m.Success ? m.Index - relevantText.Offset + startOffset : -1;
			return m;
		}

		/// <inheritdoc/>
		public override int GetFirstInterestedOffset(int startOffset)
		{
			int matchOffset;
			GetMatch(startOffset, out matchOffset);
			return matchOffset;
		}

		/// <inheritdoc/>
		public override VisualLineElement ConstructElement(int offset)
		{
			int matchOffset;
			Match m = GetMatch(offset, out matchOffset);
			if (m.Success && matchOffset == offset) {
				return ConstructElementFromMatch(m);
			} else {
				return null;
			}
		}

		/// <summary>
		/// Constructs a VisualLineElement that replaces the matched text.
		/// The default implementation will create a <see cref="VisualLineLinkText"/>
		/// based on the URI provided by <see cref="GetUriFromMatch"/>.
		/// </summary>
		protected virtual VisualLineElement ConstructElementFromMatch(Match m)
		{
			Uri uri = GetUriFromMatch(m);
			if (uri == null)
				return null;
			VisualLineLinkText linkText = new VisualLineLinkText(CurrentContext.VisualLine, m.Length);
			linkText.NavigateUri = uri;
			linkText.RequireControlModifierForClick = this.RequireControlModifierForClick;
			return linkText;
		}

		/// <summary>
		/// Fetches the URI from the regex match. Returns null if the URI format is invalid.
		/// </summary>
		protected virtual Uri GetUriFromMatch(Match match)
		{
			string targetUrl = match.Value;
			if (targetUrl.StartsWith("www.", StringComparison.Ordinal))
				targetUrl = "http://" + targetUrl;
			if (Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute))
				return new Uri(targetUrl);

			return null;
		}
	}

	// This class is internal because it does not need to be accessed by the user - it can be configured using TextEditorOptions.

	/// <summary>
	/// Detects e-mail addresses and makes them clickable.
	/// </summary>
	/// <remarks>
	/// This element generator can be easily enabled and configured using the
	/// <see cref="TextEditorOptions"/>.
	/// </remarks>
	sealed class MailLinkElementGenerator : LinkElementGenerator, IBuiltinElementGenerator
	{
		/// <summary>
		/// Creates a new MailLinkElementGenerator.
		/// </summary>
		public MailLinkElementGenerator()
			: base(defaultMailRegex)
		{
		}

		void IBuiltinElementGenerator.FetchOptions(TextEditorOptions options)
		{
			this.RequireControlModifierForClick = options.RequireControlModifierForHyperlinkClick;
			this.linkRegex = new Regex(options.MailRegex);
		}

		protected override Uri GetUriFromMatch(Match match)
		{
			var targetUrl = "mailto:" + match.Value;
			if (Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute))
				return new Uri(targetUrl);

			return null;
		}
	}
}
