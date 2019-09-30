
# Umbraco.Build.psm1
#
# $env:PSModulePath = "$pwd\build\Modules\;$env:PSModulePath"
# Import-Module Umbraco.Build -Force -DisableNameChecking
#
# PowerShell Modules:
# https://msdn.microsoft.com/en-us/library/dd878324%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
#
# PowerShell Module Manifest:
# https://msdn.microsoft.com/en-us/library/dd878337%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
#
# See also
# http://www.powershellmagazine.com/2014/08/15/pstip-taking-control-of-verbose-and-debug-output-part-5/


. "$PSScriptRoot\Utilities.ps1"
. "$PSScriptRoot\Get-VisualStudio.ps1"

. "$PSScriptRoot\Get-UmbracoBuildEnv.ps1"
. "$PSScriptRoot\Set-UmbracoVersion.ps1"
. "$PSScriptRoot\Set-UmbracoContinuousVersion.ps1"
. "$PSScriptRoot\Get-UmbracoVersion.ps1"
. "$PSScriptRoot\Verify-NuGet.ps1"

. "$PSScriptRoot\Build-UmbracoDocs.ps1"

#
# Prepares the build
#
function Prepare-Build
{
  param (
    $uenv, # an Umbraco build environment (see Get-UmbracoBuildEnv)
    
    [Alias("k")]
    [switch]
    $keep = $false
  )

  Write-Host ">> Prepare Build"
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"

  # clear
  Write-Host "Clear folders and files"

  Remove-Directory "$src\Umbraco.Web.UI.Client\bower_components"

  if (-not $keep)
  {
	Remove-Directory "$tmp"
	Remove-Directory "$out"
  }

  if (-not (Test-Path "$tmp")) 
  {
    mkdir "$tmp" > $null
  }
  if (-not (Test-Path "$out")) 
  {
    mkdir "$out" > $null
  }
    
  # ensure proper web.config
  $webUi = "$src\Umbraco.Web.UI"
  Store-WebConfig $webUi
  Write-Host "Create clean web.config"
  Copy-File "$webUi\web.Template.config" "$webUi\web.config"
}

function Clear-EnvVar($var)
{
  $value = [Environment]::GetEnvironmentVariable($var)
  if (test-path "env:$var") { rm "env:$var" }
  return $value
}

function Set-EnvVar($var, $value)
{
  if ($value)
  {
    [Environment]::SetEnvironmentVariable($var, $value)
  }
  else
  {
    if (test-path "env:$var") { rm "env:$var" }
  }
}

function Sandbox-Node
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )
  
  $global:node_path = $env:path
  $nodePath = $uenv.NodePath
  $gitExe = (get-command git).Source  
  $gitPath = [System.IO.Path]::GetDirectoryName($gitExe)
  $env:path = "$nodePath;$gitPath"
  
  $global:node_nodepath = Clear-EnvVar "NODEPATH"
  $global:node_npmcache = Clear-EnvVar "NPM_CONFIG_CACHE"
  $global:node_npmprefix = Clear-EnvVar "NPM_CONFIG_PREFIX"
}

function Restore-Node
{
  $env:path = $node_path
  
  Set-EnvVar "NODEPATH" $node_nodepath
  Set-EnvVar "NPM_CONFIG_CACHE" $node_npmcache
  Set-EnvVar "NPM_CONFIG_PREFIX" $node_npmprefix
}

