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
using System.Collections.Generic;
using System.Diagnostics;

namespace ICSharpCode.AvalonEdit.Rendering
{
	sealed class HeightTreeInnerNode : HeightTreeNode
	{
		internal const int MaxChildCount = 16;
		// Must be at least 3 (it must be more than MinChildCount)
		// Must be at most 16 due to the 'collapsed' bitfield.
		// Must be at most 255 because we use type byte in various places.
		// Must be even to simplify the insertion code (splitting evenly)

		internal const int MinChildCount = (MaxChildCount + 1) / 2;
		// Must be at least 2 to avoid the degenerate trees
		// Must be at most MaxChildCount/2 (rounded up) to allow node merging

		struct AggregatedData
		{
			// C# requires fixed-size arrays to appear only in structs  
			internal unsafe fixed double totalHeights[MaxChildCount];
			internal unsafe fixed int lineCounts[MaxChildCount];
			// only indices 0..childCount-1 are valid
		}
		AggregatedData data;
		internal readonly HeightTreeNode?[] children = new HeightTreeNode?[MaxChildCount];
		// invariant: children[0..childCount-1] are non-null

		public static HeightTreeInnerNode NewRoot(HeightTreeNode a, HeightTreeNode b)
		{
			var root = new HeightTreeInnerNode();
			root.children[0] = a;
			root.children[1] = b;
			root.childCount = 2;
			a.parent = root;
			a.indexInParent = 0;
			b.parent = root;
			b.indexInParent = 1;
			root.UpdateChild(0);
			root.UpdateChild(1);
			root.collapsed = root.RecomputeCollapsedBits();
			return root;
		}

		// Gets the index of the child that contains the specified line.
		internal int FindChildForLine(int line, out int lineInChild)
		{
			Debug.Assert(line >= 0);
			unsafe {
				for (int i = 0; i < childCount; i++) {
					if (line < data.lineCounts[i]) {
						lineInChild = line;
						return i;
					}
					line -= data.lineCounts[i];
				}
				// line is out of range, this might happen when inserting a line at the end of the document
				Debug.Assert(line == 0);
				if (childCount == 0) {
					throw new InvalidOperationException("childCount==0");
				}
				// return the last child
				lineInChild = line + data.lineCounts[childCount - 1];
				return childCount - 1;
			}
		}

		// Gets the index of the child that contains the specified visual position.
		// Post-condition: 0 <= result < childCount
		internal int FindChildForVisualPosition(double position, out double positionInChild)
		{
			Debug.Assert(childCount >= 1);
			double totalHeight = 0;
			unsafe { // using the safety invariant that childCount<=MaxChildCount
				for (int i = 0; i < childCount; i++) {
					if ((collapsed & (1 << i)) != 0)
						continue;
					double newTotalHeight = totalHeight + data.totalHeights[i];
					if (position < newTotalHeight) {
						positionInChild = position - totalHeight;
						return i;
					}
					totalHeight = newTotalHeight;
				}
				// Not Found: Can happen when position>totalHeight,
				// i.e. at the end of the document, or due to rounding errors.
				// In this case, return the last non-collapsed child.
				for (int i = childCount - 1; i >= 0; i--) {
					if (data.totalHeights[i] > 0 && (collapsed & (1 << i)) == 0) {
						positionInChild = data.totalHeights[i];
						return i;
					}
				}
				// If all children are collapsed, return the first child.
				positionInChild = data.totalHeights[0];
				return 0;
			}
		}

		internal override int LineCount => GetTotalLineCountUntilChildIndex(childCount);

		internal int GetTotalLineCountUntilChildIndex(int childIndex)
		{
			// To avoid memory unsafety this is not just an assertion, but a runtime check.
			if (childIndex < 0 || childIndex > childCount)
				throw new ArgumentOutOfRangeException(nameof(childIndex));
			int totalCount = 0;
			unsafe { // using the safety invariant that childCount<=MaxChildCount
				for (int i = 0; i < childIndex; i++) {
					totalCount += data.lineCounts[i];
				}
			}
			return totalCount;
		}

