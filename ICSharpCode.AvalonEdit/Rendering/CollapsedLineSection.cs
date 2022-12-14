// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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
#nullable enable
using System;

using ICSharpCode.AvalonEdit.Document;

namespace ICSharpCode.AvalonEdit.Rendering
{
	/// <summary>
	/// Represents a collapsed line section.
	/// Use the Uncollapse() method to uncollapse the section.
	/// </summary>
	public sealed class CollapsedLineSection
	{
		// note: we don't need to store start/end, we could recompute them from
		// the height tree if that had parent pointers.
		DocumentLine? start, end;
		internal readonly HeightTree heightTree;
		internal HeightTreeLeafNode? startLeaf, endLeaf;
		// tree nodes that contains the start/end of the collapsed section
		internal byte startIndexInLeaf, endIndexInLeaf;
		// start/end line within the HeightTreeLeafNode

#if DEBUG
		internal string ID;
		static int nextId;
#else
		const string ID = "";
#endif

		internal CollapsedLineSection(HeightTree heightTree, DocumentLine start, DocumentLine end)
		{
			this.heightTree = heightTree;
			this.start = start;
			this.end = end;
#if DEBUG
			unchecked {
				this.ID = " #" + (nextId++);
			}
#endif
		}

		/// <summary>
		/// Gets if the document line is collapsed.
		/// This property initially is true and turns to false when uncollapsing the section.
		/// </summary>
		public bool IsCollapsed {
			get { return start != null; }
		}

		/// <summary>
		/// Gets the start line of the section.
		/// When the section is uncollapsed or the text containing it is deleted,
		/// this property returns null.
		/// </summary>
		public DocumentLine? Start {
			get { return start; }
			internal set { start = value; }
		}

		/// <summary>
		/// Gets the end line of the section.
		/// When the section is uncollapsed or the text containing it is deleted,
		/// this property returns null.
		/// </summary>
		public DocumentLine? End {
			get { return end; }
			internal set { end = value; }
		}

		internal void Reset()
		{
			start = end = null;
			startLeaf = endLeaf = null;
		}

		/// <summary>
		/// Uncollapses the section.
		/// This causes the Start and End properties to be set to null!
		/// Does nothing if the section is already uncollapsed.
		/// </summary>
		public void Uncollapse()
		{
			if (startLeaf == null || endLeaf == null)
				return;
			HeightTreeNode startNode = startLeaf;
			HeightTreeNode endNode = endLeaf;
			while (startNode != endNode) {
				startNode.RemoveEvent(this, HeightTreeNode.EventKind.Start);
				endNode.RemoveEvent(this, HeightTreeNode.EventKind.End);
				startNode.parent!.UpdateHeight(startNode.indexInParent);
				endNode.parent!.UpdateHeight(endNode.indexInParent);
				startNode = startNode.parent;
				endNode = endNode.parent;
			}
			// Now we have arrived at the node which has both events.
			startNode.RemoveEvent(this, HeightTreeNode.EventKind.Start);
			startNode.RemoveEvent(this, HeightTreeNode.EventKind.End);
			// Propagate the new height up to the root node.
			while (startNode.parent != null) {
				startNode.parent.UpdateHeight(startNode.indexInParent);
				startNode = startNode.parent;
			}

			Reset();
		}

		/// <summary>
		/// Gets a string representation of the collapsed section.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
		public override string ToString()
		{
			return "[CollapsedSection" + ID + " Start=" + (start != null ? start.LineNumber.ToString() : "null")
				+ " End=" + (end != null ? end.LineNumber.ToString() : "null") + "]";
		}

		internal bool StartIsWithin(HeightTreeNode heightTreeNode, out int index)
		{
			index = startIndexInLeaf;
			HeightTreeNode? node = startLeaf;
			while (node != null) {
				if (node == heightTreeNode)
					return true;
				index = node.indexInParent;
				node = node.parent;
			}
			return false;
		}

		internal bool EndIsWithin(HeightTreeNode heightTreeNode, out int index)
		{
			index = endIndexInLeaf;
			HeightTreeNode? node = endLeaf;
			while (node != null) {
				if (node == heightTreeNode)
					return true;
				index = node.indexInParent;
				node = node.parent;
			}
			return false;
		}
	}
}
