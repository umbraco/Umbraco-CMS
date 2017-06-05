
# to the whole build.proj dance
# then do the build.bat thing too
# or - use VSO to build the nuget packages?
#
# see set-psdebug -trace 0/1/2
# use &echoargs to debug & commands
# ./build-packages | out-file build-packages.log

# fixme
#  first thing, we need to restore NuGet!
#  then, we need to ./get-version

param (
  [string]
  $vso
)

# import build module
import-module ".\build-module.psm1"

# constants
# (unless we decide to make them params)
$buildConfiguration = "Release"

# directories
# fixme - refactor how we do fullpath everywhere
$build = $pwd
$temp = fullpath "..\build.temp"
$out = fullpath "..\build.out"
$src = fullpath "..\src"
$tools = fullpath "..\tools"

write "Build: $build"
write "Temp: $temp"
write "Src: $src"

write "Configuration: $buildConfiguration"

# tools
ensureTools $tools

$zip = "$tools\7za.exe"
$nuget = "$tools\nuget.exe"
$vswhere = "$tools\vswhere.exe"
$vs = findVisualStudio $vswhere

$nugetVersion = (&$nuget help | select-string "^NuGet Version:").Line.Split(':')[1].Trim()
write "NuGet: $nugetVersion"
write "Visual Studio: $($vs.Major).$($vs.Minor)"

# get version
function getVersion()
{
  $version = $env:UMBRACO_VERSION
  if ($version -eq "" -or $version -eq $null) {
    write-error "Could not determine version."
    exit 1
  }
  $release = $env:UMBRACO_RELEASE
  $comment = $env:UMBRACO_COMMENT
  if ($comment -eq $null) { $comment = "" }
  $buildno = $env:UMBRACO_BUILD
  if ($buildno -eq $null) { $buildno = "" }
  write "Version: $version"
  
  $global:version = $version
  $global:comment = $comment
  $global:buildno = $buildno
}

function preCompile()
{
  # clear
  write "Clear folders and files"

  rmrf "$src\Umbraco.Web.UI.Client\build"
  rmrf "$src\Umbraco.Web.UI.Client\bower_components"

  rmrf "$temp"
  mkdir "$temp"
  
  rmrf "$out"
  mkdir "$out"

  # prepare
  write "Making sure we have a web.config"

  $webUi = "$src\Umbraco.Web.UI"
  if (-not (test-path "$webUi\web.config"))
  {
    cpf "$webUi\web.Template.config" "$webUi\web.config"
  }
}

function compile()
{
  $toolsVersion = "4.0"
  if ($vs.Major -eq 15)
  {
    $toolsVersion = "15.0"
  }
    
  write "Compile"
  # fixme logger and stuff?!
  # beware of the weird double \\ at the end of paths
  # see http://edgylogic.com/blog/powershell-and-external-commands-done-right/
  &$vs.MsBuild "$src\Umbraco.Web.UI\Umbraco.Web.UI.csproj" `
    /p:WarningLevel=0 `
    /p:Configuration=$buildConfiguration `
    /p:UseWPP_CopyWebApplication=True `
    /p:PipelineDependsOnBuild=False `
    /p:OutDir=$temp\bin\\ `
    /p:WebProjectOutputDir=$temp\WebApp\\ `
    /p:Verbosity=minimal `
    /t:Clean`;Rebuild `
    /tv:$toolsVersion
}

