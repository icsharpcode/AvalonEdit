# AvalonEdit [![NuGet](https://img.shields.io/nuget/v/AvalonEdit.svg)](https://nuget.org/packages/AvalonEdit) [![Build status](https://ci.appveyor.com/api/projects/status/bvvux3y2b6tw272e/branch/master?svg=true)](https://ci.appveyor.com/project/icsharpcode/avalonedit/branch/master) [![Build Status Azure Pipelines](https://icsharpcode.visualstudio.com/icsharpcode-pipelines/_apis/build/status/icsharpcode.AvalonEdit?branchName=master)](https://icsharpcode.visualstudio.com/icsharpcode-pipelines/_build/latest?definitionId=3&branchName=master)


AvalonEdit is the name of the WPF-based text editor in SharpDevelop 4.x "Mirador" and beyond. It is also being used in ILSpy and many other projects.

Downloads
-------

AvalonEdit is available as [NuGet package](https://www.nuget.org/packages/AvalonEdit). Usage details, documentation and more
can be found on the [AvalonEdit homepage](http://avalonedit.net/)

How to build
-------

AvalonEdit is targeting netcoreapp3.0, net40 and net45 TFMs. Because of netcoreapp3.0 you must have .NET Core 3.0 SDK installed 
on your machine. Visual Studio 2019 16.3 and up is required for working with the solution (global.json will select the proper SDK to use for building for you).


Documentation
-------

Check out the [official documentation](http://avalonedit.net/documentation/) and the [samples and articles wiki page](https://github.com/icsharpcode/AvalonEdit/wiki/Samples-and-Articles)

To build the Documentation you need to install Sandcastle from https://github.com/EWSoftware/SHFB/releases (currently validated tooling is
v2019.4.14.0)

The build of the Documentation can take very long, please be patient.

License
-------

AvalonEdit is distributed under the [MIT License](http://opensource.org/licenses/MIT).

Projects using AvalonEdit
-------

A good place to start are the "top 10" listed under **GitHub Usage** for the [AvalonEdit package](https://www.nuget.org/packages/AvalonEdit) on NuGet.

* https://github.com/icsharpcode/ILSpy/ ILSpy .NET decompiler
* https://github.com/KirillOsenkov/MSBuildStructuredLog A logger for MSBuild 
* https://github.com/aelij/RoslynPad RoslynPad
* https://github.com/huangjia2107/XamlViewer LightWeight Xaml Editor

Note: if your project is not listed here, let us know! :)
