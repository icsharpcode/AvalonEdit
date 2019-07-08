// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Document
{
	/// <summary>
	/// Provides ITextSourceVersion instances.
	/// </summary>
	public class TextSourceVersionProvider
	{
		Version currentVersion;

		/// <summary>
		/// Creates a new TextSourceVersionProvider instance.
		/// </summary>
		public TextSourceVersionProvider()
		{
			this.currentVersion = new Version(this);
		}

		/// <summary>
		/// Gets the current version.
		/// </summary>
		public ITextSourceVersion CurrentVersion {
			get { return currentVersion; }
		}

		/// <summary>
		/// Replaces the current version with a new version.
		/// </summary>
		/// <param name="change">Change from current version to new version</param>
		public void AppendChange(TextChangeEventArgs change)
		{
			if (change == null)
				throw new ArgumentNullException("change");
			currentVersion.change = change;
			currentVersion.next = new Version(currentVersion);
			currentVersion = currentVersion.next;
		}

		[DebuggerDisplay("Version #{id}")]
		sealed class Version : ITextSourceVersion
		{
			// Reference back to the provider.
			// Used to determine if two checkpoints belong to the same document.
			readonly TextSourceVersionProvider provider;
			// ID used for CompareAge()
			readonly int id;

			// the change from this version to the next version
			internal TextChangeEventArgs change;
			internal Version next;

			internal Version(TextSourceVersionProvider provider)
			{
				this.provider = provider;
			}

			internal Version(Version prev)
			{
				this.provider = prev.provider;
				this.id = unchecked(prev.id + 1);
			}

			public bool BelongsToSameDocumentAs(ITextSourceVersion other)
			{
				Version o = other as Version;
				return o != null && provider == o.provider;
			}

			public int CompareAge(ITextSourceVersion other)
			{
				if (other == null)
					throw new ArgumentNullException("other");
				Version o = other as Version;
				if (o == null || provider != o.provider)
					throw new ArgumentException("Versions do not belong to the same document.");
				// We will allow overflows, but assume that the maximum distance between checkpoints is 2^31-1.
				// This is guaranteed on x86 because so many checkpoints don't fit into memory.
				return Math.Sign(unchecked(this.id - o.id));
			}

			public IEnumerable<TextChangeEventArgs> GetChangesTo(ITextSourceVersion other)
			{
				int result = CompareAge(other);
				Version o = (Version)other;
				if (result < 0)
					return GetForwardChanges(o);
				else if (result > 0)
					return o.GetForwardChanges(this).Reverse().Select(change => change.Invert());
				else
					return Empty<TextChangeEventArgs>.Array;
			}

			IEnumerable<TextChangeEventArgs> GetForwardChanges(Version other)
			{
				// Return changes from this(inclusive) to other(exclusive).
				for (Version node = this; node != other; node = node.next) {
					yield return node.change;
				}
			}

			public int MoveOffsetTo(ITextSourceVersion other, int oldOffset, AnchorMovementType movement)
			{
				int offset = oldOffset;
				foreach (var e in GetChangesTo(other)) {
					offset = e.GetNewOffset(offset, movement);
				}
				return offset;
			}
		}
	}
}
