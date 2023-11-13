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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using AcAvalonEdit.CodeCompletion;
using AcAvalonEdit.Document;
using AcAvalonEdit.Editing;
using AcAvalonEdit.Highlighting;
using AcAvalonEdit.Rendering;
using AcAvalonEdit.Utils;

namespace AcAvalonEdit
{
   /// <summary>
   /// The text editor control.
   /// Contains a scrollable TextArea.
   /// </summary>
   [Localizability(LocalizationCategory.Text), ContentProperty("Text")]
   public class TextEditor : Control, ITextEditorComponent, IServiceProvider, IWeakEventListener
   {
      #region Constructors
      static TextEditor()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(TextEditor),
                                        new FrameworkPropertyMetadata(typeof(TextEditor)));
         FocusableProperty.OverrideMetadata(typeof(TextEditor),
                                    new FrameworkPropertyMetadata(Boxes.True));
      }

      /// <summary>
      /// Creates a new TextEditor instance.
      /// </summary>
      public TextEditor() : this(new TextArea())
      {
         Document.Changed += ActionsOnDocumentChanged;
         Document.Changing += CompletionWindowOnReplace;

         RichTextColorizer FunctionUnderlining = new(TextArea.CursorColors);
         TextArea.TextView.LineTransformers.Add(FunctionUnderlining);

         TextArea.SelectionCornerRadius = 0;
      }

      /// <summary>
      /// Creates a new TextEditor instance.
      /// </summary>
      protected TextEditor(TextArea textArea)
      {
         if (textArea == null)
            throw new ArgumentNullException("textArea");
         this.textArea = textArea;

         textArea.TextView.Services.AddService(typeof(TextEditor), this);

         SetCurrentValue(OptionsProperty, textArea.Options);
         SetCurrentValue(DocumentProperty, new TextDocument());
      }

      #endregion

      /// <inheritdoc/>
      protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
      {
         return new TextEditorAutomationPeer(this);
      }

      /// Forward focus to TextArea.
      /// <inheritdoc/>
      protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
      {
         base.OnGotKeyboardFocus(e);
         if (e.NewFocus == this)
         {
            Keyboard.Focus(textArea);
            e.Handled = true;
         }
      }

      #region Document property
      /// <summary>
      /// Document property.
      /// </summary>
      public static readonly DependencyProperty DocumentProperty
         = TextView.DocumentProperty.AddOwner(
            typeof(TextEditor), new FrameworkPropertyMetadata(OnDocumentChanged));

      /// <summary>
      /// Gets/Sets the document displayed by the text editor.
      /// This is a dependency property.
      /// </summary>
      public TextDocument Document
      {
         get { return (TextDocument)GetValue(DocumentProperty); }
         set { SetValue(DocumentProperty, value); }
      }

      /// <summary>
      /// Occurs when the document property has changed.
      /// </summary>
      public event EventHandler DocumentChanged;

      /// <summary>
      /// Raises the <see cref="DocumentChanged"/> event.
      /// </summary>
      protected virtual void OnDocumentChanged(EventArgs e)
      {
         if (DocumentChanged != null)
         {
            DocumentChanged(this, e);
         }
      }

      static void OnDocumentChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
      {
         ((TextEditor)dp).OnDocumentChanged((TextDocument)e.OldValue, (TextDocument)e.NewValue);
      }

      void OnDocumentChanged(TextDocument oldValue, TextDocument newValue)
      {
         if (oldValue != null)
         {
            TextDocumentWeakEventManager.TextChanged.RemoveListener(oldValue, this);
            PropertyChangedEventManager.RemoveListener(oldValue.UndoStack, this, "IsOriginalFile");
         }
         textArea.Document = newValue;
         if (newValue != null)
         {
            TextDocumentWeakEventManager.TextChanged.AddListener(newValue, this);
            PropertyChangedEventManager.AddListener(newValue.UndoStack, this, "IsOriginalFile");
         }
         OnDocumentChanged(EventArgs.Empty);
         OnTextChanged(EventArgs.Empty);
      }
      #endregion

      #region Options property
      /// <summary>
      /// Options property.
      /// </summary>
      public static readonly DependencyProperty OptionsProperty
         = TextView.OptionsProperty.AddOwner(typeof(TextEditor), new FrameworkPropertyMetadata(OnOptionsChanged));

      /// <summary>
      /// Gets/Sets the options currently used by the text editor.
      /// </summary>
      public TextEditorOptions Options
      {
         get { return (TextEditorOptions)GetValue(OptionsProperty); }
         set { SetValue(OptionsProperty, value); }
      }

      /// <summary>
      /// Occurs when a text editor option has changed.
      /// </summary>
      public event PropertyChangedEventHandler OptionChanged;

      /// <summary>
      /// Raises the <see cref="OptionChanged"/> event.
      /// </summary>
      protected virtual void OnOptionChanged(PropertyChangedEventArgs e)
      {
         if (OptionChanged != null)
         {
            OptionChanged(this, e);
         }
      }

      static void OnOptionsChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
      {
         ((TextEditor)dp).OnOptionsChanged((TextEditorOptions)e.OldValue, (TextEditorOptions)e.NewValue);
      }

      void OnOptionsChanged(TextEditorOptions oldValue, TextEditorOptions newValue)
      {
         if (oldValue != null)
         {
            PropertyChangedWeakEventManager.RemoveListener(oldValue, this);
         }
         textArea.Options = newValue;
         if (newValue != null)
         {
            PropertyChangedWeakEventManager.AddListener(newValue, this);
         }
         OnOptionChanged(new PropertyChangedEventArgs(null));
      }

      /// <inheritdoc cref="IWeakEventListener.ReceiveWeakEvent"/>
      protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
      {
         if (managerType == typeof(PropertyChangedWeakEventManager))
         {
            OnOptionChanged((PropertyChangedEventArgs)e);
            return true;
         }
         else if (managerType == typeof(TextDocumentWeakEventManager.TextChanged))
         {
            OnTextChanged(e);
            return true;
         }
         else if (managerType == typeof(PropertyChangedEventManager))
         {
            return HandleIsOriginalChanged((PropertyChangedEventArgs)e);
         }
         return false;
      }

      bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
      {
         return ReceiveWeakEvent(managerType, sender, e);
      }
      #endregion

      #region Text property
      /// <summary>
      /// Gets/Sets the text of the current document.
      /// </summary>
      [Localizability(LocalizationCategory.Text), DefaultValue("")]
      public string Text
      {
         get
         {
            TextDocument document = this.Document;
            return document != null ? document.Text : string.Empty;
         }
         set
         {
            TextDocument document = GetDocument();
            document.Text = value ?? string.Empty;
            // after replacing the full text, the caret is positioned at the end of the document
            // - reset it to the beginning.
            this.CaretOffset = 0;
            document.UndoStack.ClearAll();
         }
      }

      TextDocument GetDocument()
      {
         TextDocument document = this.Document;
         if (document == null)
            throw ThrowUtil.NoDocumentAssigned();
         return document;
      }

      /// <summary>
      /// Occurs when the Text property changes.
      /// </summary>
      public event EventHandler TextChanged;

      /// <summary>
      /// Raises the <see cref="TextChanged"/> event.
      /// </summary>
      protected virtual void OnTextChanged(EventArgs e)
      {
         if (TextChanged != null)
         {
            TextChanged(this, e);
         }
      }
      #endregion

      #region TextArea / ScrollViewer properties
      readonly TextArea textArea;
      ScrollViewer scrollViewer;

      /// <summary>
      /// Is called after the template was applied.
      /// </summary>
      public override void OnApplyTemplate()
      {
         base.OnApplyTemplate();
         scrollViewer = (ScrollViewer)Template.FindName("PART_ScrollViewer", this);
      }

      /// <summary>
      /// Gets the text area.
      /// </summary>
      public TextArea TextArea
      {
         get
         {
            return textArea;
         }
      }

      /// <summary>
      /// Gets the scroll viewer used by the text editor.
      /// This property can return null if the template has not been applied / does not contain a scroll viewer.
      /// </summary>
      internal ScrollViewer ScrollViewer
      {
         get { return scrollViewer; }
      }

      bool CanExecute(RoutedUICommand command)
      {
         return command.CanExecute(null, textArea);
      }

      void Execute(RoutedUICommand command)
      {
         command.Execute(null, textArea);
      }
      #endregion

      #region Syntax highlighting
      /// <summary>
      /// The <see cref="SyntaxHighlighting"/> property.
      /// </summary>
      public static readonly DependencyProperty SyntaxHighlightingProperty =
         DependencyProperty.Register("SyntaxHighlighting", typeof(IHighlightingDefinition), typeof(TextEditor),
                              new FrameworkPropertyMetadata(OnSyntaxHighlightingChanged));


      /// <summary>
      /// Gets/sets the syntax highlighting definition used to colorize the text.
      /// </summary>
      public IHighlightingDefinition SyntaxHighlighting
      {
         get { return (IHighlightingDefinition)GetValue(SyntaxHighlightingProperty); }
         set { SetValue(SyntaxHighlightingProperty, value); }
      }

      IVisualLineTransformer colorizer;

      static void OnSyntaxHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         ((TextEditor)d).OnSyntaxHighlightingChanged(e.NewValue as IHighlightingDefinition);
      }

      void OnSyntaxHighlightingChanged(IHighlightingDefinition newValue)
      {
         if (colorizer != null)
         {
            textArea.TextView.LineTransformers.Remove(colorizer);
            colorizer = null;
         }
         if (newValue != null)
         {
            colorizer = CreateColorizer(newValue);
            if (colorizer != null)
               textArea.TextView.LineTransformers.Insert(0, colorizer);
         }
      }

      /// <summary>
      /// Creates the highlighting colorizer for the specified highlighting definition.
      /// Allows derived classes to provide custom colorizer implementations for special highlighting definitions.
      /// </summary>
      /// <returns></returns>
      protected virtual IVisualLineTransformer CreateColorizer(IHighlightingDefinition highlightingDefinition)
      {
         if (highlightingDefinition == null)
            throw new ArgumentNullException("highlightingDefinition");
         return new HighlightingColorizer(highlightingDefinition);
      }
      #endregion

      #region WordWrap
      /// <summary>
      /// Word wrap dependency property.
      /// </summary>
      public static readonly DependencyProperty WordWrapProperty =
         DependencyProperty.Register("WordWrap", typeof(bool), typeof(TextEditor),
                              new FrameworkPropertyMetadata(Boxes.False));

      /// <summary>
      /// Specifies whether the text editor uses word wrapping.
      /// </summary>
      /// <remarks>
      /// Setting WordWrap=true has the same effect as setting HorizontalScrollBarVisibility=Disabled and will override the
      /// HorizontalScrollBarVisibility setting.
      /// </remarks>
      public bool WordWrap
      {
         get { return (bool)GetValue(WordWrapProperty); }
         set { SetValue(WordWrapProperty, Boxes.Box(value)); }
      }
      #endregion

      #region IsReadOnly
      /// <summary>
      /// IsReadOnly dependency property.
      /// </summary>
      public static readonly DependencyProperty IsReadOnlyProperty =
         DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TextEditor),
                              new FrameworkPropertyMetadata(Boxes.False, OnIsReadOnlyChanged));

      /// <summary>
      /// Specifies whether the user can change the text editor content.
      /// Setting this property will replace the
      /// <see cref="Editing.TextArea.ReadOnlySectionProvider">TextArea.ReadOnlySectionProvider</see>.
      /// </summary>
      public bool IsReadOnly
      {
         get { return (bool)GetValue(IsReadOnlyProperty); }
         set { SetValue(IsReadOnlyProperty, Boxes.Box(value)); }
      }

      static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         TextEditor editor = d as TextEditor;
         if (editor != null)
         {
            if ((bool)e.NewValue)
               editor.TextArea.ReadOnlySectionProvider = ReadOnlySectionDocument.Instance;
            else
               editor.TextArea.ReadOnlySectionProvider = NoReadOnlySections.Instance;

            TextEditorAutomationPeer peer = TextEditorAutomationPeer.FromElement(editor) as TextEditorAutomationPeer;
            if (peer != null)
            {
               peer.RaiseIsReadOnlyChanged((bool)e.OldValue, (bool)e.NewValue);
            }
         }
      }
      #endregion

      #region IsModified
      /// <summary>
      /// Dependency property for <see cref="IsModified"/>
      /// </summary>
      public static readonly DependencyProperty IsModifiedProperty =
         DependencyProperty.Register("IsModified", typeof(bool), typeof(TextEditor),
                              new FrameworkPropertyMetadata(Boxes.False, OnIsModifiedChanged));

      /// <summary>
      /// Gets/Sets the 'modified' flag.
      /// </summary>
      public bool IsModified
      {
         get { return (bool)GetValue(IsModifiedProperty); }
         set { SetValue(IsModifiedProperty, Boxes.Box(value)); }
      }

      static void OnIsModifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         TextEditor editor = d as TextEditor;
         if (editor != null)
         {
            TextDocument document = editor.Document;
            if (document != null)
            {
               UndoStack undoStack = document.UndoStack;
               if ((bool)e.NewValue)
               {
                  if (undoStack.IsOriginalFile)
                     undoStack.DiscardOriginalFileMarker();
               }
               else
               {
                  undoStack.MarkAsOriginalFile();
               }
            }
         }
      }

      bool HandleIsOriginalChanged(PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "IsOriginalFile")
         {
            TextDocument document = this.Document;
            if (document != null)
            {
               SetCurrentValue(IsModifiedProperty, Boxes.Box(!document.UndoStack.IsOriginalFile));
            }
            return true;
         }
         else
         {
            return false;
         }
      }
      #endregion

      #region ShowLineNumbers
      /// <summary>
      /// ShowLineNumbers dependency property.
      /// </summary>
      public static readonly DependencyProperty ShowLineNumbersProperty =
         DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(TextEditor),
                              new FrameworkPropertyMetadata(Boxes.False, OnShowLineNumbersChanged));

      /// <summary>
      /// Specifies whether line numbers are shown on the left to the text view.
      /// </summary>
      public bool ShowLineNumbers
      {
         get { return (bool)GetValue(ShowLineNumbersProperty); }
         set { SetValue(ShowLineNumbersProperty, Boxes.Box(value)); }
      }

      static void OnShowLineNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         TextEditor editor = (TextEditor)d;
         var leftMargins = editor.TextArea.LeftMargins;
         if ((bool)e.NewValue)
         {
            LineNumberMargin lineNumbers = new LineNumberMargin();
            Line line = (Line)DottedLineMargin.Create();
            leftMargins.Insert(0, lineNumbers);
            leftMargins.Insert(1, line);
            var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = editor };
            line.SetBinding(Line.StrokeProperty, lineNumbersForeground);
            lineNumbers.SetBinding(Control.ForegroundProperty, lineNumbersForeground);
         }
         else
         {
            for (int i = 0; i < leftMargins.Count; i++)
            {
               if (leftMargins[i] is LineNumberMargin)
               {
                  leftMargins.RemoveAt(i);
                  if (i < leftMargins.Count && DottedLineMargin.IsDottedLineMargin(leftMargins[i]))
                  {
                     leftMargins.RemoveAt(i);
                  }
                  break;
               }
            }
         }
      }
      #endregion

      #region LineNumbersForeground
      /// <summary>
      /// LineNumbersForeground dependency property.
      /// </summary>
      public static readonly DependencyProperty LineNumbersForegroundProperty =
         DependencyProperty.Register("LineNumbersForeground", typeof(Brush), typeof(TextEditor),
                              new FrameworkPropertyMetadata(Brushes.Gray, OnLineNumbersForegroundChanged));

      /// <summary>
      /// Gets/sets the Brush used for displaying the foreground color of line numbers.
      /// </summary>
      public Brush LineNumbersForeground
      {
         get { return (Brush)GetValue(LineNumbersForegroundProperty); }
         set { SetValue(LineNumbersForegroundProperty, value); }
      }

      static void OnLineNumbersForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         TextEditor editor = (TextEditor)d;
         var lineNumberMargin = editor.TextArea.LeftMargins.FirstOrDefault(margin => margin is LineNumberMargin) as LineNumberMargin; ;

         if (lineNumberMargin != null)
         {
            lineNumberMargin.SetValue(Control.ForegroundProperty, e.NewValue);
         }
      }
      #endregion

      #region TextBoxBase-like methods
      /// <summary>
      /// Appends text to the end of the document.
      /// </summary>
      public void AppendText(string textData)
      {
         var document = GetDocument();
         document.Insert(document.TextLength, textData);
      }

      /// <summary>
      /// Begins a group of document changes.
      /// </summary>
      public void BeginChange()
      {
         GetDocument().BeginUpdate();
      }

      /// <summary>
      /// Copies the current selection to the clipboard.
      /// </summary>
      public void Copy()
      {
         Execute(ApplicationCommands.Copy);
      }

      /// <summary>
      /// Removes the current selection and copies it to the clipboard.
      /// </summary>
      public void Cut()
      {
         Execute(ApplicationCommands.Cut);
      }

      /// <summary>
      /// Begins a group of document changes and returns an object that ends the group of document
      /// changes when it is disposed.
      /// </summary>
      public IDisposable DeclareChangeBlock()
      {
         return GetDocument().RunUpdate();
      }

      /// <summary>
      /// Removes the current selection without copying it to the clipboard.
      /// </summary>
      public void Delete()
      {
         Execute(ApplicationCommands.Delete);
      }

      /// <summary>
      /// Ends the current group of document changes.
      /// </summary>
      public void EndChange()
      {
         GetDocument().EndUpdate();
      }

      /// <summary>
      /// Scrolls one line down.
      /// </summary>
      public void LineDown()
      {
         if (scrollViewer != null)
            scrollViewer.LineDown();
      }

      /// <summary>
      /// Scrolls to the left.
      /// </summary>
      public void LineLeft()
      {
         if (scrollViewer != null)
            scrollViewer.LineLeft();
      }

      /// <summary>
      /// Scrolls to the right.
      /// </summary>
      public void LineRight()
      {
         if (scrollViewer != null)
            scrollViewer.LineRight();
      }

      /// <summary>
      /// Scrolls one line up.
      /// </summary>
      public void LineUp()
      {
         if (scrollViewer != null)
            scrollViewer.LineUp();
      }

      /// <summary>
      /// Scrolls one page down.
      /// </summary>
      public void PageDown()
      {
         if (scrollViewer != null)
            scrollViewer.PageDown();
      }

      /// <summary>
      /// Scrolls one page up.
      /// </summary>
      public void PageUp()
      {
         if (scrollViewer != null)
            scrollViewer.PageUp();
      }

      /// <summary>
      /// Scrolls one page left.
      /// </summary>
      public void PageLeft()
      {
         if (scrollViewer != null)
            scrollViewer.PageLeft();
      }

      /// <summary>
      /// Scrolls one page right.
      /// </summary>
      public void PageRight()
      {
         if (scrollViewer != null)
            scrollViewer.PageRight();
      }

      /// <summary>
      /// Pastes the clipboard content.
      /// </summary>
      public void Paste()
      {
         Execute(ApplicationCommands.Paste);
      }

      /// <summary>
      /// Redoes the most recent undone command.
      /// </summary>
      /// <returns>True is the redo operation was successful, false is the redo stack is empty.</returns>
      public bool Redo()
      {
         if (CanExecute(ApplicationCommands.Redo))
         {
            Execute(ApplicationCommands.Redo);
            return true;
         }
         return false;
      }

      /// <summary>
      /// Scrolls to the end of the document.
      /// </summary>
      public void ScrollToEnd()
      {
         ApplyTemplate(); // ensure scrollViewer is created
         if (scrollViewer != null)
            scrollViewer.ScrollToEnd();
      }

      /// <summary>
      /// Scrolls to the start of the document.
      /// </summary>
      public void ScrollToHome()
      {
         ApplyTemplate(); // ensure scrollViewer is created
         if (scrollViewer != null)
            scrollViewer.ScrollToHome();
      }

      /// <summary>
      /// Scrolls to the specified position in the document.
      /// </summary>
      public void ScrollToHorizontalOffset(double offset)
      {
         ApplyTemplate(); // ensure scrollViewer is created
         if (scrollViewer != null)
            scrollViewer.ScrollToHorizontalOffset(offset);
      }

      /// <summary>
      /// Scrolls to the specified position in the document.
      /// </summary>
      public void ScrollToVerticalOffset(double offset)
      {
         ApplyTemplate(); // ensure scrollViewer is created
         if (scrollViewer != null)
            scrollViewer.ScrollToVerticalOffset(offset);
      }

      /// <summary>
      /// Selects the entire text.
      /// </summary>
      public void SelectAll()
      {
         Execute(ApplicationCommands.SelectAll);
      }

      /// <summary>
      /// Undoes the most recent command.
      /// </summary>
      /// <returns>True is the undo operation was successful, false is the undo stack is empty.</returns>
      public bool Undo()
      {
         if (CanExecute(ApplicationCommands.Undo))
         {
            Execute(ApplicationCommands.Undo);
            return true;
         }
         return false;
      }

      /// <summary>
      /// Gets if the most recent undone command can be redone.
      /// </summary>
      public bool CanRedo
      {
         get { return CanExecute(ApplicationCommands.Redo); }
      }

      /// <summary>
      /// Gets if the most recent command can be undone.
      /// </summary>
      public bool CanUndo
      {
         get { return CanExecute(ApplicationCommands.Undo); }
      }

      /// <summary>
      /// Gets the vertical size of the document.
      /// </summary>
      public double ExtentHeight
      {
         get
         {
            return scrollViewer != null ? scrollViewer.ExtentHeight : 0;
         }
      }

      /// <summary>
      /// Gets the horizontal size of the current document region.
      /// </summary>
      public double ExtentWidth
      {
         get
         {
            return scrollViewer != null ? scrollViewer.ExtentWidth : 0;
         }
      }

      /// <summary>
      /// Gets the horizontal size of the viewport.
      /// </summary>
      public double ViewportHeight
      {
         get
         {
            return scrollViewer != null ? scrollViewer.ViewportHeight : 0;
         }
      }

      /// <summary>
      /// Gets the horizontal size of the viewport.
      /// </summary>
      public double ViewportWidth
      {
         get
         {
            return scrollViewer != null ? scrollViewer.ViewportWidth : 0;
         }
      }

      /// <summary>
      /// Gets the vertical scroll position.
      /// </summary>
      public double VerticalOffset
      {
         get
         {
            return scrollViewer != null ? scrollViewer.VerticalOffset : 0;
         }
      }

      /// <summary>
      /// Gets the horizontal scroll position.
      /// </summary>
      public double HorizontalOffset
      {
         get
         {
            return scrollViewer != null ? scrollViewer.HorizontalOffset : 0;
         }
      }
      #endregion

      #region TextBox methods
      /// <summary>
      /// Gets/Sets the selected text.
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public string SelectedText
      {
         get
         {
            // We'll get the text from the whole surrounding segment.
            // This is done to ensure that SelectedText.Length == SelectionLength.
            if (textArea.Document != null && !textArea.Selection.IsEmpty)
               return textArea.Document.GetText(textArea.Selection.SurroundingSegment);
            else
               return string.Empty;
         }
         set
         {
            if (value == null)
               throw new ArgumentNullException("value");
            if (textArea.Document != null)
            {
               int offset = this.SelectionStart;
               int length = this.SelectionLength;
               textArea.Document.Replace(offset, length, value);
               // keep inserted text selected
               textArea.Selection = SimpleSelection.Create(textArea, offset, offset + value.Length);
            }
         }
      }

      /// <summary>
      /// Gets/sets the caret position.
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public int CaretOffset
      {
         get
         {
            return textArea.Caret.Offset;
         }
         set
         {
            textArea.Caret.Offset = value;
         }
      }

      /// <summary>
      /// Gets/sets the start position of the selection.
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public int SelectionStart
      {
         get
         {
            if (textArea.Selection.IsEmpty)
               return textArea.Caret.Offset;
            else
               return textArea.Selection.SurroundingSegment.Offset;
         }
         set
         {
            Select(value, SelectionLength);
         }
      }

      /// <summary>
      /// Gets/sets the length of the selection.
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public int SelectionLength
      {
         get
         {
            if (!textArea.Selection.IsEmpty)
               return textArea.Selection.SurroundingSegment.Length;
            else
               return 0;
         }
         set
         {
            Select(SelectionStart, value);
         }
      }

      /// <summary>
      /// Selects the specified text section.
      /// </summary>
      public void Select(int start, int length)
      {
         int documentLength = Document != null ? Document.TextLength : 0;
         if (start < 0 || start > documentLength)
            throw new ArgumentOutOfRangeException("start", start, "Value must be between 0 and " + documentLength);
         if (length < 0 || start + length > documentLength)
            throw new ArgumentOutOfRangeException("length", length, "Value must be between 0 and " + (documentLength - start));
         textArea.Selection = SimpleSelection.Create(textArea, start, start + length);
         textArea.Caret.Offset = start + length;
      }

      /// <summary>
      /// Gets the number of lines in the document.
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public int LineCount
      {
         get
         {
            TextDocument document = this.Document;
            if (document != null)
               return document.LineCount;
            else
               return 1;
         }
      }

      /// <summary>
      /// Clears the text.
      /// </summary>
      public void Clear()
      {
         this.Text = string.Empty;
      }
      #endregion

      #region Loading from stream
      /// <summary>
      /// Loads the text from the stream, auto-detecting the encoding.
      /// </summary>
      /// <remarks>
      /// This method sets <see cref="IsModified"/> to false.
      /// </remarks>
      public void Load(Stream stream)
      {
         using (StreamReader reader = FileReader.OpenStream(stream, this.Encoding ?? Encoding.UTF8))
         {
            this.Text = reader.ReadToEnd();
            SetCurrentValue(EncodingProperty, reader.CurrentEncoding); // assign encoding after ReadToEnd() so that the StreamReader can autodetect the encoding
         }
         SetCurrentValue(IsModifiedProperty, Boxes.False);
      }

      /// <summary>
      /// Loads the text from the stream, auto-detecting the encoding.
      /// </summary>
      public void Load(string fileName)
      {
         if (fileName == null)
            throw new ArgumentNullException("fileName");
         using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
         {
            Load(fs);
         }
      }

      /// <summary>
      /// Encoding dependency property.
      /// </summary>
      public static readonly DependencyProperty EncodingProperty =
         DependencyProperty.Register("Encoding", typeof(Encoding), typeof(TextEditor));

      /// <summary>
      /// Gets/sets the encoding used when the file is saved.
      /// </summary>
      /// <remarks>
      /// The <see cref="Load(Stream)"/> method autodetects the encoding of the file and sets this property accordingly.
      /// The <see cref="Save(Stream)"/> method uses the encoding specified in this property.
      /// </remarks>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public Encoding Encoding
      {
         get { return (Encoding)GetValue(EncodingProperty); }
         set { SetValue(EncodingProperty, value); }
      }

      /// <summary>
      /// Saves the text to the stream.
      /// </summary>
      /// <remarks>
      /// This method sets <see cref="IsModified"/> to false.
      /// </remarks>
      public void Save(Stream stream)
      {
         if (stream == null)
            throw new ArgumentNullException("stream");
         var encoding = this.Encoding;
         var document = this.Document;
         StreamWriter writer = encoding != null ? new StreamWriter(stream, encoding) : new StreamWriter(stream);
         if (document != null)
            document.WriteTextTo(writer);
         writer.Flush();
         // do not close the stream
         SetCurrentValue(IsModifiedProperty, Boxes.False);
      }

      /// <summary>
      /// Saves the text to the file.
      /// </summary>
      public void Save(string fileName)
      {
         if (fileName == null)
            throw new ArgumentNullException("fileName");
         using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
         {
            Save(fs);
         }
      }
      #endregion

      #region MouseHover events
      /// <summary>
      /// The PreviewMouseHover event.
      /// </summary>
      public static readonly RoutedEvent PreviewMouseHoverEvent =
         TextView.PreviewMouseHoverEvent.AddOwner(typeof(TextEditor));

      /// <summary>
      /// The MouseHover event.
      /// </summary>
      public static readonly RoutedEvent MouseHoverEvent =
         TextView.MouseHoverEvent.AddOwner(typeof(TextEditor));


      /// <summary>
      /// The PreviewMouseHoverStopped event.
      /// </summary>
      public static readonly RoutedEvent PreviewMouseHoverStoppedEvent =
         TextView.PreviewMouseHoverStoppedEvent.AddOwner(typeof(TextEditor));

      /// <summary>
      /// The MouseHoverStopped event.
      /// </summary>
      public static readonly RoutedEvent MouseHoverStoppedEvent =
         TextView.MouseHoverStoppedEvent.AddOwner(typeof(TextEditor));


      /// <summary>
      /// Occurs when the mouse has hovered over a fixed location for some time.
      /// </summary>
      public event MouseEventHandler PreviewMouseHover
      {
         add { AddHandler(PreviewMouseHoverEvent, value); }
         remove { RemoveHandler(PreviewMouseHoverEvent, value); }
      }

      /// <summary>
      /// Occurs when the mouse has hovered over a fixed location for some time.
      /// </summary>
      public event MouseEventHandler MouseHover
      {
         add { AddHandler(MouseHoverEvent, value); }
         remove { RemoveHandler(MouseHoverEvent, value); }
      }

      /// <summary>
      /// Occurs when the mouse had previously hovered but now started moving again.
      /// </summary>
      public event MouseEventHandler PreviewMouseHoverStopped
      {
         add { AddHandler(PreviewMouseHoverStoppedEvent, value); }
         remove { RemoveHandler(PreviewMouseHoverStoppedEvent, value); }
      }

      /// <summary>
      /// Occurs when the mouse had previously hovered but now started moving again.
      /// </summary>
      public event MouseEventHandler MouseHoverStopped
      {
         add { AddHandler(MouseHoverStoppedEvent, value); }
         remove { RemoveHandler(MouseHoverStoppedEvent, value); }
      }
      #endregion

      #region ScrollBarVisibility
      /// <summary>
      /// Dependency property for <see cref="HorizontalScrollBarVisibility"/>
      /// </summary>
      public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(TextEditor), new FrameworkPropertyMetadata(ScrollBarVisibility.Visible));

      /// <summary>
      /// Gets/Sets the horizontal scroll bar visibility.
      /// </summary>
      public ScrollBarVisibility HorizontalScrollBarVisibility
      {
         get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
         set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
      }

      /// <summary>
      /// Dependency property for <see cref="VerticalScrollBarVisibility"/>
      /// </summary>
      public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(TextEditor), new FrameworkPropertyMetadata(ScrollBarVisibility.Visible));

      /// <summary>
      /// Gets/Sets the vertical scroll bar visibility.
      /// </summary>
      public ScrollBarVisibility VerticalScrollBarVisibility
      {
         get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
         set { SetValue(VerticalScrollBarVisibilityProperty, value); }
      }
      #endregion


      #region Stuff
      object IServiceProvider.GetService(Type serviceType)
      {
         return textArea.GetService(serviceType);
      }

      /// <summary>
      /// Gets the text view position from a point inside the editor.
      /// </summary>
      /// <param name="point">The position, relative to top left
      /// corner of TextEditor control</param>
      /// <returns>The text view position, or null if the point is outside the document.</returns>
      public TextViewPosition? GetPositionFromPoint(Point point)
      {
         if (this.Document == null)
            return null;
         TextView textView = this.TextArea.TextView;
         return textView.GetPosition(TranslatePoint(point, textView) + textView.ScrollOffset);
      }

      /// <summary>
      /// Scrolls to the specified line.
      /// This method requires that the TextEditor was already assigned a size (WPF layout must have run prior).
      /// </summary>
      public void ScrollToLine(int line)
      {
         ScrollTo(line, -1);
      }

      /// <summary>
      /// Scrolls to the specified line/column.
      /// This method requires that the TextEditor was already assigned a size (WPF layout must have run prior).
      /// </summary>
      public void ScrollTo(int line, int column)
      {
         const double MinimumScrollFraction = 0.3;
         ScrollTo(line, column, VisualYPosition.LineMiddle, null != scrollViewer ? scrollViewer.ViewportHeight / 2 : 0.0, MinimumScrollFraction);
      }

      /// <summary>
      /// Scrolls to the specified line/column.
      /// This method requires that the TextEditor was already assigned a size (WPF layout must have run prior).
      /// </summary>
      /// <param name="line">Line to scroll to.</param>
      /// <param name="column">Column to scroll to (important if wrapping is 'on', and for the horizontal scroll position).</param>
      /// <param name="yPositionMode">The mode how to reference the Y position of the line.</param>
      /// <param name="referencedVerticalViewPortOffset">Offset from the top of the viewport to where the referenced line/column should be positioned.</param>
      /// <param name="minimumScrollFraction">The minimum vertical and/or horizontal scroll offset, expressed as fraction of the height or width of the viewport window, respectively.</param>
      public void ScrollTo(int line, int column, VisualYPosition yPositionMode, double referencedVerticalViewPortOffset, double minimumScrollFraction)
      {
         TextView textView = textArea.TextView;
         TextDocument document = textView.Document;
         if (scrollViewer != null && document != null)
         {
            if (line < 1)
               line = 1;
            if (line > document.LineCount)
               line = document.LineCount;

            IScrollInfo scrollInfo = textView;
            if (!scrollInfo.CanHorizontallyScroll)
            {
               // Word wrap is enabled. Ensure that we have up-to-date info about line height so that we scroll
               // to the correct position.
               // This avoids that the user has to repeat the ScrollTo() call several times when there are very long lines.
               VisualLine vl = textView.GetOrConstructVisualLine(document.GetLineByNumber(line));
               double remainingHeight = referencedVerticalViewPortOffset;

               while (remainingHeight > 0)
               {
                  DocumentLine prevLine = vl.FirstDocumentLine.PreviousLine;
                  if (prevLine == null)
                     break;
                  vl = textView.GetOrConstructVisualLine(prevLine);
                  remainingHeight -= vl.Height;
               }
            }

            Point p = textArea.TextView.GetVisualPosition(new TextViewPosition(line, Math.Max(1, column)), yPositionMode);
            double verticalPos = p.Y - referencedVerticalViewPortOffset;
            if (Math.Abs(verticalPos - scrollViewer.VerticalOffset) > minimumScrollFraction * scrollViewer.ViewportHeight)
            {
               scrollViewer.ScrollToVerticalOffset(Math.Max(0, verticalPos));
            }
            if (column > 0)
            {
               if (p.X > scrollViewer.ViewportWidth - Caret.MinimumDistanceToViewBorder * 2)
               {
                  double horizontalPos = Math.Max(0, p.X - scrollViewer.ViewportWidth / 2);
                  if (Math.Abs(horizontalPos - scrollViewer.HorizontalOffset) > minimumScrollFraction * scrollViewer.ViewportWidth)
                  {
                     scrollViewer.ScrollToHorizontalOffset(horizontalPos);
                  }
               }
               else
               {
                  scrollViewer.ScrollToHorizontalOffset(0);
               }
            }
         }
      }

      #endregion


      #region Custom Additions

      static readonly string CarriageReturn = "↵";
      /// <summary>
      /// Sets whether Shift+Enter and Alt+Enter should be transformed to a Carriage Return
      /// </summary>
      public bool TransformModifiedEnterToCarriageReturn { get; set; } = false;

      /// <summary>
      /// Hide or Show Caret when not in Focus
      /// </summary>
      public bool HideCaret
      {
         get
         {
            return TextArea.HideCaret;
         }
         set
         {
            if (value == TextArea.HideCaret)
               return;
            TextArea.HideCaret = value;
            if (!value)
            {
               TextArea.Caret.Show();
            }
            else
            {
               TextArea.Caret.Hide();
            }
         }
      }

      private RichTextModel _model = new();
      /// <summary>
      /// Returns the indices for all SelectedLines
      /// </summary>
      /// <returns></returns>
      public int[]? GetSelectedLines(bool AtCaretChanged)
      {
         if (AtCaretChanged)
            return new int[1] { TextArea.Caret.Line - 1 };
         else
         {

            if (SelectionLength == 0)
               return null;

            var OffsetStart = TextArea.Selection.SurroundingSegment.Offset - 1;
            var OffsetEnd = TextArea.Selection.SurroundingSegment.EndOffset;
            if (OffsetStart > -1 && OffsetEnd < Text.Length && Text[OffsetStart] == VariableDelimiters[0] && Text[OffsetEnd] == VariableDelimiters[1])
            {
               TextArea.Selection = Selection.Create(TextArea, OffsetStart, OffsetEnd + 1);
               CaretOffset = OffsetEnd + 1;
            }

            int startline = TextArea.Selection.StartPosition.Line - 1;
            int endline = TextArea.Selection.EndPosition.Line - 1;
            if (endline < startline)
            {
               (startline, endline) = (endline, startline);
            }
            return Enumerable.Range(startline, (endline - startline) + 1).ToArray();
         }
      }

      /// <summary>
      /// Fills the Text with a given string, does not modify the Font settings
      /// </summary>
      public void LoadText(string inputString)
      {
         Document.Changed -= ActionsOnDocumentChanged;
         Document.Changing -= CompletionWindowOnReplace;
         Text = inputString;
         Document.Changed += ActionsOnDocumentChanged;
         Document.Changing += CompletionWindowOnReplace;
      }

      /// <summary>
      /// Fills the Text and creates a Rich Text Model from the provided Lists of strings and HighlightingColor
      /// </summary>
      public void Load(List<string> text, List<HighlightingColor> highlightingColors)
      {
         //Remove Eventhandlers
         Document.Changed -= ActionsOnDocumentChanged;
         Document.Changing -= CompletionWindowOnReplace;

         //Clean Up
         _model.Clear();
         TextArea.TextView.LineTransformers.Clear();


         if (text.Count != highlightingColors.Count)
            throw new ArgumentException("Arguments must be the same length");

         int length = text.Sum(x => x.Length);
         int newLineCount = text.Sum(x => CountLines(x));

         //Calculate BufferLength for Span
         int bufferLength = length - (newLineCount * Environment.NewLine.Length) + (text.Count - 1) * Environment.NewLine.Length;

         Span<char> buffer = stackalloc char[bufferLength];
         int bufferPos = 0;

         for (int i = 0; i < text.Count; i++)
         {
            bufferPos = HideInternalLinebreaks(text, highlightingColors, buffer, bufferPos, i);
         }


         Text = buffer.ToString();
         RichTextColorizer color = new(_model);
         TextArea.TextView.LineTransformers.Add(color);
         RichTextColorizer FunctionUnderlining = new(TextArea.CursorColors);
         TextArea.TextView.LineTransformers.Add(FunctionUnderlining);
         //Reapply EventHandlers
         Document.Changed += ActionsOnDocumentChanged;
         Document.Changing += CompletionWindowOnReplace;
      }

      /// <summary>
      /// Transforms the input List of Textelements into a single string with hidden linebreaks
      /// </summary>
      /// <param name="text"></param>
      /// <param name="highlightingColors"></param>
      /// <param name="buffer"></param>
      /// <param name="bufferPos"></param>
      /// <param name="i"></param>
      /// <returns></returns>
      private int HideInternalLinebreaks(List<string> text, List<HighlightingColor> highlightingColors, Span<char> buffer, int bufferPos, int i)
      {
         var currentSpan = text[i].AsSpan();
         int startIndex = bufferPos;
         for (int j = 0; j < currentSpan.Length; j++)
         {
            if (j != currentSpan.Length - 1)
               if (currentSpan.Slice(j, 2).SequenceEqual(Environment.NewLine))
               {
                  _model.SetEOLMarker(bufferPos);
                  j++;
                  continue;
               }
            buffer[bufferPos] = currentSpan[j];
            bufferPos++;
         }
         if (i != text.Count - 1)
            foreach (var c in Environment.NewLine)
            {
               buffer[bufferPos] = c;
               bufferPos++;
            }
         int endIndex = bufferPos;
         _model.ApplyHighlighting(startIndex, (endIndex - startIndex) + 1, highlightingColors[i]);
         return bufferPos;
      }

      /// <summary>
      /// Counts Linebreaks in given string
      /// </summary>
      /// <param name="str"></param>
      /// <returns></returns>
      private static int CountLines(string str)
      {
         if (string.IsNullOrEmpty(str))
            return 0;

         int index = -1;
         int count = 0;

         while (-1 != (index = str.IndexOf(Environment.NewLine, index + 1)))
            count++;

         return count;
      }

      private StringBuilder BuildDocumentText(RichTextModel model, List<string> text, List<HighlightingColor> highlightingColors)
      {
         StringBuilder builder = new StringBuilder(text.Sum(x => x.Length));
         for (int i = 0; i < text.Count; i++)
         {
            model.ApplyHighlighting(Math.Max(0, builder.Length - 1), text[i].Length + Environment.NewLine.Length, highlightingColors[i]);
            builder.Append(text[i]);
            builder.Append(Environment.NewLine);
         }
         return builder;
      }

      private HighlightingColor BuildHighlightingFromRun(Run run)
      {
         HighlightingColor hlc = new();

         hlc.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(run.Foreground.ToString()));
         hlc.Background = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString((run.Background.ToString())));

         hlc.FontFamily = run.FontFamily;
         hlc.FontStyle = run.FontStyle;
         hlc.FontWeight = run.FontWeight;
         hlc.FontStretch = run.FontStretch;

         hlc.Strikethrough = run.TextDecorations.Contains(TextDecorations.Strikethrough.First());
         hlc.Underline = run.TextDecorations.Contains(TextDecorations.Underline.First());


         return hlc;

      }
      /// <summary>
      /// Applies specific Highlighting to the current Selection
      /// </summary>
      public void ApplyHighlightingToSelection(HighlightingColor highlightingColor)
      {
         _model.ApplyHighlighting(Document.GetOffset(TextArea.Selection.StartPosition.Location), TextArea.Selection.Length, highlightingColor);
      }

      /// <summary>
      /// Applies specific Highlighting to the current Selection, on a per line basis for Compability
      /// </summary>
      public void ApplyHighlightingToLine(HighlightingColor highlightingColor)
      {
         if (SelectionLength == 0)
         {
            _model.ApplyHighlighting(Document.Lines[TextArea.Caret.Line - 1].Offset, Document.Lines[TextArea.Caret.Line - 1].Length + Environment.NewLine.Length, highlightingColor);
         }
         else
         {
            int startline = TextArea.Selection.StartPosition.Line;
            int endline = TextArea.Selection.EndPosition.Line;
            if (endline < startline)
            {
               (startline, endline) = (endline, startline);
            }
            for (int i = startline - 1; i < endline; i++)
            {
               _model.ApplyHighlighting(Document.Lines[i].Offset, Document.Lines[i].Length + Environment.NewLine.Length, highlightingColor);
            }
         }

         TextArea.TextView.Redraw();
      }

      /// <summary>
      /// Converts the Rich Text into a FlowDocument for Consumption, on a per Line basis for compability
      /// </summary>
      /// <returns></returns>
      public FlowDocument GetFlowDocumentPerLine()
      {
         TextArea.Tooltip.Content = null;
         TextArea.Tooltip.IsOpen = false;
         RichText rich = new(Text, _model);

         var runs = rich.CreateRunsOnLineBreaks();
         FlowDocument document = new FlowDocument();
         foreach (Run run in runs)
         {
            if (string.IsNullOrEmpty(run.Text))
               run.Text = " ";
            document.Blocks.Add(new Paragraph());
            (document.Blocks.Last() as Paragraph)!.Inlines.Add(run);
         }
         return document;
      }

      /// <summary>
      /// Returns the RichText of the current Highlighting and underlying Text
      /// </summary>
      /// <returns></returns>
      public RichText GetRichText()
      {
         return new RichText(Text, _model);
      }

      private void ActionsOnDocumentChanged(object? sender, DocumentChangeEventArgs e)
      {
         _model.UpdateOffsets(e.OffsetChangeMap);
         if (_callBackForParsing is not null)
         {
            int line = TextArea.Caret.Line - 1;
            var currLine = Document.GetText(Document.Lines[line].Offset, Document.Lines[line].EndOffset - Document.Lines[line].Offset);
            IEnumerable<int> relevantHighlights = _model.GetHighlightingsAt(Document.Lines[line].Offset, Document.Lines[line].EndOffset).Where(x => x.Item1.EOLMarker.HasValue).Select(x => x.Item2);
            int offset = 0;
            foreach (var hl in relevantHighlights)
            {
               currLine = currLine.Insert((offset + (hl - Document.Lines[line].Offset)), Environment.NewLine);
               offset += Environment.NewLine.Length;
            }
            if (CompletionWindow is null || CompletionWindow.CompletionList.VisibleCompletionsCount == 0)
               _callBackForParsing(currLine, line);
            else
               _callBackForParsing("\"\"", line);
         }
      }

      private void CompletionWindowOnReplace(object? sender, DocumentChangeEventArgs e)
      {
         if (CompletionWindow is null)
            return;
         if (e.RemovalLength > 0 && e.InsertionLength == 1)
         {
            if (CreateCompletionWindow())
               CompletionWindow.StartOffset--;
            else
            {
               CompletionWindow.StartOffset = e.Offset;
               CompletionWindow.EndOffset = e.Offset + e.InsertionLength;

            }
         }
      }

      private bool _autocompleteOn = false;
      private List<ICompletionData> _completionData;
      private List<ICompletionData> _defaultData;

      internal static CompletionWindow? CompletionWindow;

      /// <summary>
      /// Enables a standard implementation of Autocomplete with the provided completion data
      /// </summary>
      /// <param name="completions"></param>
      public void InitializeDefaultAutoCompleteWith(List<ICompletionData> completions)
      {
         if (_autocompleteOn) DisableDefaultAutocomplete();
         _completionData = completions;
         _defaultData = completions;
         TextArea.AutoCompleteFired += OnAutoCompletion;
         TextArea.TextEntering += OnTextEntering;
         TextArea.TextEntered += OnTextEntered;
         _autocompleteOn = true;
      }
      /// <summary>
      /// Fills the Syntax Lookup for floating tooltips with Framework Elements
      /// </summary>
      /// <param name="syntax"></param>
      public void FillSyntaxLookUp(List<(string, FrameworkElement)> syntax)
      {
         TextArea.FillSyntaxLookUp(syntax);
      }

      /// <summary>
      /// Enables or disables Syntax Tooltips
      /// </summary>
      public bool EnableSyntaxTooltip
      {
         get
         {
            return TextArea.EnableSyntaxTooltip;
         }
         set
         {
            TextArea.EnableSyntaxTooltip = value;
         }
      }

      /// <summary>
      ///Adds additional Completion Data to the completion window, either with or without soft-reset 
      /// </summary>
      /// <param name="list"></param>
      /// <param name="fromDefault"></param>
      public void AddAdditionalCompletions(List<ICompletionData> list, bool fromDefault = true)
      {

         if (fromDefault)
         {
            _completionData = _defaultData;
         }

         _completionData.AddRange(list);
      }


      internal bool FreeText
      {
         get
         {
            var relevant = Document.GetText(Document.Lines[TextArea.Caret.Line - 1].Offset, TextArea.Caret.Offset - Document.Lines[TextArea.Caret.Line - 1].Offset).AsSpan();
            int counter = 0;
            for (int i = 0; i < relevant.Length; i++)
            {
               if (relevant[i] == '"')
                  counter++;
            }
            if (counter % 2 == 0)
            {
               return false;
            }
            return true;
         }
      }


      /// <summary>
      /// Disables a standard implementation of Autocomplete 
      /// </summary>
      public void DisableDefaultAutocomplete()
      {
         if (!_autocompleteOn) return;
         TextArea.TextEntering -= OnTextEntering;
         TextArea.TextEntered -= OnTextEntered;
         TextArea.AutoCompleteFired -= OnAutoCompletion;
         CompletionWindow?.Close();
         CompletionWindow?.CompletionList.CompletionData.Clear();
         CompletionWindow = null;
         _autocompleteOn = false;
      }

      private void OnTextEntered(object sender, TextCompositionEventArgs e)
      {
         if (FreeText)
            return;
         if (CompletionWindow is not null && e.Text.Length > 0)
         {
            if (char.IsControl(e.Text[0]))
            {
               CompletionWindow.Close();
            }
         }
         if (CompletionWindow is not null && e.Text == "." && _callBackForVariables is not null)
         {
            if (CompletionWindow.CompletionList.VisibleCompletionsCount > 0)
            {
               return;
            }
            string lastWord = TextArea.GetLastWord();
            List<ICompletionData>? ListToAdd = _callBackForVariables(lastWord);

            if (ListToAdd is not null)
            {
               foreach (var cData in ListToAdd)
               {
                  CompletionWindow.CompletionList.CompletionData.Add(cData);
               }
            }
            CompletionWindow.Refresh(lastWord);
         }
      }

      private void OnTextEntering(object sender, TextCompositionEventArgs e)
      {
         if (e.Text == " ")
         {
            CompletionWindow?.Close();
            return;
         }
         CreateCompletionWindow();
      }

      private bool CreateCompletionWindow()
      {
         if (FreeText || CompletionWindow is not null)
            return false;

         CompletionWindow ??= new CompletionWindow(TextArea);

         TextArea.Tooltip.IsOpen = false;
         TextArea.Tooltip.Content = null;

         IList<ICompletionData> boundData = CompletionWindow.CompletionList.CompletionData;
         boundData.Clear();
         foreach (ICompletionData data in _completionData)
         {
            boundData.Add(data);
         }
         if (boundData.Any())
         {
            CompletionWindow.Show();
            CompletionWindow.Closed += delegate
            {
               CompletionWindow = null;
               TextArea.UnderlineSelectedFunctionParameters();
            };
         }
         return true;
      }

      private async void OnInsertionRequested(object? sender, EventArgs e)
      {
         int pre = Text.Length;
         await Task.Delay(TimeSpan.FromMilliseconds(10));
         SelectVariable(Text.Length - pre);

      }

      /// <summary>
      /// Replaces the selected Text with a given string
      /// </summary>
      /// <param name="input"></param>
      public void InsertText(string input, bool formular)
      {
         TextArea.Selection.ReplaceSelectionWithText(input);
         if (AutoSelectReplaceableVariables && formular)
         {
            SelectVariable(input.Length);
         }
         else
         {
            TextArea.ClearSelection();
         }
      }

      private void OnAutoCompletion(object sender, TextCompositionEventArgs e)
      {
         SelectVariable(e.Text.Length);
      }
      /// <summary>
      /// When inserting Text should placeholders be automatically selected 
      /// </summary>
      public bool AutoSelectReplaceableVariables { get; set; } = true;

      /// <summary>
      ///Start and end Delimiter for the variables 
      /// </summary>
      public char[] VariableDelimiters { get; set; } = new char[2];

      private Func<string, List<ICompletionData>>? _callBackForVariables;

      /// <summary>
      /// Sets a callback function to handle Intellisense on lazyload variables
      /// </summary>
      /// <param name="func"></param>
      public void SetFunctionForIntellisenseCallback(Func<string, List<ICompletionData>>? func)
      {
         _callBackForVariables = func;
      }

      private Func<string, int, bool>? _callBackForParsing;

      /// <summary>
      /// Sets callback for parsing a Formula
      /// </summary>
      /// <param name="func"></param>
      public void SetParseCallbackFunction(Func<string, int, bool>? func)
      {
         _callBackForParsing = func;
      }

      private void SelectVariable(int length)
      {
         if (length == 0)
            return;
         int offset = TextArea.Caret.Offset - length;
         var input = Text.AsSpan().Slice(offset, length);
         if (input.Contains(VariableDelimiters[0]) && input.Contains(VariableDelimiters[1]))
         {
            var start = input.IndexOf(VariableDelimiters[0]);
            var end = input.IndexOf(VariableDelimiters[1]) + 1;
            if (start > end) (start, end) = (end, start);
            TextArea.Caret.Offset = offset + end;
            TextArea.Selection = Selection.Create(TextArea, offset + start, offset + end);
         }
      }

      private bool SelecteNextVariable()
      {
         int offset = TextArea.Caret.Offset;
         var section = Text.AsSpan();

         if (section.Slice(offset, Text.Length - offset).Contains(VariableDelimiters[0]) && section.Slice(offset, Text.Length - offset).Contains(VariableDelimiters[1]))
         {
            var start = section.Slice(offset, Text.Length - offset).IndexOf(VariableDelimiters[0]);
            var end = section.Slice(offset, Text.Length - offset).IndexOf(VariableDelimiters[1]) + 1;
            if (start > end) (start, end) = (end, start);
            TextArea.Caret.Offset = end + offset;
            TextArea.Selection = Selection.Create(TextArea, start + offset, end + offset);

         }
         else if (section.Slice(0, offset).Contains(VariableDelimiters[0]) && section.Slice(0, offset).Contains(VariableDelimiters[1]))
         {
            var start = section.Slice(0, offset).IndexOf(VariableDelimiters[0]);
            var end = section.Slice(0, offset).IndexOf(VariableDelimiters[1]) + 1;
            if (start > end) (start, end) = (end, start);
            TextArea.Caret.Offset = end;
            TextArea.Selection = Selection.Create(TextArea, start, end);
         }
         else
         {
            return false;
         }

         return true;

      }

      /// <summary>
      /// Enables or disables Highlighting of the current position inside of a function
      /// </summary>
      public bool HighlightSelectedFormula
      {
         get
         {
            return TextArea.HighlightSelectedFormula;
         }
         set
         {
            TextArea.HighlightSelectedFormula = value;
         }
      }



      ///<inheritdoc/>
      protected override void OnPreviewKeyDown(KeyEventArgs e)
      {
         base.OnKeyDown(e);
         if (!e.Handled)
         {
            HandleKey(e);
         }
      }

      private void HandleKey(KeyEventArgs e)
      {
         if (TransformModifiedEnterToCarriageReturn)
         {
            if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
               TextArea.Selection.ReplaceSelectionWithText(CarriageReturn);
               e.Handled = true;
               return;
            }
         }
         if (!FreeText && !e.Handled)
            if (e.Key == Key.Tab)
            {
               CompletionWindow?.Close();
               e.Handled = SelecteNextVariable();
            }
         if (e.Handled || FreeText || CompletionWindow is not null)
            return;
         if (e.Key == Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
         {
            CreateCompletionWindow();
            e.Handled = true;
         }
      }
      #endregion
   }
}
