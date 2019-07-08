// SPDX-License-Identifier: MIT

using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Document
{
	/// <summary>
	/// Contains weak event managers for the TextDocument events.
	/// </summary>
	public static class TextDocumentWeakEventManager
	{
		/// <summary>
		/// Weak event manager for the <see cref="TextDocument.UpdateStarted"/> event.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class UpdateStarted : WeakEventManagerBase<UpdateStarted, TextDocument>
		{
			/// <inheritdoc/>
			protected override void StartListening(TextDocument source)
			{
				source.UpdateStarted += DeliverEvent;
			}

			/// <inheritdoc/>
			protected override void StopListening(TextDocument source)
			{
				source.UpdateStarted -= DeliverEvent;
			}
		}

		/// <summary>
		/// Weak event manager for the <see cref="TextDocument.UpdateFinished"/> event.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class UpdateFinished : WeakEventManagerBase<UpdateFinished, TextDocument>
		{
			/// <inheritdoc/>
			protected override void StartListening(TextDocument source)
			{
				source.UpdateFinished += DeliverEvent;
			}

			/// <inheritdoc/>
			protected override void StopListening(TextDocument source)
			{
				source.UpdateFinished -= DeliverEvent;
			}
		}

		/// <summary>
		/// Weak event manager for the <see cref="TextDocument.Changing"/> event.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class Changing : WeakEventManagerBase<Changing, TextDocument>
		{
			/// <inheritdoc/>
			protected override void StartListening(TextDocument source)
			{
				source.Changing += DeliverEvent;
			}

			/// <inheritdoc/>
			protected override void StopListening(TextDocument source)
			{
				source.Changing -= DeliverEvent;
			}
		}

		/// <summary>
		/// Weak event manager for the <see cref="TextDocument.Changed"/> event.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class Changed : WeakEventManagerBase<Changed, TextDocument>
		{
			/// <inheritdoc/>
			protected override void StartListening(TextDocument source)
			{
				source.Changed += DeliverEvent;
			}

			/// <inheritdoc/>
			protected override void StopListening(TextDocument source)
			{
				source.Changed -= DeliverEvent;
			}
		}

		/// <summary>
		/// Weak event manager for the <see cref="TextDocument.TextChanged"/> event.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class TextChanged : WeakEventManagerBase<TextChanged, TextDocument>
		{
			/// <inheritdoc/>
			protected override void StartListening(TextDocument source)
			{
				source.TextChanged += DeliverEvent;
			}

			/// <inheritdoc/>
			protected override void StopListening(TextDocument source)
			{
				source.TextChanged -= DeliverEvent;
			}
		}
	}
}