#
# Builds the Belle UI project
#
function Compile-Belle
{
  param (
    $uenv, # an Umbraco build environment (see Get-UmbracoBuildEnv)
    $version # an Umbraco version object (see Get-UmbracoVersion)
  )

  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $src = "$($uenv.SolutionRoot)\src"
  
  Write-Host ">> Compile Belle"
  Write-Host "Logging to $tmp\belle.log"
  
  # get a temp clean node env (will restore)
  Sandbox-Node $uenv
  
  push-location "$($uenv.SolutionRoot)\src\Umbraco.Web.UI.Client"
  write "node version is:" > $tmp\belle.log
  &node -v >> $tmp\belle.log 2>&1
  write "npm version is:" >> $tmp\belle.log 2>&1
  &npm -v >> $tmp\belle.log 2>&1
  write "cleaning npm cache" >> $tmp\belle.log 2>&1
  &npm cache clean >> $tmp\belle.log 2>&1
  write "installing bower" >> $tmp\belle.log 2>&1
  &npm install -g bower >> $tmp\belle.log 2>&1
  write "installing gulp" >> $tmp\belle.log 2>&1
  &npm install -g gulp >> $tmp\belle.log 2>&1
  write "installing gulp-cli" >> $tmp\belle.log 2>&1
  &npm install -g gulp-cli --quiet >> $tmp\belle.log 2>&1
  write "executing npm install" >> $tmp\belle.log 2>&1
  &npm install >> $tmp\belle.log 2>&1
  write "executing gulp build for version $version" >> $tmp\belle.log 2>&1
  &gulp build --buildversion=$version.Release >> $tmp\belle.log 2>&1
  pop-location
  
  # fixme - should we filter the log to find errors?
  #get-content .\build.tmp\belle.log | %{ if ($_ -match "build") { write $_}}

  # restore
  Restore-Node
  
  # setting node_modules folder to hidden
  # used to prevent VS13 from crashing on it while loading the websites project
  # also makes sure aspnet compiler does not try to handle rogue files and chokes
  # in VSO with Microsoft.VisualC.CppCodeProvider -related errors
  # use get-item -force 'cos it might be hidden already
  write "Set hidden attribute on node_modules"
  $dir = get-item -force "$src\Umbraco.Web.UI.Client\node_modules"
  $dir.Attributes = $dir.Attributes -bor ([System.IO.FileAttributes]::Hidden)
}

