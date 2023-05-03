using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

using NUnit.Framework;

namespace ICSharpCode.AvalonEdit.Tests.Rendering
{
	[TestFixture]
	[Apartment(System.Threading.ApartmentState.STA)]
	public class LinkElementGeneratorTests
	{
		[Test]
		public void ParsingOfCustomLinkRegexes()
		{
			var linkElementGenerator = new LinkElementGenerator();
			const string link = "example://mylink";
			const string input = "Visit " + link + " please";
			var textSource = GetTextSource(input);
			linkElementGenerator.StartGeneration(textSource);

			var interestedOffsetBeforeCustomRegex = linkElementGenerator.GetFirstInterestedOffset(0);
			Assert.That(interestedOffsetBeforeCustomRegex, Is.EqualTo(-1));

			var optionsWithCustomLink = new TextEditorOptions {
				// Check for example:// protocol instead of http:// protocol.
				LinkRegex = @"\bexample://\w+\b",
			};
			((IBuiltinElementGenerator)linkElementGenerator).FetchOptions(optionsWithCustomLink);

			var interestedOffsetAfterCustomRegex = linkElementGenerator.GetFirstInterestedOffset(0);
			var expectedOffset = input.IndexOf(link);
			Assert.That(interestedOffsetAfterCustomRegex, Is.EqualTo(expectedOffset));

			linkElementGenerator.FinishGeneration();
		}

		[Test]
		public void ParsingOfCustomMailRegexes()
		{
			var linkElementGenerator = new MailLinkElementGenerator();
			const string link = "example@example.verylongtld";
			const string input = "Email " + link + " please";
			var textSource = GetTextSource(input);
			linkElementGenerator.StartGeneration(textSource);

			var interestedOffsetBeforeCustomRegex = linkElementGenerator.GetFirstInterestedOffset(0);
			Assert.That(interestedOffsetBeforeCustomRegex, Is.EqualTo(-1));

			var optionsWithCustomLink = new TextEditorOptions {
				// The same as the default except that TLDs can be up to 12 chars now.
				MailRegex = @"\b[\w\d\.\-]+\@[\w\d\.\-]+\.[a-z]{2,12}\b",
			};
			((IBuiltinElementGenerator)linkElementGenerator).FetchOptions(optionsWithCustomLink);

			var interestedOffsetAfterCustomRegex = linkElementGenerator.GetFirstInterestedOffset(0);
			var expectedOffset = input.IndexOf(link);
			Assert.That(interestedOffsetAfterCustomRegex, Is.EqualTo(expectedOffset));

			linkElementGenerator.FinishGeneration();
		}

		private static VisualLineTextSource GetTextSource(string input)
		{
			var textDocument = new TextDocument(input);
			var textView = new TextView {
				Document = textDocument,
			};
			var documentLine = textDocument.Lines[0];
			var visualLine = textView.GetOrConstructVisualLine(documentLine);
			var textSource = new VisualLineTextSource(visualLine) {
				Document = textDocument,
				TextView = textView,
			};
			return textSource;
		}
	}
}
