using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using AcAvalonEdit.Document;
using AcAvalonEdit.Editing;

namespace AcAvalonEdit.CodeCompletion
{
#pragma warning disable CS1591 // Fehledes XML-Kommentar für öffentlich sichtbaren Typ oder Element
	public class AcronCompletionData : ICompletionData
	{
		public AcronCompletionData()
		{

		}

		private ImageSource _image;
		public ImageSource Image {
			get {
				return _image;
			}
			set {
				if (_image != value) {
					_image = value;
				}
			}
		}
		private string _text;
		public string Text {
			get {
				return _text;
			}
			set {
				_text = value;
			}
		}
		private object _content;
		public object Content {
			get {
				return _content;
			}
			set {
				_content = value;
			}
		}

		private object _description;
		public object Description {
			get {
				return _description;
			}
			set {
				_description = value;
			}
		}
		private double _priority;
		public double Priority {
			get {
				return _priority;
			}
			set {
				_priority = value;
			}
		}

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			textArea.Document.Replace(completionSegment, Text);
		}
	}

#pragma warning restore CS1591 // Fehledes XML-Kommentar für öffentlich sichtbaren Typ oder Element
}
