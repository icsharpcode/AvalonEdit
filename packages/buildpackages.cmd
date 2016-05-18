@ECHO OFF
SETLOCAL
cd %~dp0
SET msbuild=%windir%\microsoft.net\framework\v4.0.30319\msbuild
SET documentation=..\Documentation
SET project=..\ICSharpCode.AvalonEdit\ICSharpCode.AvalonEdit.csproj
SET buildoptions=/t:Rebuild /p:Configuration=Release /p:DebugType=PdbOnly

@echo Using this script requires nuget.exe to be in the PATH, and that Sandcastle Help File Builder is installed.

@ECHO ON
:Clean debug build
%msbuild% /m %project% /t:Clean /p:Configuration=Debug /p:OutputPath=%~dp0\AvalonEdit\lib\Net40
@if %errorlevel% neq 0 exit /B 1

:Normal build without modified output path; used by SHFB
%msbuild% /m %project% %buildoptions% /p:Platform=Net40
@if %errorlevel% neq 0 exit /B 1

:BUILD .NET 4.0 version
%msbuild% /m %project% %buildoptions% /p:Platform=Net40 /p:OutputPath=%~dp0\AvalonEdit\lib\Net40
@if %errorlevel% neq 0 exit /B 1

:BUILD .NET 3.5 version
%msbuild% /m %project% %buildoptions% /p:Platform=Net35 "/p:DefineConstants=TRACE" /p:OutputPath=%~dp0\AvalonEdit\lib\Net35
@if %errorlevel% neq 0 exit /B 1

@echo Building documentation with SHFB (for processing <inheritdoc/>)
%msbuild% %documentation%\ICSharpCode.AvalonEdit.shfbproj /p:Configuration=Release
@if %errorlevel% neq 0 exit /B 1

copy /Y %documentation%\IntelliSense\ICSharpCode.AvalonEdit.xml AvalonEdit\lib\Net35\ICSharpCode.AvalonEdit.xml
@if errorlevel 1 exit /B 1
copy /Y %documentation%\IntelliSense\ICSharpCode.AvalonEdit.xml AvalonEdit\lib\Net40\ICSharpCode.AvalonEdit.xml
@if errorlevel 1 exit /B 1

mkdir AvalonEdit
nuget.exe pack AvalonEdit.nuspec -Symbols -BasePath AvalonEdit -OutputDirectory AvalonEdit
@if %errorlevel% neq 0 exit /B 1
mkdir AvalonEdit.Sample
nuget.exe pack AvalonEdit.Sample.nuspec -BasePath AvalonEdit.Sample -OutputDirectory AvalonEdit.Sample
@if %errorlevel% neq 0 exit /B 1

@ECHO OFF
ENDLOCAL