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
using System.Xml;
using System.Xml.Schema;

namespace ICSharpCode.AvalonEdit.Highlighting.Xshd
{
	/// <summary>
	/// Static class with helper methods to load XSHD highlighting files.
	/// </summary>
	public static class HighlightingLoader
	{
		#region XSHD loading

		/// <summary>
		/// Lodas a syntax definition from the xml reader.
		/// </summary>
		public static XshdSyntaxDefinition LoadXshd(XmlReader reader)
		{
			return LoadXshd(reader, false);
		}

		internal static XshdSyntaxDefinition LoadXshd(XmlReader reader, bool skipValidation)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			try
			{
				reader.MoveToContent();
				if (reader.NamespaceURI == V2Loader.Namespace)
				{
					return V2Loader.LoadDefinition(reader, skipValidation);
				}
				else
				{
					return V1Loader.LoadDefinition(reader, skipValidation);
				}
			}
			catch (XmlSchemaException ex)
			{
				throw WrapException(ex, ex.LineNumber, ex.LinePosition);
			}
			catch (XmlException ex)
			{
				throw WrapException(ex, ex.LineNumber, ex.LinePosition);
			}
		}

		private static Exception WrapException(Exception ex, int lineNumber, int linePosition)
		{
			return new HighlightingDefinitionInvalidException(FormatExceptionMessage(ex.Message, lineNumber, linePosition), ex);
		}

		internal static string FormatExceptionMessage(string message, int lineNumber, int linePosition)
		{
			if (lineNumber <= 0)
				return message;
			else
				return "Error at position (line " + lineNumber + ", column " + linePosition + "):\n" + message;
		}

		internal static XmlReader GetValidatingReader(XmlReader input, bool ignoreWhitespace, XmlSchemaSet schemaSet)
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.CloseInput = true;
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = ignoreWhitespace;
			if (schemaSet != null)
			{
				settings.Schemas = schemaSet;
				settings.ValidationType = ValidationType.Schema;
			}
			return XmlReader.Create(input, settings);
		}

		internal static XmlSchemaSet LoadSchemaSet(XmlReader schemaInput)
		{
			XmlSchemaSet schemaSet = new XmlSchemaSet();
			schemaSet.Add(null, schemaInput);
			schemaSet.ValidationEventHandler += delegate (object sender, ValidationEventArgs args)
			{
				throw new HighlightingDefinitionInvalidException(args.Message);
			};
			return schemaSet;
		}

		#endregion XSHD loading

		#region Load Highlighting from XSHD

		/// <summary>
		/// Creates a highlighting definition from the XSHD file.
		/// </summary>
		public static IHighlightingDefinition Load(XshdSyntaxDefinition syntaxDefinition, IHighlightingDefinitionReferenceResolver resolver)
		{
			if (syntaxDefinition == null)
				throw new ArgumentNullException("syntaxDefinition");
			return new XmlHighlightingDefinition(syntaxDefinition, resolver);
		}

		/// <summary>
		/// Creates a highlighting definition from the XSHD file.
		/// </summary>
		public static IHighlightingDefinition Load(XmlReader reader, IHighlightingDefinitionReferenceResolver resolver)
		{
			return Load(LoadXshd(reader), resolver);
		}

		#endregion Load Highlighting from XSHD
	}
}