#
# Compiles Umbraco
#
function Compile-Umbraco
{
  param (
    $uenv, # an Umbraco build environment (see Get-UmbracoBuildEnv)
    [string] $buildConfiguration = "Release"
  )

  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"
  
  if ($uenv.VisualStudio -eq $null)
  {
    Write-Error "Build environment does not provide VisualStudio."
    break
  }
  
  $toolsVersion = "4.0"
  if ($uenv.VisualStudio.Major -eq 15)
  {
    $toolsVersion = "15.0"
  }
  if ($uenv.VisualStudio.Major -eq 16)
  {
    $toolsVersion = "Current"
  }
    
  Write-Host ">> Compile Umbraco"
  Write-Host "Logging to $tmp\msbuild.umbraco.log"

  # beware of the weird double \\ at the end of paths
  # see http://edgylogic.com/blog/powershell-and-external-commands-done-right/
  &$uenv.VisualStudio.MsBuild "$src\Umbraco.Web.UI\Umbraco.Web.UI.csproj" `
    /p:WarningLevel=0 `
    /p:Configuration=$buildConfiguration `
    /p:Platform=AnyCPU `
    /p:UseWPP_CopyWebApplication=True `
    /p:PipelineDependsOnBuild=False `
    /p:OutDir=$tmp\bin\\ `
    /p:WebProjectOutputDir=$tmp\WebApp\\ `
    /p:Verbosity=minimal `
    /t:Clean`;Rebuild `
    /tv:$toolsVersion `
    /p:UmbracoBuild=True `
    > $tmp\msbuild.umbraco.log
    
  # /p:UmbracoBuild tells the csproj that we are building from PS
}

#
# Prepare Tests
#
function Prepare-Tests
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  
  Write-Host ">> Prepare Tests"

  # fixme - idea is to avoid rebuilding everything for tests
  # but because of our weird assembly versioning (with .* stuff)
  # everything gets rebuilt all the time...
  #Copy-Files "$tmp\bin" "." "$tmp\tests"
  
  # data
  Write-Host "Copy data files"
  if (-Not (Test-Path -Path "$tmp\tests\Packaging" ) )
  {
    Write-Host "Create packaging directory"
    mkdir "$tmp\tests\Packaging" > $null
  }
  Copy-Files "$src\Umbraco.Tests\Packaging\Packages" "*" "$tmp\tests\Packaging\Packages"
  
  # required for package install tests  
  if (-Not (Test-Path -Path "$tmp\tests\bin" ) )
  {
    Write-Host "Create bin directory"
    mkdir "$tmp\tests\bin" > $null
  }
}

#
# Compiles Tests
#
function Compile-Tests
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )

  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$tmp\tests"

  $buildConfiguration = "Release"
  
  if ($uenv.VisualStudio -eq $null)
  {
    Write-Error "Build environment does not provide VisualStudio."
    break
  }
  
  $toolsVersion = "4.0"
  if ($uenv.VisualStudio.Major -eq 15)
  {
    $toolsVersion = "15.0"
  }
    
  Write-Host ">> Compile Tests"
  Write-Host "Logging to $tmp\msbuild.tests.log"

  # beware of the weird double \\ at the end of paths
  # see http://edgylogic.com/blog/powershell-and-external-commands-done-right/
  &$uenv.VisualStudio.MsBuild "$src\Umbraco.Tests\Umbraco.Tests.csproj" `
    /p:WarningLevel=0 `
    /p:Configuration=$buildConfiguration `
    /p:Platform=AnyCPU `
    /p:UseWPP_CopyWebApplication=True `
    /p:PipelineDependsOnBuild=False `
    /p:OutDir=$out\\ `
    /p:Verbosity=minimal `
    /t:Build `
    /tv:$toolsVersion `
    /p:UmbracoBuild=True `
    /p:NugetPackages=$src\packages `
    > $tmp\msbuild.tests.log
    
  # /p:UmbracoBuild tells the csproj that we are building from PS
}

#
# Cleans things up and prepare files after compilation
#
function Prepare-Packages
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )

  Write-Host ">> Prepare Packages" 
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"

  $buildConfiguration = "Release"

  # restore web.config
  Restore-WebConfig "$src\Umbraco.Web.UI"

  # cleanup build
  Write-Host "Clean build"
  Remove-File "$tmp\bin\*.dll.config"
  Remove-File "$tmp\WebApp\bin\*.dll.config"

  # cleanup presentation
  Write-Host "Cleanup presentation"
  Remove-Directory "$tmp\WebApp\umbraco.presentation"

  # create directories
  Write-Host "Create directories"
  mkdir "$tmp\Configs" > $null
  mkdir "$tmp\Configs\Lang" > $null
  mkdir "$tmp\WebApp\App_Data" > $null
  #mkdir "$tmp\WebApp\Media" > $null
  #mkdir "$tmp\WebApp\Views" > $null

  # copy various files
  Write-Host "Copy xml documentation"
  cp -force "$tmp\bin\*.xml" "$tmp\WebApp\bin"

  Write-Host "Copy transformed configs and langs"
  # note: exclude imageprocessor/*.config as imageprocessor pkg installs them
  Copy-Files "$tmp\WebApp\config" "*.config" "$tmp\Configs" `
    { -not $_.RelativeName.StartsWith("imageprocessor") }
  Copy-Files "$tmp\WebApp\config" "*.js" "$tmp\Configs"
  Copy-Files "$tmp\WebApp\config\lang" "*.xml" "$tmp\Configs\Lang"
  Copy-File "$tmp\WebApp\web.config" "$tmp\Configs\web.config.transform"

  Write-Host "Copy transformed web.config"
  Copy-File "$src\Umbraco.Web.UI\web.$buildConfiguration.Config.transformed" "$tmp\WebApp\web.config"

  # offset the modified timestamps on all umbraco dlls, as WebResources
  # break if date is in the future, which, due to timezone offsets can happen.
  Write-Host "Offset dlls timestamps"
  ls -r "$tmp\*.dll" | foreach {
    $_.CreationTime = $_.CreationTime.AddHours(-11)
    $_.LastWriteTime = $_.LastWriteTime.AddHours(-11)
  }

  # copy libs
  Write-Host "Copy SqlCE libraries"
  Copy-Files "$src\packages\SqlServerCE.4.0.0.1" "*.*" "$tmp\bin" `
    { -not $_.Extension.StartsWith(".nu") -and -not $_.RelativeName.StartsWith("lib\") }
  Copy-Files "$src\packages\SqlServerCE.4.0.0.1" "*.*" "$tmp\WebApp\bin" `
    { -not $_.Extension.StartsWith(".nu") -and -not $_.RelativeName.StartsWith("lib\") }
          
  # copy Belle
  Write-Host "Copy Belle"
  Copy-Files "$src\Umbraco.Web.UI\umbraco\assets" "*" "$tmp\WebApp\umbraco\assets"
  Copy-Files "$src\Umbraco.Web.UI\umbraco\js" "*" "$tmp\WebApp\umbraco\js"
  Copy-Files "$src\Umbraco.Web.UI\umbraco\lib" "*" "$tmp\WebApp\umbraco\lib"
  Copy-Files "$src\Umbraco.Web.UI\umbraco\views" "*" "$tmp\WebApp\umbraco\views"

}

#
# Creates the Zip packages
#
function Package-Zip
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )

  Write-Host ">> Create Zip packages" 
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"

  Write-Host "Zip all binaries"  
  &$uenv.Zip a -r "$out\UmbracoCms.AllBinaries.$($version.Semver).zip" `
    "$tmp\bin\*" `
    "-x!dotless.Core.*" `
    > $null
   
  Write-Host "Zip cms"  
  &$uenv.Zip a -r "$out\UmbracoCms.$($version.Semver).zip" `
    "$tmp\WebApp\*" `
    "-x!dotless.Core.*" "-x!Content_Types.xml" "-x!*.pdb"`
    > $null
}