		internal override double TotalHeight => GetTotalHeightUntilChildIndex(childCount);

		internal double GetTotalHeightUntilChildIndex(int childIndex)
		{
			// To avoid memory unsafety this is not just an assertion, but a runtime check.
			if (childIndex < 0 || childIndex > childCount)
				throw new ArgumentOutOfRangeException(nameof(childIndex));
			double totalHeight = 0;
			unsafe {
				for (int i = 0; i < childIndex; i++) {
					if ((collapsed & (1 << i)) == 0) {
						totalHeight += data.totalHeights[i];
					}
				}
			}
			return totalHeight;
		}

		internal override void SetHeight(int line, double val)
		{
			int lineInChild;
			int childIndex = FindChildForLine(line, out lineInChild);
			children[childIndex]!.SetHeight(lineInChild, val);
			unsafe { // index already validated by array access above
				data.totalHeights[childIndex] = children[childIndex]!.TotalHeight;
			}
		}

		internal override void UpdateHeight(double oldValue, double newValue)
		{
			for (int i = 0; i < childCount; i++) {
				children[i]!.UpdateHeight(oldValue, newValue);
				unsafe { // index already validated by array access above
					data.totalHeights[i] = children[i]!.TotalHeight;
				}
			}
		}

		internal override bool GetIsCollapsed(int line)
		{
			int childIndex = FindChildForLine(line, out int lineInChild);
			if ((collapsed & (1 << childIndex)) != 0) {
				return true;
			} else {
				return children[childIndex]!.GetIsCollapsed(lineInChild);
			}
		}

		internal override HeightTreeNode? InsertLine(int line, double height)
		{
			int childIndex = FindChildForLine(line, out int lineInChild);
			HeightTreeNode? newChild = children[childIndex]!.InsertLine(lineInChild, height);
			bool needRecomputeCollapsed = UpdateChild(childIndex);
			HeightTreeInnerNode? newSibling = null;
			if (newChild != null) {
				// child was split, insert newChild into this node
				if (childCount == MaxChildCount) {
					// this node is full, split it
					newSibling = new HeightTreeInnerNode();
					int splitIndex = MaxChildCount / 2;
					newSibling.StealFromPredecessor(this, childCount - splitIndex);
					// insert newChild into this node or newSibling
					if (childIndex < splitIndex) {
						needRecomputeCollapsed |= InsertChild(childIndex + 1, newChild);
					} else {
						newSibling.InsertChild(childIndex + 1 - splitIndex, newChild);
						newSibling.collapsed = newSibling.RecomputeCollapsedBits();
					}
				} else {
					// this node is not full, insert newChild
					needRecomputeCollapsed |= InsertChild(childIndex + 1, newChild);
				}
			}
			if (needRecomputeCollapsed) {
				collapsed = RecomputeCollapsedBits();
			}
			return newSibling;
		}

		// Updates information cached from the child node.
		// Returns whether the collapsed bits will need to be recomputed.
		bool UpdateChild(int childIndex)
		{
			Debug.Assert(0 <= childIndex && childIndex < childCount);
			var child = children[childIndex]!;
			Debug.Assert(child.parent == this && child.indexInParent == childIndex);
			unsafe { // index already validated by array access above
				data.totalHeights[childIndex] = child.TotalHeight;
				data.lineCounts[childIndex] = child.LineCount;
			}
			// First, clear out the events belonging to this child so that we can propagate
			// them again from scratch.
			bool needRecomputeCollapsed = false;
			int remainingEvents = 0;
			if (events != null) {
				for (int i = 0; i < events.Length; i++) {
					if (events[i].Position == childIndex) {
						events[i].Section = null;
						needRecomputeCollapsed = true;
					} else if (events[i].Section != null) {
						remainingEvents++;
					}
				}
			}
			// Propagate events from child to this node.
			if (child.events != null) {
				int outputIndex = 0;
				foreach (Event e in child.events) {
					if (e.Section == null)
						continue;
					// Ignore events if the partner event is contained in the same child.
					if (e.Kind == EventKind.Start) {
						if (e.Section.EndIsWithin(child, out _))
							continue;
					} else {
						Debug.Assert(e.Kind == EventKind.End);
						if (e.Section.StartIsWithin(child, out _))
							continue;
					}
					InsertEvent(ref outputIndex, new Event {
						Kind = e.Kind,
						Section = e.Section,
						Position = (byte)childIndex
					});
					needRecomputeCollapsed = true;
					remainingEvents++;
				}
			}
			CompactifyEvents(remainingEvents);
			return needRecomputeCollapsed;
		}

