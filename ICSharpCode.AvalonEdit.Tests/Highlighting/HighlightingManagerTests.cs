using System;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Tests.Highlighting
{
	[TestFixture]
	public class HighlightingManagerTests
	{
		[Test]
		public void OverwriteHighlightingDefinitionWithSameName()
		{
			var highlightingManager = new HighlightingManager();

			var definitionA = CreateDefinition("TestDefinition");
			var definitionB = CreateDefinition("TestDefinition");
			var definitionC = CreateDefinition("DifferentName");

			Assert.That(highlightingManager.HighlightingDefinitions, Is.Empty);

			highlightingManager.RegisterHighlighting(definitionA.Name, Array.Empty<string>(), definitionA);
			Assert.That(highlightingManager.HighlightingDefinitions, Is.EqualTo(new[] { definitionA }));

			highlightingManager.RegisterHighlighting(definitionB.Name, Array.Empty<string>(), definitionB);
			Assert.That(highlightingManager.HighlightingDefinitions, Is.EqualTo(new[] { definitionB }));

			highlightingManager.RegisterHighlighting(definitionC.Name, Array.Empty<string>(), definitionC);
			Assert.That(highlightingManager.HighlightingDefinitions, Is.EqualTo(new[] { definitionB, definitionC }));

			XmlHighlightingDefinition CreateDefinition(string name)
			{
				return new XmlHighlightingDefinition(new XshdSyntaxDefinition { Name = name, Elements = { new XshdRuleSet() } }, highlightingManager);
			}
		}
	}
}