#
# Prepares NuGet
#
function Prepare-NuGet
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )

  Write-Host ">> Prepare NuGet" 
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"

  # add Web.config transform files to the NuGet package
  Write-Host "Add web.config transforms to NuGet package"
  mv "$tmp\WebApp\Views\Web.config" "$tmp\WebApp\Views\Web.config.transform"
  
  # fixme - that one does not exist in .bat build either?
  #mv "$tmp\WebApp\Xslt\Web.config" "$tmp\WebApp\Xslt\Web.config.transform"
}

#
# Restores NuGet
#
function Restore-NuGet
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )

  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"

  Write-Host ">> Restore NuGet"
  Write-Host "Logging to $tmp\nuget.restore.log" 
  
  &$uenv.NuGet restore "$src\Umbraco.sln" -configfile "$src\NuGet.config" > "$tmp\nuget.restore.log"
}

#
# Copies the Azure Gallery script to output
#
function Prepare-AzureGallery
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"
  $psScript = "$($uenv.SolutionRoot)\build\azuregalleryrelease.ps1"
  
  Write-Host ">> Copy azuregalleryrelease.ps1 to output folder"
  Copy-Item $psScript $out
}

#
# Creates the NuGet packages
#
function Package-NuGet
{
  param (
    $uenv, # an Umbraco build environment (see Get-UmbracoBuildEnv)
    $version # an Umbraco version object (see Get-UmbracoVersion)
  )
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"
  $nuspecs = "$($uenv.SolutionRoot)\build\NuSpecs"
  
  Write-Host ">> Create NuGet packages"

  # see https://docs.microsoft.com/en-us/nuget/schema/nuspec
  # note - warnings about SqlCE native libs being outside of 'lib' folder,
  # nothing much we can do about it as it's intentional yet there does not
  # seem to be a way to disable the warning
  
  &$uenv.NuGet Pack "$nuspecs\UmbracoCms.Core.nuspec" `
    -Properties BuildTmp="$tmp" `
    -Version $version.Semver.ToString() `
    -Symbols -Verbosity quiet -outputDirectory $out

  &$uenv.NuGet Pack "$nuspecs\UmbracoCms.nuspec" `
    -Properties BuildTmp="$tmp" `
    -Version $version.Semver.ToString() `
    -Verbosity quiet -outputDirectory $out
}

