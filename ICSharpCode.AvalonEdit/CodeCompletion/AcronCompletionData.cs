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

   /// <inheritdoc/>
   public class AcronCompletionData : ICompletionData
   {
      /// <inheritdoc/>
      public AcronCompletionData()
      {

      }

      private ImageSource _image;
      /// <inheritdoc/>
      public ImageSource Image
      {
         get
         {
            return _image;
         }
         set
         {
            if (_image != value)
            {
               _image = value;
            }
         }
      }
      private string _text;
      /// <inheritdoc/>
      public string Text
      {
         get
         {
            return _text;
         }
         set
         {
            _text = value;
         }
      }
      private object _content;
      /// <inheritdoc/>
      public object Content
      {
         get
         {
            return _content;
         }
         set
         {
            _content = value;
         }
      }

      private object _description;
      /// <inheritdoc/>
      public object Description
      {
         get
         {
            return _description;
         }
         set
         {
            _description = value;
         }
      }

      private double _priority;
      /// <inheritdoc/>
      public double Priority
      {
         get
         {
            return _priority;
         }
         set
         {
            _priority = value;
         }
      }
      

      /// <inheritdoc/>
      public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
      {
         textArea.Document.Replace(completionSegment, Text);
         textArea.OnAutoCompleteFired(Text);
      }
   }
}