		internal void UpdateHeight(int childIndex)
		{
			Debug.Assert(0 <= childIndex && childIndex < childCount);
			var child = children[childIndex]!;
			Debug.Assert(child.parent == this && child.indexInParent == childIndex);
			unsafe { // index already validated by array access above
				data.totalHeights[childIndex] = child.TotalHeight;
			}
		}

		internal bool InsertChild(int childIndex, HeightTreeNode newChild)
		{
			MakeGapForInsertion(childIndex, 1);
			children[childIndex] = newChild;
			newChild.parent = this;
			newChild.indexInParent = (byte)childIndex;
			return UpdateChild(childIndex);
		}

		void MakeGapForInsertion(int childIndex, int amount)
		{
			// Move all children >=childIndex up by amount.
			Debug.Assert(0 <= childIndex && childIndex <= childCount);
			Debug.Assert(childCount + amount <= MaxChildCount);
			for (int i = childCount - 1; i >= childIndex; i--) {
				children[i + amount] = children[i];
				unsafe { // index already validated by array access above
					data.totalHeights[i + amount] = data.totalHeights[i];
					data.lineCounts[i + amount] = data.lineCounts[i];
				}
				children[i + amount]!.indexInParent = (byte)(i + amount);
			}
			childCount += (byte)amount;
			AdjustEventPositions(childIndex, amount, deleteAffectedEvents: false);
		}