#
# Builds Umbraco
#
function Build-Umbraco
{
  [CmdletBinding()]
  param (
    [string]
    $target = "all",
    [string]
    $buildConfiguration = "Release"
  )
  
  $target = $target.ToLowerInvariant()
  Write-Host ">> Build-Umbraco <$target> <$buildConfiguration>"

  Write-Host "Get Build Environment"
  $uenv = Get-UmbracoBuildEnv
  
  Write-Host "Get Version"
  $version = Get-UmbracoVersion
  Write-Host "Version $($version.Semver)"

  if ($target -eq "pre-build")
  {
    Prepare-Build $uenv
    #Compile-Belle $uenv $version

    # set environment variables
    $env:UMBRACO_VERSION=$version.Semver.ToString()
    $env:UMBRACO_RELEASE=$version.Release
    $env:UMBRACO_COMMENT=$version.Comment
    $env:UMBRACO_BUILD=$version.Build

    # set environment variable for VSO
    # https://github.com/Microsoft/vsts-tasks/issues/375
    # https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
    Write-Host ("##vso[task.setvariable variable=UMBRACO_VERSION;]$($version.Semver.ToString())")
    Write-Host ("##vso[task.setvariable variable=UMBRACO_RELEASE;]$($version.Release)")
    Write-Host ("##vso[task.setvariable variable=UMBRACO_COMMENT;]$($version.Comment)")
    Write-Host ("##vso[task.setvariable variable=UMBRACO_BUILD;]$($version.Build)")
    
    Write-Host ("##vso[task.setvariable variable=UMBRACO_TMP;]$($uenv.SolutionRoot)\build.tmp")
  }
  elseif ($target -eq "pre-tests")
  {
    Prepare-Tests $uenv
  }
  elseif ($target -eq "compile-tests")
  {
    Compile-Tests $uenv
  }
  elseif ($target -eq "compile-umbraco")
  {
    Compile-Umbraco $uenv $buildConfiguration
  }
  elseif ($target -eq "pre-packages")
  {
    Prepare-Packages $uenv
  }
  elseif ($target -eq "pre-nuget")
  {
    Prepare-NuGet $uenv
  }
  elseif ($target -eq "restore-nuget")
  {
    Restore-NuGet $uenv
  }
  elseif ($target -eq "pkg-zip")
  {
    Package-Zip $uenv
  }
  elseif ($target -eq "compile-belle")
  {
    Compile-Belle $uenv $version
  }
  elseif ($target -eq "prepare-azuregallery")
  {
    Prepare-AzureGallery $uenv
  }
  elseif ($target -eq "all")
  {
    Prepare-Build $uenv
    Restore-NuGet $uenv
    Compile-Belle $uenv $version
    Compile-Umbraco $uenv $buildConfiguration
    Prepare-Tests $uenv
    Compile-Tests $uenv
    # not running tests...
    Prepare-Packages $uenv
    Package-Zip $uenv
    Verify-NuGet $uenv
    Prepare-NuGet $uenv
    Package-NuGet $uenv $version
	Prepare-AzureGallery $uenv
  }
  else
  {
    Write-Error "Unsupported target `"$target`"."
  }
}

#
# export functions
#
Export-ModuleMember -function Get-UmbracoBuildEnv
Export-ModuleMember -function Set-UmbracoVersion
Export-ModuleMember -function Set-UmbracoContinuousVersion
Export-ModuleMember -function Get-UmbracoVersion
Export-ModuleMember -function Build-Umbraco
Export-ModuleMember -function Build-UmbracoDocs
Export-ModuleMember -function Verify-NuGet

#eof