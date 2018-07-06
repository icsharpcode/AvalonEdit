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

namespace ICSharpCode.AvalonEdit.Document
{
	/*
	/// <summary>
	/// Implementation of a gap text buffer.
	/// </summary>
	sealed class GapTextBuffer
	{
		char[] buffer = Empty<char>.Array;

		/// <summary>
		/// The current text content.
		/// Is set to null whenever the buffer changes, and gets a value only when the
		/// full text content is requested.
		/// </summary>
		string textContent;

		/// <summary>
		/// last GetText result
		/// </summary>
		string lastGetTextResult;
		int lastGetTextRequestOffset;

		int gapBeginOffset;
		int gapEndOffset;
		int gapLength; // gapLength == gapEndOffset - gapBeginOffset

		/// <summary>
		/// when gap is too small for inserted text or gap is too large (exceeds maxGapLength),
		/// a new buffer is reallocated with a new gap of at least this size.
		/// </summary>
		const int minGapLength = 128;

		/// <summary>
		/// when the gap exceeds this size, reallocate a smaller buffer
		/// </summary>
		const int maxGapLength = 4096;

		public int Length {
			get {
				return buffer.Length - gapLength;
			}
		}

		/// <summary>
		/// Gets the buffer content.
		/// </summary>
		public string Text {
			get {
				if (textContent == null)
					textContent = GetText(0, Length);
				return textContent;
			}
			set {
				Debug.Assert(value != null);
				textContent = value;  lastGetTextResult = null;
				buffer = new char[value.Length + minGapLength];
				value.CopyTo(0, buffer, 0, value.Length);
				gapBeginOffset = value.Length;
				gapEndOffset = buffer.Length;
				gapLength = gapEndOffset - gapBeginOffset;
			}
		}

		public char GetCharAt(int offset)
		{
			return offset < gapBeginOffset ? buffer[offset] : buffer[offset + gapLength];
		}

		public string GetText(int offset, int length)
		{
			if (length == 0)
				return string.Empty;
			if (lastGetTextRequestOffset == offset && lastGetTextResult != null && length == lastGetTextResult.Length)
				return lastGetTextResult;

			int end = offset + length;
			string result;
			if (end < gapBeginOffset) {
				result = new string(buffer, offset, length);
			} else if (offset > gapBeginOffset) {
				result = new string(buffer, offset + gapLength, length);
			} else {
				int block1Size = gapBeginOffset - offset;
				int block2Size = end - gapBeginOffset;

				StringBuilder buf = new StringBuilder(block1Size + block2Size);
				buf.Append(buffer, offset,       block1Size);
				buf.Append(buffer, gapEndOffset, block2Size);
				result = buf.ToString();
			}
			lastGetTextRequestOffset = offset;
			lastGetTextResult = result;
			return result;
		}

		/// <summary>
		/// Inserts text at the specified offset.
		/// </summary>
		public void Insert(int offset, string text)
		{
			Debug.Assert(offset >= 0 && offset <= Length);

			if (text.Length == 0)
				return;

			textContent = null; lastGetTextResult = null;
			PlaceGap(offset, text.Length);
			text.CopyTo(0, buffer, gapBeginOffset, text.Length);
			gapBeginOffset += text.Length;
			gapLength = gapEndOffset - gapBeginOffset;
		}

		/// <summary>
		/// Remove <paramref name="length"/> characters at <paramref name="offset"/>.
		/// Leave a gap of at least <paramref name="reserveGapSize"/>.
		/// </summary>
		public void Remove(int offset, int length, int reserveGapSize)
		{
			Debug.Assert(offset >= 0 && offset <= Length);
			Debug.Assert(length >= 0 && offset + length <= Length);
			Debug.Assert(reserveGapSize >= 0);

			if (length == 0)
				return;

			textContent = null; lastGetTextResult = null;
			PlaceGap(offset, reserveGapSize - length);
			gapEndOffset += length; // delete removed text
			gapLength = gapEndOffset - gapBeginOffset;
			if (gapLength - reserveGapSize > maxGapLength && gapLength - reserveGapSize > buffer.Length / 4) {
				// shrink gap
				MakeNewBuffer(gapBeginOffset, reserveGapSize + minGapLength);
			}
		}

		void PlaceGap(int newGapOffset, int minRequiredGapLength)
		{
			if (gapLength < minRequiredGapLength) {
				// enlarge gap
				MakeNewBuffer(newGapOffset, minRequiredGapLength + Math.Max(minGapLength, buffer.Length / 8));
			} else {
				while (newGapOffset < gapBeginOffset) {
					buffer[--gapEndOffset] = buffer[--gapBeginOffset];
				}
				while (newGapOffset > gapBeginOffset) {
					buffer[gapBeginOffset++] = buffer[gapEndOffset++];
				}
			}
		}

		void MakeNewBuffer(int newGapOffset, int newGapLength)
		{
			char[] newBuffer = new char[Length + newGapLength];
			Debug.WriteLine("GapTextBuffer was reallocated, new size=" + newBuffer.Length);
			if (newGapOffset < gapBeginOffset) {
				// gap is moving backwards

				// first part:
				Array.Copy(buffer, 0, newBuffer, 0, newGapOffset);
				// moving middle part:
				Array.Copy(buffer, newGapOffset, newBuffer, newGapOffset + newGapLength, gapBeginOffset - newGapOffset);
				// last part:
				Array.Copy(buffer, gapEndOffset, newBuffer, newBuffer.Length - (buffer.Length - gapEndOffset), buffer.Length - gapEndOffset);
			} else {
				// gap is moving forwards
				// first part:
				Array.Copy(buffer, 0, newBuffer, 0, gapBeginOffset);
				// moving middle part:
				Array.Copy(buffer, gapEndOffset, newBuffer, gapBeginOffset, newGapOffset - gapBeginOffset);
				// last part:
				int lastPartLength = newBuffer.Length - (newGapOffset + newGapLength);
				Array.Copy(buffer, buffer.Length - lastPartLength, newBuffer, newGapOffset + newGapLength, lastPartLength);
			}

			gapBeginOffset = newGapOffset;
			gapEndOffset = newGapOffset + newGapLength;
			gapLength = newGapLength;
			buffer = newBuffer;
		}
	}
	*/
}