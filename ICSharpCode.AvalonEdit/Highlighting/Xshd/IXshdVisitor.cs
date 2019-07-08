// SPDX-License-Identifier: MIT

namespace ICSharpCode.AvalonEdit.Highlighting.Xshd
{
	/// <summary>
	/// A visitor over the XSHD element tree.
	/// </summary>
	public interface IXshdVisitor
	{
		/// <summary>Visit method for XshdRuleSet</summary>
		object VisitRuleSet(XshdRuleSet ruleSet);

		/// <summary>Visit method for XshdColor</summary>
		object VisitColor(XshdColor color);

		/// <summary>Visit method for XshdKeywords</summary>
		object VisitKeywords(XshdKeywords keywords);

		/// <summary>Visit method for XshdSpan</summary>
		object VisitSpan(XshdSpan span);

		/// <summary>Visit method for XshdImport</summary>
		object VisitImport(XshdImport import);

		/// <summary>Visit method for XshdRule</summary>
		object VisitRule(XshdRule rule);
	}
}
