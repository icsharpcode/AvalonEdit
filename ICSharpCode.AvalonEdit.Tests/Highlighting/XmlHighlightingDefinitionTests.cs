using System.Linq;

using AcAvalonEdit.Document;
using AcAvalonEdit.Highlighting;
using AcAvalonEdit.Highlighting.Xshd;

using NUnit.Framework;

namespace AcAvalonEdit.Tests.Highlighting
{
	[TestFixture]
	public class XmlHighlightingDefinitionTests
	{
		[Test]
		public void LongerKeywordsArePreferred()
		{
			var color = new XshdColor { Name = "Result" };
			var syntaxDefinition = new XshdSyntaxDefinition {
				Elements = {
					color,
					new XshdRuleSet {
						Elements = { new XshdKeywords {
								ColorReference = new XshdReference<XshdColor>(null, color.Name),
								Words = { "foo", "foo.bar." }
							}
						}
					}
				}
			};

			var document = new TextDocument("This is a foo.bar. keyword");
			var highlighter = new DocumentHighlighter(document, new XmlHighlightingDefinition(syntaxDefinition, HighlightingManager.Instance));
			var result = highlighter.HighlightLine(1);

			var highlightedText = document.GetText(result.Sections.Single());
			Assert.That(highlightedText, Is.EqualTo("foo.bar."));
		}
	}
}
