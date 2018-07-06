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

using System.Collections.Generic;
using System.Diagnostics;

namespace ICSharpCode.AvalonEdit.Rendering
{
	internal struct HeightTreeLineNode
	{
		internal HeightTreeLineNode(double height)
		{
			this.collapsedSections = null;
			this.height = height;
		}

		internal double height;
		internal List<CollapsedLineSection> collapsedSections;

		internal bool IsDirectlyCollapsed
		{
			get { return collapsedSections != null; }
		}

		internal void AddDirectlyCollapsed(CollapsedLineSection section)
		{
			if (collapsedSections == null)
				collapsedSections = new List<CollapsedLineSection>();
			collapsedSections.Add(section);
		}

		internal void RemoveDirectlyCollapsed(CollapsedLineSection section)
		{
			Debug.Assert(collapsedSections.Contains(section));
			collapsedSections.Remove(section);
			if (collapsedSections.Count == 0)
				collapsedSections = null;
		}

		/// <summary>
		/// Returns 0 if the line is directly collapsed, otherwise, returns <see cref="height"/>.
		/// </summary>
		internal double TotalHeight
		{
			get
			{
				return IsDirectlyCollapsed ? 0 : height;
			}
		}
	}
}