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
using System.Text;

namespace ICSharpCode.AvalonEdit.Rendering
{
	/// <summary>
	/// A node in the text view's height tree.
	/// </summary>
	abstract class HeightTreeNode
	{
		internal enum EventKind : byte { Start, End };
		internal struct Event
		{
			public CollapsedLineSection? Section;
			public EventKind Kind;
			public byte Position;
			// In leaf nodes, Position is actual (inclusive) starts/end index.
			// In inner nodes, Position is the child index of the leaf that contains the actual start/end.
			// This means for the purpose of computing the `collapsed` bitfield, Position acts as
			// exclusive start/end within inner nodes.
			// In all cases, Position is the index of the node with which the event is associated
			// for the purpose of moving when lines are inserted or deleted.
		}

		internal HeightTreeInnerNode? parent;

		internal Event[]? events;
		// Holds start/end events for collapsed sections.
		// Inside leaf nodes, each collapsed section has exactly one start event and one end event.
		// If the collapsed section starts and ends in different leaf nodes, 
		// it additionally has start+end events in the parent level of inner nodes.
		// As long as these events are in different inner nodes, the section also has
		// start+end events in the grandparent level, and so on.

		internal ushort collapsed;
		// bit (1 << 0) = child 0, bit (1 << 1) = child 1, etc.
		// Used to determine if the child should be excluded when computing the total height.
		// Only considers CollapsedLineSections that start or end within this node.

		internal byte childCount;
		// safety-critical invariant: 0 <= childCount <= Max...Count
		// Usually childCount>=Min..Count, but it can be less during a deletion operation or for the root node.
		// For leaf nodes, childCount is the number of lines stored in the node.

		internal byte indexInParent;
		// invariant: parent is null or parent.children[indexInParent] == this

		/// <summary>
		/// The total height of all lines in the node.
		/// The height of a line is not counted if it is collapsed by a section
		/// starting or ending in this node.
		/// </summary>
		internal abstract double TotalHeight { get; }

		/// <summary>
		/// The number of lines represented by this node.
		/// Collapsed lines are included in the count.
		/// </summary>
		internal abstract int LineCount { get; }

		internal abstract void SetHeight(int line, double val);
		internal abstract void UpdateHeight(double oldValue, double newValue);

		/// <summary>
		/// Inserts a new line into the tree.
		/// Returns null if the insertion was possible without splitting this node. 
		/// Otherwise, returns a new node that contains the lines that were split off.
		/// </summary>
		internal abstract HeightTreeNode? InsertLine(int line, double height);
		[Flags]
		internal enum DeletionResults : byte
		{
			None = 0,
			NodeDeleted = 1, // this node now is empty and needs to be deleted from its parent
			PredecessorChanged = 2, // lines were moved from/to the predecessor node, parent needs to update aggregated information
			SuccessorChanged = 4 // lines were moved from/to the successor node, parent needs to update aggregated information
		}
		internal abstract DeletionResults DeleteLine(int line, HeightTreeNode? predecessor, HeightTreeNode? successor);
		internal abstract bool GetIsCollapsed(int line);
		internal abstract void AddCollapsedSection(int v, int length, CollapsedLineSection section);
		internal abstract IEnumerable<CollapsedLineSection> GetAllCollapsedSections(EventKind kind);

#if DEBUG
		internal abstract void AppendTreeToString(StringBuilder b, int indent, int lineNumber);
		protected void AppendEventsToString(StringBuilder b, int indent)
		{
			if (events != null) {
				for (int i = 0; i < events.Length; i++) {
					if (events[i].Section == null)
						continue;
					b.Append(' ', indent);
					b.Append(events[i].Kind);
					b.Append(' ');
					b.Append(events[i].Position);
					b.Append(' ');
					b.Append(events[i].Section);
					b.AppendLine();
				}
			}
		}

		public override string ToString()
		{
			var b = new StringBuilder();
			AppendTreeToString(b, 0, 0);
			return b.ToString();
		}

		internal virtual void CheckInvariant(bool isRoot, int lineNumber)
		{
			if (isRoot) {
				Debug.Assert(parent == null);
			} else {
				Debug.Assert(parent != null && parent.children[indexInParent] == this);
			}
			Debug.Assert(collapsed == RecomputeCollapsedBits());
		}
#endif

		protected void AdjustEventPositions(int index, int delta, bool deleteAffectedEvents)
		{
			// Adjust the positions of all events that are after index.
			// Positive delta means insertion, negative delta means deletion.
			if (events == null)
				return;
			bool inLeaf = this is HeightTreeLeafNode;
			for (int i = 0; i < events.Length; i++) {
				ref Event e = ref events[i];
				if (e.Position >= index && e.Section != null) {
					if (e.Position < index - delta) {
						// can only happen for negative delta (=deletion)
						// when the event is in the deleted region
						Debug.Assert(deleteAffectedEvents);
						e.Section = null;
						continue;
					}
					byte newPosition = (byte)(e.Position + delta);
					if (inLeaf) {
						if (e.Kind == EventKind.Start) {
							Debug.Assert(e.Section.startLeaf == this);
							Debug.Assert(e.Section.startIndexInLeaf == e.Position);
							e.Section.startIndexInLeaf = newPosition;
						} else {
							Debug.Assert(e.Section.endLeaf == this);
							Debug.Assert(e.Section.endIndexInLeaf == e.Position);
							e.Section.endIndexInLeaf = newPosition;
						}
					}
					e.Position = newPosition;
				}
			}
			// Update collapsed
			if (delta < 0) {
				ushort maskBefore = (ushort)((1 << index) - 1);
				ushort maskAfter = (ushort)~((1 << (index - delta)) - 1);
				collapsed = (ushort)((collapsed & maskBefore) | ((collapsed & maskAfter) >> -delta));
				// cannot assert collapsed == RecomputeCollapsedBits() here because we might
				// be inside InnerNode.DeleteLine() with an outstanding needsRecomputeCollapsed.
			} else {
				// We need to check the individual sections to see if the new line is collapsed, 
				// so we might as well recompute all the collapsed bits.
				collapsed = RecomputeCollapsedBits();
			}
		}

