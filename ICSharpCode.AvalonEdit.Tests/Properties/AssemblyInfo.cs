// SPDX-License-Identifier: MIT

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;

// This sets the default COM visibility of types in the assembly to invisible.
// If you need to expose a type to COM, use [ComVisible(true)] on that type.
[assembly: ComVisible(false)]

#if !NETCOREAPP
// Run unit tests on STA thread.
[assembly: Apartment(ApartmentState.STA)]
#endif