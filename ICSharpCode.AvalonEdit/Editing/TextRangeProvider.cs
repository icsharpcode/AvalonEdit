using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Utils;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit.Document;
using System.Windows.Automation.Text;


namespace ICSharpCode.AvalonEdit.Editing
{
	class TextRangeProvider : ITextRangeProvider
	{
		private TextDocument doc;
		private int[] endPoints = new int[2];
		public TextRangeProvider(TextDocument doc, int start, int end)
		{
			this.doc = doc;
			endPoints[0] = start;
			endPoints[1] = end;
		}
		
		public void AddToSelection()
		{
		}

		public ITextRangeProvider Clone()
		{
			return new TextRangeProvider(doc, endPoints[0], endPoints[1]);
		}

		public bool Compare(ITextRangeProvider range)
		{
			TextRangeProvider other = (TextRangeProvider) range;
			return (doc == other.doc &&
				endPoints[0] == other.endPoints[0] &&
				endPoints[1] == other.endPoints[1]);
		}

		public int CompareEndpoints(System.Windows.Automation.Text.TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, System.Windows.Automation.Text.TextPatternRangeEndpoint targetEndpoint)
		{
			TextRangeProvider other = (TextRangeProvider) targetRange;
			return  endPoints[(int) endpoint].CompareTo(other.endPoints[(int) targetEndpoint]);
		}

		public void ExpandToEnclosingUnit(System.Windows.Automation.Text.TextUnit unit)
		{
			switch (unit)
			{
				case TextUnit.Character:
					endPoints[1] = endPoints[0]+1;
					break;
					case TextUnit.Word:
					{
						endPoints[0] = FindWordStart(doc,endPoints[0]);
						endPoints[1] = FindWordEnd(doc,endPoints[1]);
					}
					break;
				case TextUnit.Line:
				case  TextUnit.Format:
					{
						var line = doc.GetLineByOffset(endPoints[0]);
						endPoints[0] = line.Offset;
						endPoints[1] = line.EndOffset;
					}
					break;
			}
		}

		public ITextRangeProvider FindAttribute(int attribute, object value, bool backward)
		{
			return null;
		}

		public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
		{
			return null;
		}

		public object GetAttributeValue(int attribute)
		{
			return null;
		}

		public double[] GetBoundingRectangles()
		{
			return null;
		}

		public IRawElementProviderSimple[] GetChildren()
		{
			return null;
		}

		public IRawElementProviderSimple GetEnclosingElement()
		{
			return null;
		}

		public string GetText(int maxLength)
		{
			return doc.GetText(endPoints[0], endPoints[1] - endPoints[0]);
		}

		public int Move(System.Windows.Automation.Text.TextUnit unit, int count)
		{
			switch (unit)
			{
				case TextUnit.Character:
					{
						int toMove = Math.Max(Math.Min(doc.TextLength, endPoints[0] + count), 0) - endPoints[0];
						endPoints[0] += toMove;
						endPoints[1] += toMove;
						return toMove;
					}
			}
					return 0;
		}

		public void MoveEndpointByRange(System.Windows.Automation.Text.TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, System.Windows.Automation.Text.TextPatternRangeEndpoint targetEndpoint)
		{
			TextRangeProvider other = (TextRangeProvider)targetRange;
			endPoints[(int) endpoint] = other.endPoints[(int) targetEndpoint];
		}

		public int MoveEndpointByUnit(System.Windows.Automation.Text.TextPatternRangeEndpoint endpoint, System.Windows.Automation.Text.TextUnit unit, int count)
		{
			return 0;
		}

		public void RemoveFromSelection()
		{
		}

		public void ScrollIntoView(bool alignToTop)
		{
		}

		public void Select()
		{
		}
		static bool IsWordBorder(ITextSource document, int offset)
		{
			return TextUtilities.GetNextCaretPosition(document, offset - 1, LogicalDirection.Forward, CaretPositioningMode.WordBorder) == offset;
		}
		static int FindWordStart(ITextSource document, int offset)
		{
			char ch = document.GetCharAt(offset);
			if (char.IsWhiteSpace(ch))
				return offset;
			int start = TextUtilities.GetNextCaretPosition(document, offset+1, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);
			if (start < offset)
				return start;
			return offset;
		}
		static int FindWordEnd(ITextSource document, int offset)
		{
			char ch = document.GetCharAt(offset);
			if (char.IsWhiteSpace(ch))
				return offset+1;
			return TextUtilities.GetNextCaretPosition(document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);
		}
				
	}
}