		internal override DeletionResults DeleteLine(int line, HeightTreeNode? predecessor, HeightTreeNode? successor)
		{
			int childIndex = FindChildForLine(line, out int lineInChild);
			HeightTreeInnerNode? predecessorParent;
			int predecessorChildIndex;
			if (childIndex > 0) {
				predecessorParent = this;
				predecessorChildIndex = childIndex - 1;
			} else {
				predecessorParent = (HeightTreeInnerNode?)predecessor;
				predecessorChildIndex = predecessorParent?.childCount - 1 ?? 0;
			}
			HeightTreeInnerNode? successorParent;
			int successorChildIndex;
			if (childIndex < childCount - 1) {
				successorParent = this;
				successorChildIndex = childIndex + 1;
			} else {
				successorParent = (HeightTreeInnerNode?)successor;
				successorChildIndex = 0;
			}
			DeletionResults childResults = children[childIndex]!.DeleteLine(lineInChild, predecessorParent?.children[predecessorChildIndex], successorParent?.children[successorChildIndex]);
			DeletionResults results = DeletionResults.None;
			bool needsRecomputeCollapsed = false;
			if ((childResults & DeletionResults.PredecessorChanged) != 0) {
				bool predRecomputeCollapsed = predecessorParent!.UpdateChild(predecessorChildIndex);
				if (predecessorParent == predecessor) {
					results |= DeletionResults.PredecessorChanged;
					if (predRecomputeCollapsed) {
						predecessor.collapsed = predecessor.RecomputeCollapsedBits();
					}
				} else {
					needsRecomputeCollapsed |= predRecomputeCollapsed;
				}
			}
			if ((childResults & DeletionResults.SuccessorChanged) != 0) {
				bool succRecomputeCollapsed = successorParent!.UpdateChild(successorChildIndex);
				if (successorParent == successor) {
					results |= DeletionResults.SuccessorChanged;
					if (succRecomputeCollapsed) {
						successor.collapsed = successor.RecomputeCollapsedBits();
					} 
				} else {
					needsRecomputeCollapsed |= succRecomputeCollapsed;
				}
			}
			if ((childResults & DeletionResults.NodeDeleted) != 0) {
				Debug.Assert(children[childIndex]!.LineCount == 0);  // child must be empty
				PerformDeletion(childIndex, childIndex + 1);
				needsRecomputeCollapsed = true;
				// After removing a child from this inner node, it is possible that we need to rebalance the inner nodes
				if (childCount < MinChildCount) {
					var prev = (HeightTreeInnerNode?)predecessor;
					var next = (HeightTreeInnerNode?)successor;
					// Try to steal lines from our siblings
					if (prev != null && prev.childCount > MinChildCount && prev.childCount > (next?.childCount ?? 0)) {
						StealFromPredecessor(prev, (prev.childCount - MinChildCount + 1) / 2);
						results |= DeletionResults.PredecessorChanged;
						Debug.Assert(childCount >= MinChildCount);
						Debug.Assert(prev.childCount >= MinChildCount);
						// StealFromPredecessor already recomputed 'collapsed'
						needsRecomputeCollapsed = false;
					} else if (next != null && next.childCount > MinChildCount) {
						StealFromSuccessor(next, (next.childCount - MinChildCount + 1) / 2);
						results |= DeletionResults.SuccessorChanged;
						Debug.Assert(childCount >= MinChildCount);
						Debug.Assert(next.childCount >= MinChildCount);
						// StealFromPredecessor already recomputed 'collapsed'
						needsRecomputeCollapsed = false;
					} else if (prev != null) {
						// Merge into predecessor
						prev.StealFromSuccessor(this, childCount);
						results |= DeletionResults.PredecessorChanged | DeletionResults.NodeDeleted;
						// Don't need to recompute collapsed for a node about to be deleted
						needsRecomputeCollapsed = false;
					} else if (next != null) {
						// Merge into successor
						next.StealFromPredecessor(this, childCount);
						results |= DeletionResults.SuccessorChanged | DeletionResults.NodeDeleted;
						// Don't need to recompute collapsed for a node about to be deleted
						needsRecomputeCollapsed = false;
					}
				}
			} else {
				needsRecomputeCollapsed |= UpdateChild(childIndex);
			}
			if (needsRecomputeCollapsed) {
				collapsed = RecomputeCollapsedBits();
			}
			return results;
		}

		private void PerformDeletion(int start, int end)
		{
			Debug.Assert(0 <= start && start <= end && end <= childCount);
			int length = end - start;
			childCount -= (byte)length;
			for (int i = start; i < childCount; i++) {
				children[i] = children[i + length];
				unsafe { // index already validated by array access above
					data.totalHeights[i] = data.totalHeights[i + length];
					data.lineCounts[i] = data.lineCounts[i + length];
				}
				children[i]!.indexInParent = (byte)i;
			}
			for (int i = 0; i < length; i++) {
				children[childCount + i] = null;
			}
			AdjustEventPositions(start, -length, deleteAffectedEvents: true);
		}

