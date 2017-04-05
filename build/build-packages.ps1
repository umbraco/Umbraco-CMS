
# to the whole build.proj dance
# then do the build.bat thing too
# or - use VSO to build the nuget packages?

# fixme
#  first thing, we need to restore NuGet!
#  then, we need to ./get-version

import-module ".\build-module.psm1"

# constants
# (unless we decide to make them params)
$buildConfiguration = "Release"
$vso = $false

# directories
# fixme - refactor how we do fullpath everywhere
$build = $pwd
$temp = "$build\_BuildOutput"
$src = $(fullpath "..\src")

# tools
$zip = $(fullpath "..\tools\7zip\7za.exe")
$nuget = "$src\.nuget\NuGet.exe"
$vswhere = $(fullpath "..\tools\VsWhere\vswhere.exe")
$vs = findVs($vswhere)

# get version
$version = $env:UMBRACO_VERSION
if ($version -eq "" -or $version -eq $null) {
  write-error "Could not determine version."
  exit 1
}
$release = $env:UMBRACO_RELEASE
$comment = $env:UMBRACO_COMMENT
if ($comment -eq $null) { $comment = "" }
$build = $env:UMBRACO_BUILD
if ($build -eq $null) { $build = "" }
write "Version $version"

# clear
write "Clear folders and files"

rmrf "$src\Umbraco.Web.UI.Client\build"
rmrf "$src\Umbraco.Web.UI.Client\bower_components"

rmrf "$temp"

rmf "$build\UmbracoCms.*.zip"
rmf "$build\UmbracoExamine.*.zip"
rmf "$build\UmbracoCms.*.nupkg"
rmf "$build\webpihash.txt"

# prepare
write "Making sure we have a web.config"

$webUi = "$src\Umbraco.Web.UI"
if (-not test-path "$webUi\web.config") {
  cp "$webUi\web.Template.config" "$webUi\web.config"
}

# compile
if (-not $vso) {

  $toolsVersion = "4.0"
  if ($vs.Major -eq 15) {
    $toolsVersion = "15.0"
  }
  
  &$vs.MsBuild "$src\Umbraco.Web.UI\Umbraco.Web.UI.csproj" `
    /p:WarningLevel=0 `
    /p:Configuration=$buildConfiguration `
    /p:UseWPP_CopyWebApplication=True `
    /p:PipelineDependsOnBuild=False `
    /p:OutDir="$temp\bin\" `
    /p:WebProjectOutputDir="$temp\WebApp\" `
    /p:Verbosity=minimal `
    /t:Clean;Rebuild `
    /tv:$toolsVersion
}

# fixme
#  at that point build.bat triggers build.proj
#  vso should build and run tests
#
#  rename _BuildOutput to _build.temp
#  are we building there?

# cleanup presentation
write "Cleanup presentation"

rmrf "$temp\WebApp\umbraco.presentation"

# copy various files
write "Copy xml documentation"

cp -force "$temp\bin\*.xml" "$temp\WebApp\bin"

write "Copy transformed configs and langs"

cp -force "$temp\WebApp\config\*.config"
cp -force "$temp\WebApp\config\*.js" "$temp\Configs")
cp -force "$temp\WebApp\config\lang\*.xml" "$temp\Configs\Lang")
cp -force "$temp\WebApp\web.config" "$temp\Configs\web.config.transform")

write "Copy transformed web.config"

cp -force "$src\Umbraco.Web.UI\web.$buildConfiguration.Config.transformed" "$temp\WebApp\web.config"

# offset the modified timestamps on all umbraco dlls, as WebResources
# break if date is in the future, which, due to timezone offsets can happen.
write "Offset dlls timestamps"
ls "$temp\**\*.dll" | foreach {
  $_.CreationTime = $_.CreationTime.AddHours(-11)
  $_.LastWriteTime = $_.LastWriteTime.AddHours(-11)
}

# copy libs
write "Copy SqlCE libraries"

$cpbase = "$src\packages\SqlServerCE.4.0.0.1"
ls "$cpbase\**\*.*" |
  where { -not $_.Extension.StartsWith(".nu") -and -not $_.FullName.SubString($cpbase.Length+1).StartsWith("lib\") } | 
  foreach {
    cp $_ -destination "$temp\bin\$($_.FullName.SubString($cpbase.Length+1))"
    cp $_ -destination "$temp\WebApp\$($_.FullName.SubString($cpbase.Length1+))"
  }
        
# copy Belle
write "Copy Belle"

$cpbase = "$src\Umbraco.Web.UI.Client\build\belle"
ls "$cpbase\**\*.*" |
  where { -not $_.FullName.SubString($cpbase.Length+1) -eq "index.html" } |
  foreach {
    cp $_ -destination "$temp\WebApp\umbraco\$($_.FullName.SubString($cpbase.Length+1))"
  }

# create system folders
write "Create system folders"

mkdir "$temp\WepApp\App_Data"
mkdir "$temp\WepApp\Media"
mkdir "$temp\WepApp\Views"

# zip webapp
write "Zip WebApp"

&$zip a -r "$build\UmbracoCms.AllBinaries.$version.zip" "$temp\bin\*"  -x!dotLess.Core.dll | out-null
&$zip a -r "$build\UmbracoCms.$version.zip" "$temp\WebApp\*" -x!dotLess.Core.dll -x![Content_Types].xml | out-null

# prepare and zip WebPI
write "Zip WebPI"

rmrf "$temp\WebPi"
mkdir "$temp\WebPi"
mkdir "$temp\WebPi\umbraco"

$cpbase = "$temp\WebApp"
ls "$cpbase\*" | foreach {
  cp $_ -destination "$temp\WebPi\umbraco\$_.FullName.SubString($cpbase.Length+1)"
}

$cpbase = "$src\WebPi"
ls "$cpbase\**\*.*" | foreach {
  cp $_ -destination "$temp\WebPi\$_.FullName.SubString($cpbase.Length+1)"
}
    
&$zip a -r "$build\UmbracoCms.WebPI.$version.zip" "$temp\WebPi\*" -x!dotLess.Core.dll | out-null
  
# clear
write "Delete build folder"
rmrf "$temp"

# hash the webpi file
write "Hash the WebPI file"

$hash = genHash "$build\UmbracoCms.WebPI.$version.zip"
write $hash | out-file "$build\webpihash.txt" -encoding ascii

# setting node_modules folder to hidden to prevent VS13 from
# crashing on it while loading the websites project (still needed?)
write "Set hidden attribute on node_modules"

$dir = get-item "$src\Umbraco.Web.UI.Client\node_modules"
$dir.Attributes = $dir.Attributes -bor ([System.IO.FileAttributes]::Hidden)

# add Web.config transform files to the NuGet package
write "Add web.config transforms to NuGet package"
mv "$temp\WebApp\Views\Web.config" "$temp\WebApp\Views\Web.config.transform"
mv "$temp\WebApp\Xslt\Web.config" "$temp\WebApp\Xslt\Web.config.transform"

# create NuGet packages
if (-not $vso)
{
  write "Create NuGet packages"
  
  &$nuget Pack "$build\NuSpecs\UmbracoCms.Core.nuspec" -Version $version -Symbols -Verbosity quiet
  &$nuget Pack "$build\NuSpecs\UmbracoCms.nuspec" -Version $version -Verbosity quiet
}

# eof