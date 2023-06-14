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
using System.Linq;
using System.Xml;

namespace AcAvalonEdit.Highlighting.Xshd
{
	/// <summary>
	/// Xshd visitor implementation that saves an .xshd file as XML.
	/// </summary>
	public sealed class SaveXshdVisitor : IXshdVisitor
	{
		/// <summary>
		/// XML namespace for XSHD.
		/// </summary>
		public const string Namespace = V2Loader.Namespace;

		XmlWriter writer;

		/// <summary>
		/// Creates a new SaveXshdVisitor instance.
		/// </summary>
		public SaveXshdVisitor(XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			this.writer = writer;
		}

		/// <summary>
		/// Writes the specified syntax definition.
		/// </summary>
		public void WriteDefinition(XshdSyntaxDefinition definition)
		{
			if (definition == null)
				throw new ArgumentNullException("definition");
			writer.WriteStartElement("SyntaxDefinition", Namespace);
			if (definition.Name != null)
				writer.WriteAttributeString("name", definition.Name);
			if (definition.Extensions != null)
				writer.WriteAttributeString("extensions", string.Join(";", definition.Extensions.ToArray()));

			definition.AcceptElements(this);

			writer.WriteEndElement();
		}

		object IXshdVisitor.VisitRuleSet(XshdRuleSet ruleSet)
		{
			writer.WriteStartElement("RuleSet", Namespace);

			if (ruleSet.Name != null)
				writer.WriteAttributeString("name", ruleSet.Name);
			WriteBoolAttribute("ignoreCase", ruleSet.IgnoreCase);

			ruleSet.AcceptElements(this);

			writer.WriteEndElement();
			return null;
		}

		void WriteBoolAttribute(string attributeName, bool? value)
		{
			if (value != null) {
				writer.WriteAttributeString(attributeName, value.Value ? "true" : "false");
			}
		}

		void WriteRuleSetReference(XshdReference<XshdRuleSet> ruleSetReference)
		{
			if (ruleSetReference.ReferencedElement != null) {
				if (ruleSetReference.ReferencedDefinition != null)
					writer.WriteAttributeString("ruleSet", ruleSetReference.ReferencedDefinition + "/" + ruleSetReference.ReferencedElement);
				else
					writer.WriteAttributeString("ruleSet", ruleSetReference.ReferencedElement);
			}
		}

		void WriteColorReference(XshdReference<XshdColor> color)
		{
			if (color.InlineElement != null) {
				WriteColorAttributes(color.InlineElement);
			} else if (color.ReferencedElement != null) {
				if (color.ReferencedDefinition != null)
					writer.WriteAttributeString("color", color.ReferencedDefinition + "/" + color.ReferencedElement);
				else
					writer.WriteAttributeString("color", color.ReferencedElement);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "The file format requires lowercase, and all possible values are English-only")]
		void WriteColorAttributes(XshdColor color)
		{
			if (color.Foreground != null)
				writer.WriteAttributeString("foreground", color.Foreground.ToString());
			if (color.Background != null)
				writer.WriteAttributeString("background", color.Background.ToString());
			if (color.FontWeight != null)
				writer.WriteAttributeString("fontWeight", V2Loader.FontWeightConverter.ConvertToInvariantString(color.FontWeight.Value).ToLowerInvariant());
			if (color.FontStyle != null)
				writer.WriteAttributeString("fontStyle", V2Loader.FontStyleConverter.ConvertToInvariantString(color.FontStyle.Value).ToLowerInvariant());
		}

		object IXshdVisitor.VisitColor(XshdColor color)
		{
			writer.WriteStartElement("Color", Namespace);
			if (color.Name != null)
				writer.WriteAttributeString("name", color.Name);
			WriteColorAttributes(color);
			if (color.ExampleText != null)
				writer.WriteAttributeString("exampleText", color.ExampleText);
			writer.WriteEndElement();
			return null;
		}

		object IXshdVisitor.VisitKeywords(XshdKeywords keywords)
		{
			writer.WriteStartElement("Keywords", Namespace);
			WriteColorReference(keywords.ColorReference);
			foreach (string word in keywords.Words) {
				writer.WriteElementString("Word", Namespace, word);
			}
			writer.WriteEndElement();
			return null;
		}

		object IXshdVisitor.VisitSpan(XshdSpan span)
		{
			writer.WriteStartElement("Span", Namespace);
			WriteColorReference(span.SpanColorReference);
			if (span.BeginRegexType == XshdRegexType.Default && span.BeginRegex != null)
				writer.WriteAttributeString("begin", span.BeginRegex);
			if (span.EndRegexType == XshdRegexType.Default && span.EndRegex != null)
				writer.WriteAttributeString("end", span.EndRegex);
			WriteRuleSetReference(span.RuleSetReference);
			if (span.Multiline)
				writer.WriteAttributeString("multiline", "true");

			if (span.BeginRegexType == XshdRegexType.IgnorePatternWhitespace)
				WriteBeginEndElement("Begin", span.BeginRegex, span.BeginColorReference);
			if (span.EndRegexType == XshdRegexType.IgnorePatternWhitespace)
				WriteBeginEndElement("End", span.EndRegex, span.EndColorReference);

			if (span.RuleSetReference.InlineElement != null)
				span.RuleSetReference.InlineElement.AcceptVisitor(this);

			writer.WriteEndElement();
			return null;
		}

		void WriteBeginEndElement(string elementName, string regex, XshdReference<XshdColor> colorReference)
		{
			if (regex != null) {
				writer.WriteStartElement(elementName, Namespace);
				WriteColorReference(colorReference);
				writer.WriteString(regex);
				writer.WriteEndElement();
			}
		}

		object IXshdVisitor.VisitImport(XshdImport import)
		{
			writer.WriteStartElement("Import", Namespace);
			WriteRuleSetReference(import.RuleSetReference);
			writer.WriteEndElement();
			return null;
		}

		object IXshdVisitor.VisitRule(XshdRule rule)
		{
			writer.WriteStartElement("Rule", Namespace);
			WriteColorReference(rule.ColorReference);

			writer.WriteString(rule.Regex);

			writer.WriteEndElement();
			return null;
		}
	}
}
