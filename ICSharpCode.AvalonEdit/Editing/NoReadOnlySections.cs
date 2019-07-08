// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Editing
{
	/// <summary>
	/// <see cref="IReadOnlySectionProvider"/> that has no read-only sections; all text is editable.
	/// </summary>
	sealed class NoReadOnlySections : IReadOnlySectionProvider
	{
		public static readonly NoReadOnlySections Instance = new NoReadOnlySections();

		public bool CanInsert(int offset)
		{
			return true;
		}

		public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
		{
			if (segment == null)
				throw new ArgumentNullException("segment");
			// the segment is always deletable
			return ExtensionMethods.Sequence(segment);
		}
	}

	/// <summary>
	/// <see cref="IReadOnlySectionProvider"/> that completely disables editing.
	/// </summary>
	sealed class ReadOnlySectionDocument : IReadOnlySectionProvider
	{
		public static readonly ReadOnlySectionDocument Instance = new ReadOnlySectionDocument();

		public bool CanInsert(int offset)
		{
			return false;
		}

		public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
		{
			return Enumerable.Empty<ISegment>();
		}
	}
}
