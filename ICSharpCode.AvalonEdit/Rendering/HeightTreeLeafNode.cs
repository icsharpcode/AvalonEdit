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
	sealed class HeightTreeLeafNode : HeightTreeNode
	{
		internal const int MaxLineCount = 16;
		// Must be at least 3 (it must be more than MinChildCount)
		// Must be at most 16 due to the 'collapsed' bitfield.
		// Must be at most 255 because we use type byte in various places.
		// Must be even to simplify the insertion code (splitting evenly)

		internal const int MinLineCount = (MaxLineCount + 1) / 2;
		// Must be at least 2 to avoid the degenerate trees
		// Must be at most MaxChildCount/2 (rounded up) to allow node merging

		struct LineData
		{
			// C# requires fixed-size arrays to appear only in structs  
			internal unsafe fixed double heights[MaxLineCount];
			// only indices 0..lineCount-1 are valid
		}
		LineData data;

		internal static HeightTreeLeafNode Create(int lineCount, double defaultLineHeight)
		{
			if (lineCount > MaxLineCount)
				throw new ArgumentOutOfRangeException(nameof(lineCount));
			HeightTreeLeafNode leaf = new HeightTreeLeafNode();
			unsafe {
				for (int i = 0; i < lineCount; i++) {
					leaf.data.heights[i] = defaultLineHeight;
				}
				leaf.childCount = (byte)lineCount;
			}
			return leaf;
		}

		internal override int LineCount => childCount;

		internal override double TotalHeight => GetTotalHeightUntilChildIndex(childCount);

		internal double GetTotalHeightUntilChildIndex(int childIndex)
		{
			// To avoid memory unsafety this is not just an assertion, but a runtime check.
			if (childIndex < 0 || childIndex > childCount)
				throw new ArgumentOutOfRangeException(nameof(childIndex));
			double totalHeight = 0;
			unsafe { // using the safety invariant that childIndex<=lineCount<=MaxLineCount
				for (int i = 0; i < childIndex; i++) {
					if ((collapsed & (1 << i)) == 0) {
						totalHeight += data.heights[i];
					}
				}
			}
			return totalHeight;
		}

		internal int FindChildForVisualPosition(double position)
		{
			double totalHeight = 0;
			unsafe { // using the safety invariant that lineCount<=MaxLineCount
				for (int i = 0; i < childCount; i++) {
					if ((collapsed & (1 << i)) == 0) {
						totalHeight += data.heights[i];
						if (position < totalHeight)
							return i;
					}
				}
			}
			// Not Found: Can happen when position>totalHeight,
			// i.e. at the end of the document, or due to rounding errors.
			// In this case, return the last non-collapsed child.
			for (int i = childCount - 1; i >= 0; i--) {
				if ((collapsed & (1 << i)) == 0) {
					return i;
				}
			}
			// If all children are collapsed, return the first child.
			return 0;
		}

		public double GetHeight(int line)
		{
			// To avoid memory unsafety this is not just an assertion, but a runtime check.
			if (line < 0 || line >= childCount)
				throw new ArgumentOutOfRangeException(nameof(line));
			unsafe {
				return data.heights[line];
			}
		}

		internal override void SetHeight(int line, double val)
		{
			// To avoid memory unsafety this is not just an assertion, but a runtime check.
			if (line < 0 || line >= childCount)
				throw new ArgumentOutOfRangeException(nameof(line));
			unsafe {
				data.heights[line] = val;
			}
		}

		internal override void UpdateHeight(double oldValue, double newValue)
		{
			unsafe { // using the safety invariant that lineCount<=MaxLineCount
				for (int i = 0; i < childCount; i++) {
					if (data.heights[i] == oldValue) {
						data.heights[i] = newValue;
					}
				}
			}
		}

		internal override bool GetIsCollapsed(int line)
		{
			Debug.Assert(line >= 0 && line < childCount);
			return (collapsed & (1 << line)) != 0;
		}

		internal unsafe override HeightTreeNode? InsertLine(int line, double height)
		{
			// To avoid memory unsafety this is not just an assertion, but a runtime check.
			if (line < 0 || line > childCount)
				throw new ArgumentOutOfRangeException(nameof(line));
			HeightTreeLeafNode? newLeaf = null;
			if (childCount == MaxLineCount) {
				// split leaf node
				newLeaf = new HeightTreeLeafNode();
				int splitIndex = MaxLineCount / 2;
				newLeaf.StealFromPredecessor(this, childCount - splitIndex);
				// now an insertion will be possible without splitting
				if (line >= splitIndex) {
					newLeaf.InsertLine(line - splitIndex, height);
					return newLeaf;
				}
			}
			MakeGapForInsertion(line, 1);
			// insert new line
			data.heights[line] = height;
			return newLeaf;
		}

		private void MakeGapForInsertion(int line, int amount)
		{
			// Move all elements >=line up by amount.
			Debug.Assert(line >= 0 && line <= childCount);
			if (childCount + amount > MaxLineCount) {
				throw new ArgumentOutOfRangeException(nameof(amount));
			}
			unsafe { // checked in if above
				for (int i = childCount - 1; i >= line; i--) {
					data.heights[i + amount] = data.heights[i];
				}
			}
			childCount += (byte)amount;
			AdjustEventPositions(line, amount, deleteAffectedEvents: false);
		}

		internal override DeletionResults DeleteLine(int line, HeightTreeNode? predecessor, HeightTreeNode? successor)
		{
			// To avoid memory unsafety this is not just an assertion, but a runtime check.
			if (line < 0 || line >= childCount)
				throw new ArgumentOutOfRangeException(nameof(line));
			DeletionResults results = AdjustEventsOnLine(line, predecessor, successor);
			PerformDeletion(line, line + 1);
			if (childCount >= MinLineCount) {
				return results;
			}
			var prev = (HeightTreeLeafNode?)predecessor;
			var next = (HeightTreeLeafNode?)successor;
			// Try to steal lines from our siblings
			if (prev != null && prev.childCount > MinLineCount && prev.childCount > (next?.childCount ?? 0)) {
				StealFromPredecessor(prev, (prev.childCount - MinLineCount + 1) / 2);
				Debug.Assert(childCount >= MinLineCount);
				Debug.Assert(prev.childCount >= MinLineCount);
				results |= DeletionResults.PredecessorChanged;
			} else if (next != null && next.childCount > MinLineCount) {
				StealFromSuccessor(next, (next.childCount - MinLineCount + 1) / 2);
				Debug.Assert(childCount >= MinLineCount);
				Debug.Assert(next.childCount >= MinLineCount);
				results |= DeletionResults.SuccessorChanged;
			} else if (prev != null) {
				// Merge into predecessor
				prev.StealFromSuccessor(this, childCount);
				results |= DeletionResults.PredecessorChanged | DeletionResults.NodeDeleted;
			} else if (next != null) {
				// Merge into successor
				next.StealFromPredecessor(this, childCount);
				results |= DeletionResults.SuccessorChanged | DeletionResults.NodeDeleted;
			}
			return results;
		}

		private DeletionResults AdjustEventsOnLine(int line, HeightTreeNode? predecessor, HeightTreeNode? successor)
		{
			// Handle sections that start or end directly on line
			if (events == null)
				return DeletionResults.None;
			DeletionResults results = DeletionResults.None;
			List<CollapsedLineSection>? removedSections = null;
			int successorOutputIndex = 0;
			int predecessorOutputIndex = 0;
			for (int i = 0; i < events.Length; i++) {
				ref var ev = ref events[i];
				if (ev.Position != line || ev.Section == null)
					continue;
				// This section starts or ends directly on line, so we need to move it.
				var section = ev.Section;
				if (section.Start == section.End) {
					// This is a single-line section, so we uncollapse it completely.
					// Because both start and end are local to this node, we can just remove the section
					// without going through the full Uncollapse() logic.
					ev.Section = null;
					// We can't call section.Reset() yet because we first need to delete the other event.
					removedSections ??= new List<CollapsedLineSection>();
					removedSections.Add(section);
				} else if (ev.Kind == EventKind.Start) {
					section.Start = section.Start!.NextLine;
					if (line + 1 < childCount) {
						ev.Position++;
						section.startIndexInLeaf = ev.Position;
					} else {
						// Move event to successor
						successor!.InsertEvent(ref successorOutputIndex, new Event {
							Position = 0,
							Kind = EventKind.Start,
							Section = section
						});
						section.startLeaf = (HeightTreeLeafNode)successor;
						section.startIndexInLeaf = 0;
						ev.Section = null;
						successor.collapsed = successor.RecomputeCollapsedBits();
						results |= DeletionResults.SuccessorChanged;
					}
				} else {
					section.End = section.End!.PreviousLine;
					if (line > 0) {
						ev.Position--;
						section.endIndexInLeaf = ev.Position;
					} else {
						// Move event to predecessor
						predecessor!.InsertEvent(ref predecessorOutputIndex, new Event {
							Position = (byte)(predecessor.childCount - 1),
							Kind = EventKind.End,
							Section = section
						});
						section.endLeaf = (HeightTreeLeafNode)predecessor;
						section.endIndexInLeaf = (byte)(predecessor.childCount - 1);
						ev.Section = null;
						predecessor.collapsed = predecessor.RecomputeCollapsedBits();
						results |= DeletionResults.PredecessorChanged;
					}
				}
			}
			// Note: we don't need to update the collapsed bits of this node, because the changed line  
			// is about to be deleted.
			if (removedSections != null) {
				foreach (var section in removedSections) {
					section.Reset();
				}
			}
			return results;
		}

		private void PerformDeletion(int start, int end)
		{
			Debug.Assert(0 <= start && start <= end && end <= childCount);
			// shift data to remove the lines
			byte length = (byte)(end - start);
			childCount -= length;
			unsafe {
				for (int i = start; i < childCount; i++) {
					data.heights[i] = data.heights[i + length];
				}
			}
			AdjustEventPositions(start, -length, deleteAffectedEvents: false);
		}

		internal unsafe void StealFromPredecessor(HeightTreeLeafNode prev, int linesToMove)
		{
			if (linesToMove > prev.childCount || childCount + linesToMove > MaxLineCount)
				throw new ArgumentOutOfRangeException(nameof(linesToMove));
			MakeGapForInsertion(0, linesToMove);
			// steal lines
			for (int i = 0; i < linesToMove; i++) {
				data.heights[i] = prev.data.heights[prev.childCount - linesToMove + i];
			}
			StealEvents(prev, prev.childCount - linesToMove, prev.childCount, 0);
			// update line count
			prev.childCount -= (byte)linesToMove;
			// Because 'collapsed' only considers events local to the node, and we might
			// have moved a collapsed section start from the predecessor to this node,
			// this might also change the 'collapsed' status of the existing lines within
			// this node. So fully recompute to be safe.
			collapsed = RecomputeCollapsedBits();
			prev.collapsed = prev.RecomputeCollapsedBits();
		}

		internal unsafe void StealFromSuccessor(HeightTreeLeafNode next, int linesToMove)
		{
			if (linesToMove > next.childCount || childCount + linesToMove > MaxLineCount)
				throw new ArgumentOutOfRangeException(nameof(linesToMove));
			// steal lines
			for (int i = 0; i < linesToMove; i++) {
				data.heights[childCount + i] = next.data.heights[i];
			}
			StealEvents(next, 0, linesToMove, childCount);
			// update line count
			childCount += (byte)linesToMove;
			next.PerformDeletion(0, linesToMove);
			collapsed = RecomputeCollapsedBits();
			next.collapsed = next.RecomputeCollapsedBits();
		}

		internal override void AddCollapsedSection(int start, int end, CollapsedLineSection section)
		{
			Debug.Assert(start <= end); // start+end are both inclusive
			bool startsHere = (0 <= start && start < childCount);
			bool endsHere = (0 <= end && end < childCount);
			Debug.Assert(startsHere || endsHere);
			events ??= new Event[2];
			int outputIndex = 0;
			// prepend to linked lists
			if (startsHere) {
				section.startLeaf = this;
				section.startIndexInLeaf = (byte)start;
				InsertEvent(ref outputIndex, new Event {
					Section = section,
					Kind = EventKind.Start,
					Position = (byte)start
				});
			}
			if (endsHere) {
				section.endLeaf = this;
				section.endIndexInLeaf = (byte)end;
				InsertEvent(ref outputIndex, new Event {
					Section = section,
					Kind = EventKind.End,
					Position = (byte)end
				});
			}
			collapsed = RecomputeCollapsedBits();
		}

		internal override IEnumerable<CollapsedLineSection> GetAllCollapsedSections(EventKind kind)
		{
			if (events == null)
				yield break;
			foreach (Event e in events) {
				if (e.Kind == kind && e.Section != null)
					yield return e.Section;
			}
		}

#if DEBUG
		internal override void AppendTreeToString(System.Text.StringBuilder b, int indent, int lineNumber)
		{
			b.AppendFormat("leaf (LineCount={0}, TotalHeight={1})", childCount, TotalHeight);
			b.AppendLine();
			unsafe {
				for (int i = 0; i < childCount; i++) {
					b.Append(' ', indent + 2);
					b.AppendFormat("[{0}] @{1} height={2}, collapsed={3}",
								  i, lineNumber + i, data.heights[i], (collapsed & (1 << i)) != 0);
					b.AppendLine();
				}
			}
			AppendEventsToString(b, indent + 2);
		}

		internal override void CheckInvariant(bool isRoot, int lineNumber)
		{
			Debug.Assert(childCount <= MaxLineCount);
			if (!isRoot) {
				Debug.Assert(childCount >= MinLineCount);
			}
			base.CheckInvariant(isRoot, lineNumber);
			foreach (var e in events ?? Array.Empty<Event>()) {
				if (e.Section != null) {
					Debug.Assert(e.Position < childCount);
					var line = e.Kind == EventKind.Start ? e.Section.Start : e.Section.End;
					Debug.Assert(line!.LineNumber == lineNumber + e.Position);
				}
			}
		}
#endif
	}
}