		internal void InsertEvent(ref int outputIndex, Event e)
		{
			events ??= new Event[2];
			while (outputIndex < events.Length && events[outputIndex].Section != null)
				outputIndex++;
			if (outputIndex == events.Length)
				Array.Resize(ref events, events.Length * 2);
			events[outputIndex++] = e;
		}

		protected void StealEvents(HeightTreeNode sibling, int start, int end, int startHere)
		{
			Debug.Assert(0 <= start && start <= end && end <= sibling.childCount);
			// Move all collapsed sections starting/ending at a child between start and end
			// from sibling to this node.
			if (sibling.events == null)
				return;
			var thisAsLeaf = this as HeightTreeLeafNode;
			int outputIndex = 0;
			int remainingEventsInSibling = 0;
			for (int i = 0; i < sibling.events.Length; i++) {
				Event e = sibling.events[i];
				if (e.Section == null)
					continue;
				Debug.Assert(e.Position < sibling.childCount);
				if (start <= e.Position && e.Position < end) {
					// move e to this node.
					byte newPosition = (byte)(e.Position - start + startHere);
					if (thisAsLeaf != null) {
						// update the section's leaf references
						if (e.Kind == EventKind.Start) {
							Debug.Assert(e.Section.startLeaf == sibling);
							Debug.Assert(e.Section.startIndexInLeaf == e.Position);
							e.Section.startLeaf = thisAsLeaf;
							e.Section.startIndexInLeaf = newPosition;
						} else {
							Debug.Assert(e.Section.endLeaf == sibling);
							Debug.Assert(e.Section.endIndexInLeaf == e.Position);
							e.Section.endLeaf = thisAsLeaf;
							e.Section.endIndexInLeaf = newPosition;
						}
					}
					// Write event to output array
					InsertEvent(ref outputIndex, new Event {
						Section = e.Section,
						Kind = e.Kind,
						Position = newPosition
					});
					sibling.events[i].Section = null;
				} else {
					remainingEventsInSibling++;
				}
			}
			sibling.CompactifyEvents(remainingEventsInSibling);
			// Note: we expect the caller to update `collapsed` after childCount is also updated.
		}

		protected void CompactifyEvents(int remainingEvents)
		{
			if (events == null)
				return;
			if (remainingEvents == 0) {
				events = null;
			} else if (remainingEvents < events.Length / 4) {
				int outputIndex = 0;
				for (int i = 0; i < events.Length; i++) {
					if (events[i].Section != null) {
						events[outputIndex] = events[i];
						outputIndex++;
					}
				}
				if (outputIndex < events.Length)
					Array.Resize(ref events, outputIndex);
			}
		}

		internal void RemoveEvent(CollapsedLineSection section, EventKind kind)
		{
			if (events == null)
				return;
			int remainingEvents = 0;
			for (int i = 0; i < events.Length; i++) {
				if (events[i].Section == section && events[i].Kind == kind) {
					events[i].Section = null;
				} else if (events[i].Section != null) {
					remainingEvents++;
				}
			}
			CompactifyEvents(remainingEvents);
			collapsed = RecomputeCollapsedBits();
		}

		internal ushort RecomputeCollapsedBits()
		{
			if (events == null)
				return 0;
			bool inLeaf = this is HeightTreeLeafNode;
			int startOffset = inLeaf ? 0 : 1;
			int endOffset = inLeaf ? 1 : 0;
			ushort result = 0;
			foreach (var e in events) {
				var section = e.Section;
				if (section == null)
					continue;
				if (e.Kind == EventKind.Start) {
					Debug.Assert(section.StartIsWithin(this, out int start) && start == e.Position);
					Debug.Assert(e.Position < childCount);
					result |= BitsBetween(
						e.Position + startOffset,
						section.EndIsWithin(this, out int end) ? end + endOffset : childCount
					);
				} else {
					Debug.Assert(section.EndIsWithin(this, out int end) && end == e.Position);
					Debug.Assert(e.Position < childCount);
					result |= BitsBetween(
						section.StartIsWithin(this, out int start) ? start + startOffset : 0,
						e.Position + endOffset
					);
				}
			}
			return result;
		}

		/// <summary>
		/// Generate mask where all bits between start (inclusive) and end (exclusive) are 1.
		/// </summary>
		private static ushort BitsBetween(int start, int end)
		{
			Debug.Assert(0 <= start && start <= end && end <= 16);
			return (ushort)(((1 << (end - start)) - 1) << start);
		}
	}
}