function postCompile()
{
  # fixme
  #  at that point build.bat triggers build.proj
  #  vso should build and run tests
  #
  #  rename _BuildOutput to _build.temp
  #  are we building there?

  # cleanup build
  write "Clean build"

  rmf "$temp\bin\*.dll.config"
  rmf "$temp\WebApp\bin\*.dll.config"

  # cleanup presentation
  write "Cleanup presentation"

  rmrf "$temp\WebApp\umbraco.presentation"

  # create directories
  write "Create directories"

  mkdir "$temp\Configs"
  mkdir "$temp\Configs\Lang"
  mkdir "$temp\WebApp\App_Data"
  #mkdir "$temp\WebApp\Media"
  #mkdir "$temp\WebApp\Views"

  # copy various files
  write "Copy xml documentation"

  cp -force "$temp\bin\*.xml" "$temp\WebApp\bin"

  write "Copy transformed configs and langs"

  cprf "$temp\WebApp\config" "*.config" "$temp\Configs"
  cprf "$temp\WebApp\config" "*.js" "$temp\Configs"
  cprf "$temp\WebApp\config\lang" "*.xml" "$temp\Configs\Lang"
  cpf "$temp\WebApp\web.config" "$temp\Configs\web.config.transform"

  write "Copy transformed web.config"

  cpf "$src\Umbraco.Web.UI\web.$buildConfiguration.Config.transformed" "$temp\WebApp\web.config"

  # offset the modified timestamps on all umbraco dlls, as WebResources
  # break if date is in the future, which, due to timezone offsets can happen.
  write "Offset dlls timestamps"
  ls -r "$temp\*.dll" | foreach {
    $_.CreationTime = $_.CreationTime.AddHours(-11)
    $_.LastWriteTime = $_.LastWriteTime.AddHours(-11)
  }

  # copy libs
  write "Copy SqlCE libraries"

  cprf "$src\packages\SqlServerCE.4.0.0.1" "*.*" "$temp\bin" `
    { -not $_.Extension.StartsWith(".nu") -and -not $_.RelativeName.StartsWith("lib\") }
  cprf "$src\packages\SqlServerCE.4.0.0.1" "*.*" "$temp\WebApp\bin" `
    { -not $_.Extension.StartsWith(".nu") -and -not $_.RelativeName.StartsWith("lib\") }
          
  # copy Belle
  write "Copy Belle"

  cprf "$src\Umbraco.Web.UI.Client\build\belle" "*" "$temp\WebApp\umbraco" `
    { -not ($_.RelativeName -eq "index.html") }

  # zip webapp
  write "Zip WebApp"

  &$zip a -r "$out\UmbracoCms.AllBinaries.$version.zip" "$temp\bin\*"  -x!dotless.Core.dll #| out-null
  &$zip a -r "$out\UmbracoCms.$version.zip" "$temp\WebApp\*" -x!dotless.Core.dll -x!Content_Types.xml #| out-null

  # prepare and zip WebPI
  write "Zip WebPI"

  rmrf "$temp\WebPi"
  mkdir "$temp\WebPi"
  mkdir "$temp\WebPi\umbraco"

  cprf "$temp\WebApp" "*" "$temp\WebPi\umbraco"
  cprf "$src\WebPi" "*" "$temp\WebPi"
      
  &$zip a -r "$out\UmbracoCms.WebPI.$version.zip" "$temp\WebPi\*" -x!dotless.Core.dll #| out-null
    
  # clear
  write "Delete build folder"
  #rmrf "$temp"

  # hash the webpi file
  write "Hash the WebPI file"

  $hash = genHash "$out\UmbracoCms.WebPI.$version.zip"
  write $hash | out-file "$out\webpihash.txt" -encoding ascii

  # setting node_modules folder to hidden to prevent VS13 from
  # crashing on it while loading the websites project (still needed?)
  # use get-item -force 'cos it might be hidden already
  write "Set hidden attribute on node_modules"

  $dir = get-item -force "$src\Umbraco.Web.UI.Client\node_modules"
  $dir.Attributes = $dir.Attributes -bor ([System.IO.FileAttributes]::Hidden)

  # add Web.config transform files to the NuGet package
  write "Add web.config transforms to NuGet package"

  mv "$temp\WebApp\Views\Web.config" "$temp\WebApp\Views\Web.config.transform"
  # that one does not exist in .bat build either?
  #mv "$temp\WebApp\Xslt\Web.config" "$temp\WebApp\Xslt\Web.config.transform"
}

function createNuget()
{
  write "Create NuGet packages"

  &$nuget Pack "$build\NuSpecs\UmbracoCms.Core.nuspec" -Version $version -Symbols -Verbosity quiet -outputDirectory $out
  &$nuget Pack "$build\NuSpecs\UmbracoCms.nuspec" -Version $version -Verbosity quiet -outputDirectory $out
}


# build


if ($vso -eq "pre-compile")
{
  getVersion
  preCompile
  Exit 0
}

if ($vso -eq "post-compile")
{
  getVersion
  postcompile
  Exit 0
}

if ($vso)
{
  write "Invalid vso argument '$vso'."
  Exit 1
}

# do it all here
&.\get-version.ps1 $vso
getVersion
preCompile
compile
postCompile
createNuget

# eof