@ECHO OFF
SETLOCAL
SET msbuild=%windir%\microsoft.net\framework\v4.0.30319\msbuild
SET documentation=..\Documentation
SET project=..\ICSharpCode.AvalonEdit\ICSharpCode.AvalonEdit.csproj
SET sample=..\ICSharpCode.AvalonEdit.Samle\ICSharpCode.AvalonEdit.Sample.csproj
SET buildoptions=/t:Rebuild /p:Configuration=Release /p:DebugType=PdbOnly

@echo Using this script requires nuget.exe to be in the PATH, and that Sandcastle Help File Builder is installed.

@ECHO ON
:Clean debug build
%msbuild% /m %project% /t:Clean /p:Configuration=Debug /p:OutputPath=%~dp0\AvalonEdit\lib\Net40
@if %errorlevel% neq 0 exit /B 1

:BUILD .NET 4.0 version
%msbuild% /m %project% %buildoptions% /p:Platform=Net40 /p:OutputPath=%~dp0\AvalonEdit\lib\Net40
@if %errorlevel% neq 0 exit /B 1

:BUILD .NET 3.5 version
%msbuild% /m %project% %buildoptions% /p:Platform=Net35 "/p:DefineConstants=TRACE" /p:OutputPath=%~dp0\AvalonEdit\lib\Net35
@if %errorlevel% neq 0 exit /B 1

@echo Building documentation with SHFB (for processing <inheritdoc/>)
%msbuild% %documentation%\ICSharpCode.AvalonEdit.shfbproj
@if %errorlevel% neq 0 exit /B 1

copy /Y %documentation%\Help\ICSharpCode.AvalonEdit.xml AvalonEdit\lib\Net35\ICSharpCode.AvalonEdit.xml
copy /Y %documentation%\Help\ICSharpCode.AvalonEdit.xml AvalonEdit\lib\Net40\ICSharpCode.AvalonEdit.xml

nuget.exe pack AvalonEdit.nuspec -Symbols -BasePath AvalonEdit -OutputDirectory AvalonEdit
rem nuget.exe pack AvalonEdit.Sample.nuspec -BasePath AvalonEdit.Sample -OutputDirectory AvalonEdit.Sample

@ECHO OFF
ENDLOCAL