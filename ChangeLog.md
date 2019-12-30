Note: this changelog only lists major changes and fixes for major bugs. For a complete list of changes, see the git log.

2019/12/30: AvalonEdit 6.0.1
* Minor bug fixes

2019/09/24: AvalonEdit 6.0
* Add support for .NET Core 3.0
* Remove support for .NET 3.5
* Add proper .editorconfig and fix formatting
* Remove DOTNET4 and NREFACTORY symbols
* Create snukpg and use SourceLink

*Note: Versions 5.0.2, ..3 and ..4 are not contained in this change log*

2014/06/28: AvalonEdit 5.0.1-pre
* License changed from LGPL to MIT
* New Feature: Hide mouse cursor while typing (enabled by default)
* New Feature: Highlight current line (disabled by default)
* New Feature: Overstrike mode (disabled by default)
* AvalonEdit now raises the WPF DataObject attached events on clipboard and drag'n'drop operations.
* Encoding detection now distinguishes between UTF-8 with BOM and UTF-8 without BOM. This prevents AvalonEdit from adding the BOM to existing UTF-8 files.
* Improved handling of grapheme clusters. A base character followed by a combining mark is now treated as a single character by the caret movement logic.
* Added RichText, RichTextModel and RichTextColorizer.
* Renamed the VB highlighting mode from "VBNET" to "VB"
* Changed IHighlighter API in order to support SharpDevelop's semantic C# highlighter
* The regex-based highlighting engine was moved into its own class (HighlightingEngine) to be separated from the state-tracking logic in DocumentHighlighter.
* Add FileName property to TextDocument class.
* DocumentChangeEventArgs.RemovedText/InsertedText are now of type ITextSource instead of string.
* Removed the error-tolerant XML parser ("AvalonEdit.Xml"). An improved version of this parser is available as part of NRefactory 5.
* Removed some obsolete APIs.


2014/05/01: AvalonEdit 4.4.2.9744
* Fixed crash-on-launch in .NET 3.5 version of AvalonEdit.
* Fixed bug that caused deleted document lines to return IsDeleted=false after the whole document document content was replaced.


2014/04/30: AvalonEdit 4.4.1.9739 
* Fix crash when using AvalonEdit on multiple UI threads.


2014/01/23: AvalonEdit 4.4.0.9727
* Fix Home/End keys to only act on the current TextLine when word-wrapping is enabled, not on the whole VisualLine.
* Remove the caret stop in the middle of surrogate pairs, so that characters outside the basic multilingual plane work as expected.


2013/04/02: AvalonEdit 4.3.1.9430
* Fix bug that caused IME support to be disabled depending on which WPF control had the focus before AvalonEdit got focused.


2013/03/09: AvalonEdit 4.3.0.9390
* Added IME support
* Fix "InvalidOperationException: Trying to build visual line from collapsed line" when updating existing foldings


2012/05/12: AvalonEdit 4.2.0.8783
* Added SearchPanel
* Added support for virtual space
* C# syntax highlighting: Do not colorize punctuation


2011/09/24: AvalonEdit 4.1.0.8000
* Added region tooltips
* Improved WPF text rendering performance (use of DrawingVisual)


2011/01/16: AvalonEdit 4.0.0.7070
* First official stable release
