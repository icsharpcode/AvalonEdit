using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json;
using NUnit.Framework;


namespace ICSharpCode.AvalonEdit.Tests.Highlighting
{
	[TestFixture]
	public class DeserializationTests
	{
		TextDocument document;
		DocumentHighlighter highlighter;

		[SetUp]
		public void SetUp()
		{
			document = new TextDocument("using System.Text;\n\tstring text = SomeMethod();");
			highlighter = new DocumentHighlighter(document, HighlightingManager.Instance.GetDefinition("C#"));
		}

		[Test]
		public void TestRoundTripColor()
		{
			HighlightingColor color = highlighter.GetNamedColor("Comment");
			string jsonString = JsonConvert.SerializeObject(color);

			HighlightingColor color2 = JsonConvert.DeserializeObject<HighlightingColor>(jsonString);
			Assert.AreEqual(color, color2);
		}
	}
}