		void StealFromPredecessor(HeightTreeInnerNode prev, int childrenToMove)
		{
			if (childrenToMove > prev.childCount || childCount + childrenToMove > MaxChildCount)
				throw new ArgumentOutOfRangeException(nameof(childrenToMove));
			MakeGapForInsertion(0, childrenToMove);
			// steal children
			for (int i = 0; i < childrenToMove; i++) {
				children[i] = prev.children[prev.childCount - childrenToMove + i];
				unsafe { // index already validated by array access above
					data.totalHeights[i] = prev.data.totalHeights[prev.childCount - childrenToMove + i];
					data.lineCounts[i] = prev.data.lineCounts[prev.childCount - childrenToMove + i];
				}
				children[i]!.parent = this;
				children[i]!.indexInParent = (byte)i;
			}
			StealEvents(prev, prev.childCount - childrenToMove, prev.childCount, 0);
			// update child count
			prev.childCount -= (byte)childrenToMove;
			for (int i = 0; i < childrenToMove; i++) {
				prev.children[prev.childCount + i] = null;
			}
			// Because 'collapsed' only considers events local to the node, and we might
			// have moved a collapsed section start from the predecessor to this node,
			// this might also change the 'collapsed' status of the existing lines within
			// this node. So fully recompute to be safe.
			collapsed = RecomputeCollapsedBits();
			prev.collapsed = prev.RecomputeCollapsedBits();
		}

		void StealFromSuccessor(HeightTreeInnerNode next, int childrenToMove)
		{
			if (childrenToMove > next.childCount || childCount + childrenToMove > MaxChildCount)
				throw new ArgumentOutOfRangeException(nameof(childrenToMove));
			// steal children
			for (int i = 0; i < childrenToMove; i++) {
				children[childCount + i] = next.children[i];
				unsafe { // index already validated by array access above
					data.totalHeights[childCount + i] = next.data.totalHeights[i];
					data.lineCounts[childCount + i] = next.data.lineCounts[i];
				}
				children[childCount + i]!.parent = this;
				children[childCount + i]!.indexInParent = (byte)(childCount + i);
			}
			StealEvents(next, 0, childrenToMove, childCount);
			// update child count
			childCount += (byte)childrenToMove;
			next.PerformDeletion(0, childrenToMove);
			collapsed = RecomputeCollapsedBits();
			next.collapsed = next.RecomputeCollapsedBits();
		}

		internal void RebalanceLastChild()
		{
			// special rebalancing when building a new tree in RebuildDocument
			// all nodes except the last are guaranteed to be full
			// the last node itself might be empty
			Debug.Assert(childCount >= 2);
			var lastChild = children[childCount - 1]!;
			if (lastChild is HeightTreeInnerNode lastInner) {
				if (lastInner.childCount < HeightTreeInnerNode.MinChildCount) {
					var prevInner = (HeightTreeInnerNode)children[childCount - 2]!;
					int balancedCount = (prevInner.childCount + lastInner.childCount) / 2;
					lastInner.StealFromPredecessor(prevInner, balancedCount - lastInner.childCount);
					UpdateChild(childCount - 2);
				}
				lastInner.RebalanceLastChild();
				UpdateChild(childCount - 1);
			} else {
				var lastLeaf = (HeightTreeLeafNode)lastChild;
				if (lastLeaf.LineCount < HeightTreeLeafNode.MinLineCount) {
					var prevLeaf = (HeightTreeLeafNode)children[childCount - 2]!;
					int balancedCount = (prevLeaf.LineCount + lastLeaf.LineCount) / 2;
					lastLeaf.StealFromPredecessor(prevLeaf, balancedCount - lastLeaf.LineCount);
					UpdateChild(childCount - 2);
					UpdateChild(childCount - 1);
				}
			}
			collapsed = RecomputeCollapsedBits();
		}

