$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'

Write-Host "root: $root"

$version = [System.Reflection.Assembly]::LoadFile("$root\packages\lib\net45\ICSharpCode.AvalonEdit.dll").GetName().Version
$versionStr = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build)

$semver = ''
if (Test-Path ENV:semverinfo) {
  $semver = Get-ChildItem ENV:semverinfo
}
if ($semver) { 
  $versionStr = "{0}.{1}-{2}" -f ($version.Major, $version.Minor, $semver)
}

Write-Host "Setting .nuspec version tag to $versionStr"

$content = (Get-Content $root\packages\AvalonEdit.nuspec) 
$content = $content -replace '\$version\$',$versionStr
$content = $content -replace '\$releasenotes\$',$env:APPVEYOR_REPO_COMMIT_MESSAGE

$content | Out-File $root\packages\AvalonEdit.compiled.nuspec

NuGet pack $root\packages\AvalonEdit.compiled.nuspec

$content = (Get-Content $root\packages\AvalonEdit.Sample.nuspec) 
$content = $content -replace '\$version\$',$versionStr
$content = $content -replace '\$releasenotes\$',$env:APPVEYOR_REPO_COMMIT_MESSAGE

$content | Out-File $root\packages\AvalonEdit.Sample.nuspec.compiled.nuspec

NuGet pack $root\packages\AvalonEdit.Sample.nuspec.compiled.nuspec