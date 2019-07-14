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

using System;
using System.Threading;
using System.Windows;
using NUnit.Framework;

namespace ICSharpCode.AvalonEdit
{
	[TestFixture]
	public class MultipleUIThreads
	{
		Exception error;
		
		[Test]
		public void CreateEditorInstancesOnMultipleUIThreads()
		{
			Thread t1 = new Thread(new ThreadStart(Run));
			Thread t2 = new Thread(new ThreadStart(Run));
			t1.SetApartmentState(ApartmentState.STA);
			t2.SetApartmentState(ApartmentState.STA);
			t1.Start();
			t2.Start();
			t1.Join();
			t2.Join();
			if (error != null)
				throw new InvalidOperationException(error.Message, error);
		}
		
		[STAThread]
		void Run()
		{
			try {
				var window = new Window();
				window.Content = new TextEditor();
				window.ShowActivated = false;
				window.Show();
			} catch (Exception ex) {
				error = ex;
			}
		}
	}
}