		internal override void AddCollapsedSection(int start, int end, CollapsedLineSection section)
		{
			Debug.Assert(start <= end); // start+end are both inclusive
			int lineCount = this.LineCount;
			bool startsHere = (0 <= start && start < lineCount);
			bool endsHere = (0 <= end && end < lineCount);
			Debug.Assert(startsHere || endsHere);
			int outputIndex = 0;
			if (startsHere && endsHere) {
				int startIndex = FindChildForLine(start, out int startInChild);
				int endIndex = FindChildForLine(end, out int endInChild);
				if (startIndex == endIndex) {
					// collapsed section can be fully handled by our child node
					children[startIndex]!.AddCollapsedSection(startInChild, endInChild, section);
					UpdateHeight(startIndex);
					return;
				}
				Debug.Assert(startIndex < endIndex);
				children[startIndex]!.AddCollapsedSection(startInChild, end + (startInChild - start), section);
				UpdateHeight(startIndex);
				InsertEvent(ref outputIndex, new Event {
					Section = section,
					Kind = EventKind.Start,
					Position = (byte)startIndex
				});
				children[endIndex]!.AddCollapsedSection(start + (endInChild - end), endInChild, section);
				UpdateHeight(endIndex);
				InsertEvent(ref outputIndex, new Event {
					Section = section,
					Kind = EventKind.End,
					Position = (byte)endIndex
				});
			} else if (startsHere) {
				int startIndex = FindChildForLine(start, out int startInChild);
				children[startIndex]!.AddCollapsedSection(startInChild, end + (startInChild - start), section);
				UpdateHeight(startIndex);
				InsertEvent(ref outputIndex, new Event {
					Section = section,
					Kind = EventKind.Start,
					Position = (byte)startIndex
				});
			} else {
				int endIndex = FindChildForLine(end, out int endInChild);
				children[endIndex]!.AddCollapsedSection(start + (endInChild - end), endInChild, section);
				UpdateHeight(endIndex);
				InsertEvent(ref outputIndex, new Event {
					Section = section,
					Kind = EventKind.End,
					Position = (byte)endIndex
				});
			}
			collapsed = RecomputeCollapsedBits();
		}

		internal override IEnumerable<CollapsedLineSection> GetAllCollapsedSections(EventKind kind)
		{
			for (int i = 0; i < childCount; i++) {
				foreach (var section in children[i]!.GetAllCollapsedSections(kind))
					yield return section;
			}
		}

#if DEBUG
		internal override void AppendTreeToString(System.Text.StringBuilder b, int indent, int lineNumber)
		{
			b.AppendFormat("inner (childCount={0}, LineCount={1}, TotalHeight={2}, collapsed={3:x})", childCount, LineCount, TotalHeight, collapsed);
			b.AppendLine();
			indent += 2;
			unsafe {
				for (int i = 0; i < childCount; i++) {
					b.Append(' ', indent);
					b.Append($"[{i}] ");
					if (children[i] == null) {
						b.AppendLine("null");
					} else {
						children[i]!.AppendTreeToString(b, indent, lineNumber);
					}
					lineNumber += data.lineCounts[i];
				}
			}
			AppendEventsToString(b, indent);
		}

		internal override void CheckInvariant(bool isRoot, int lineNumber)
		{
			base.CheckInvariant(isRoot, lineNumber);
			Debug.Assert(childCount <= MaxChildCount);
			if (isRoot) {
				Debug.Assert(childCount >= 2);
			} else {
				Debug.Assert(childCount >= MinChildCount);
			}
			int lineNumberInChild = lineNumber;
			for (int i = 0; i < childCount; i++) {
				children[i]!.CheckInvariant(false, lineNumberInChild);
				unsafe {
					lineNumberInChild += data.lineCounts[i];
					Debug.Assert(children[i]!.TotalHeight == data.totalHeights[i]);
					Debug.Assert(children[i]!.LineCount == data.lineCounts[i]);
				}
			}
			foreach (var e in events ?? Array.Empty<Event>()) {
				if (e.Section != null) {
					Debug.Assert(e.Position < childCount);
					var eventLine = e.Kind == EventKind.Start ? e.Section.Start : e.Section.End;
					int start = lineNumber + GetTotalLineCountUntilChildIndex(e.Position);
					int end = lineNumber + GetTotalLineCountUntilChildIndex(e.Position + 1);
					Debug.Assert(start <= eventLine!.LineNumber && eventLine!.LineNumber < end);
				}
			}
		}
#endif
	}
}
