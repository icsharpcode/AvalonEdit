using System.IO;
using System.Xml;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

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

		[TestCase("CSharp-Mode.xshd")]
		public void XshdSerializationDoesNotCrash(string resourceName)
		{
			XshdSyntaxDefinition xshd;
			using (Stream s = Resources.OpenStream(resourceName)) {
				using (XmlTextReader reader = new XmlTextReader(s)) {
					xshd = HighlightingLoader.LoadXshd(reader, false);
				}
			}
			Assert.AreEqual("C#", xshd.Name);
			Assert.IsNotEmpty(xshd.Extensions);
			Assert.AreEqual(".cs", xshd.Extensions[0]);

			Assert.DoesNotThrow(() => JsonConvert.SerializeObject(xshd));
		}
	}
}
