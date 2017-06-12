
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


# returns a string containing the hash of $file
function genHash($file) 
{
  try 
  {
    $crypto = new-object System.Security.Cryptography.SHA1CryptoServiceProvider
    $stream = [System.IO.File]::OpenRead($file)
    $hash = $crypto.ComputeHash($stream)
    $text = ""
    $hash | foreach `
    {
      $text = $text + $_.ToString("x2")
    }
    return $text
  }
  finally
  {
    if ($stream)
    {
      $stream.Dispose()
    }
    $crypto.Dispose()
  }
}

# returns the full path if $file is relative to $pwd
function fullPath($file)
{
  $path = [System.IO.Path]::Combine($pwd, $file)
  $path = [System.IO.Path]::GetFullPath($path)
  return $path
}

# removes a directory, doesn't complain if it does not exist
function rmrf($dir)
{
  remove-item $dir -force -recurse -errorAction SilentlyContinue > $null
}

# removes a file, doesn't complain if it does not exist
function rmf($file)
{
  remove-item $file -force -errorAction SilentlyContinue > $null
}

# copies a file, creates target dir if needed
function cpf($source, $target)
{
  $ignore = new-item -itemType file -path $target -force
  cp -force $source $target
}

# copies files to a directory
function cprf($source, $select, $target, $filter)
{
  $files = ls -r "$source\$select"
  $files | foreach {
    $relative = $_.FullName.SubString($source.Length+1)
    $_ | add-member -memberType NoteProperty -name RelativeName -value $relative
  }
  if ($filter -ne $null) {
    $files = $files | where $filter 
  }
  $files |
    foreach {
      if ($_.PsIsContainer) {
        $ignore = new-item -itemType directory -path "$target\$($_.RelativeName)" -force
      }
      else {
        cpf $_.FullName "$target\$($_.RelativeName)"
      }
    }
}

# regex-replaces content in a file
function fileReplace($filename, $source, $replacement) {
  $filepath = fullPath $filename
  $text = [System.IO.File]::ReadAllText($filepath)
  $text = [System.Text.RegularExpressions.Regex]::Replace($text, $source, $replacement)
  $utf8bom = New-Object System.Text.UTF8Encoding $true
  [System.IO.File]::WriteAllText($filepath, $text, $utf8bom)
}

# finds msbuild
function findVisualStudio($vswhere) {
  $vsPath = ""
  $vsVer = ""
  &$vswhere | foreach {
    if ($_.StartsWith("installationPath:")) { $vsPath = $_.SubString("installationPath:".Length).Trim() }
    if ($_.StartsWith("installationVersion:")) { $vsVer = $_.SubString("installationVersion:".Length).Trim() }
  }
  if ($vsPath -eq "") { return $null }
  
  $vsVerParts = $vsVer.Split('.')
  $vsMajor = [int]::Parse($vsVerParts[0])
  $vsMinor = [int]::Parse($vsVerParts[1])
  if ($vsMajor -eq 15) {
    $msBuild = "$vsPath\MSBuild\$vsMajor.$vsMinor\Bin"
  }
  elseif ($vsMajor -eq 14) {
    $msBuild = "c:\Program Files (x86)\MSBuild\$vsMajor\Bin"
  }
  else { return $null }
  $msBuild = "$msBuild\MsBuild.exe"
  
  $vs = new-object -typeName PsObject
  $vs | add-member -memberType NoteProperty -name Path -value $vsPath
  $vs | add-member -memberType NoteProperty -name Major -value $vsMajor
  $vs | add-member -memberType NoteProperty -name Minor -value $vsMinor
  $vs | add-member -memberType NoteProperty -name MsBuild -value $msBuild
  return $vs
}

# initialize environment
# download if needed
function Get-UmbracoBuildEnv
{
  # store tools in the module's directory
  # and cache them for two days
  $path = "$PSScriptRoot\temp"
  $cache = 2
  
  if (-not (test-path $path))
  {
    mkdir $path > $null
  }

  # ensure we have NuGet
  $nuget = "$path\nuget.exe"
  $source = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
  if ((test-path $nuget) -and ((ls $nuget).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    rmf $nuget
  }
  if (-not (test-path $nuget))
  {
    write-host "Download NuGet..."
    $client = new-object Net.WebClient
    $client.DownloadFile($source, $nuget)
  }
  
  # ensure we have 7-Zip
  $sevenZip = "$path\7za.exe"
  if ((test-path $sevenZip) -and ((ls $sevenZip).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    rmf $sevenZip
  }
  if (-not (test-path $sevenZip))
  {
    write-host "Download 7-Zip..."
    &$nuget install 7-Zip.CommandLine -OutputDirectory $path -Verbosity quiet
    $dir = ls "$path\7-Zip.CommandLine.*" | sort -property Name -descending | select -first 1
    $file = ls -path "$dir" -name 7za.exe -recurse
    mv "$dir\$file" $sevenZip
    rmrf $dir
  }
  
  # ensure we have vswhere
  $vswhere = "$path\vswhere.exe"
  if ((test-path $vswhere) -and ((ls $vswhere).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    rmf $vswhere
  }
  if (-not (test-path $vswhere))
  {
    write-host "Download VsWhere..."
    &$nuget install vswhere -OutputDirectory $path -Verbosity quiet
    $dir = ls "$path\vswhere.*" | sort -property Name -descending | select -first 1
    $file = ls -path "$dir" -name vswhere.exe -recurse
    mv "$dir\$file" $vswhere
    rmrf $dir
  }
  
  # ensure we have semver
  $semver = "$path\Semver.dll"
  if ((test-path $semver) -and ((ls $semver).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    rmf $semver
  }
  if (-not (test-path $semver))
  {
    write-host "Download Semver..."
    &$nuget install semver -OutputDirectory $path -Verbosity quiet
    $dir = ls "$path\semver.*" | sort -property Name -descending | select -first 1
    $file = "$dir\lib\net452\Semver.dll"
    if (-not (test-path $file))
    {
      write-error "Failed to file $file"
      return
    }
    mv "$file" $semver
    rmrf $dir
  }
  
  # ensure we have node
  $node = "$path\node-v6.9.1-win-x86"
  $source = "http://nodejs.org/dist/v6.9.1/node-v6.9.1-win-x86.7z"
  if (-not (test-path $node))
  {
  write-host $node
    write-host "Download Node..."
    $client = new-object Net.WebClient
    $client.DownloadFile($source, "$path\node-v6.9.1-win-x86.7z")
    &$sevenZip x "$path\node-v6.9.1-win-x86.7z" -o"$path" -aos > $nul
    rmf "$path\node-v6.9.1-win-x86.7z"    
  }
  
  # ensure we have npm
  $npm = "$path\npm.*"
  $getNpm = $true
  if (test-path $npm)
  {
    $getNpm = $false
    $tmpNpm = ls "$path\npm.*" | sort -property Name -descending | select -first 1
    if ($tmpNpm.CreationTime -lt [DateTime]::Now.AddDays(-$cache))
    {
      $getNpm = $true
    }
    else
    {
      $npm = $tmpNpm.ToString()
    }
  }
  if ($getNpm)
  {
    write-host "Download Npm..."
    &$nuget install npm -OutputDirectory $path -Verbosity quiet
    $npm = ls "$path\npm.*" | sort -property Name -descending | select -first 1
    $npm.CreationTime = [DateTime]::Now
    $npm = $npm.ToString()
  }
  
  # find visual studio
  $vsPath = ""
  $vsVer = ""
  &$vswhere | foreach {
    if ($_.StartsWith("installationPath:")) { $vsPath = $_.SubString("installationPath:".Length).Trim() }
    if ($_.StartsWith("installationVersion:")) { $vsVer = $_.SubString("installationVersion:".Length).Trim() }
  }
  if ($vsPath -eq "") { return $null }
  
  $vsVerParts = $vsVer.Split('.')
  $vsMajor = [int]::Parse($vsVerParts[0])
  $vsMinor = [int]::Parse($vsVerParts[1])
  if ($vsMajor -eq 15) {
    $msBuild = "$vsPath\MSBuild\$vsMajor.$vsMinor\Bin"
  }
  elseif ($vsMajor -eq 14) {
    $msBuild = "c:\Program Files (x86)\MSBuild\$vsMajor\Bin"
  }
  else 
  {
    $msBuild = $null
  }
  
  $vs = $null
  if ($msBuild)
  {
    $vs = new-object -typeName PsObject
    $vs | add-member -memberType NoteProperty -name Path -value $vsPath
    $vs | add-member -memberType NoteProperty -name Major -value $vsMajor
    $vs | add-member -memberType NoteProperty -name Minor -value $vsMinor
    $vs | add-member -memberType NoteProperty -name MsBuild -value "$msBuild\MsBuild.exe"
  }
  
  $solutionRoot = fullPath "$PSScriptRoot\..\..\.."
  
  $uenv = new-object -typeName PsObject
  $uenv | add-member -memberType NoteProperty -name SolutionRoot -value $solutionRoot
  $uenv | add-member -memberType NoteProperty -name VisualStudio -value $vs
  $uenv | add-member -memberType NoteProperty -name NuGet -value $nuget
  $uenv | add-member -memberType NoteProperty -name Zip -value $sevenZip
  $uenv | add-member -memberType NoteProperty -name VsWhere -value $vswhere
  $uenv | add-member -memberType NoteProperty -name Semver -value $semver
  $uenv | add-member -memberType NoteProperty -name NodePath -value $node
  $uenv | add-member -memberType NoteProperty -name NpmPath -value $npm
  
  return $uenv
}

function Set-UmbracoVersion($version)
{
  $uenv = Get-UmbracoBuildEnv
  try
  {
    [Reflection.Assembly]::LoadFile($uenv.Semver) > $null
  }
  catch
  {
    write-error "Failed to load $uenv.Semver"
    break
  }
  
  # validate input
  $ok = [Regex]::Match($version, "^[0-9]+\.[0-9]+\.[0-9]+(\-[a-z0-9]+)?(\+[0-9]+)?$")
  if (-not $ok.Success)
  {
    write-error "Invalid version $version"
    break
  }

  # parse input
  try
  {
    $semver = [SemVer.SemVersion]::Parse($version)
  }
  catch
  {
    write-error "Invalid version $version"
    break
  }
  
  #
  $release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch
  
  # edit files and set the proper versions and dates
  write-host "Update UmbracoVersion.cs"
  fileReplace "$($uenv.SolutionRoot)\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
    "(\d+)\.(\d+)\.(\d+)(.(\d+))?" `
    "$release" 
  fileReplace "$($uenv.SolutionRoot)\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
    "CurrentComment { get { return `"(.+)`"" `
    "CurrentComment { get { return `"$semver.PreRelease`""
  write-host "Update SolutionInfo.cs"
  fileReplace "$($uenv.SolutionRoot)\src\SolutionInfo.cs" `
    "AssemblyFileVersion\(`"(.+)?`"\)" `
    "AssemblyFileVersion(`"$release`")"
  fileReplace "$($uenv.SolutionRoot)\src\SolutionInfo.cs" `
    "AssemblyInformationalVersion\(`"(.+)?`"\)" `
    "AssemblyInformationalVersion(`"$semver`")"
  $year = [System.DateTime]::Now.ToString("yyyy")
  fileReplace "$($uenv.SolutionRoot)\src\SolutionInfo.cs" `
    "AssemblyCopyright\(`"Copyright © Umbraco (\d{4})`"\)" `
    "AssemblyCopyright(`"Copyright © Umbraco $year`")"
   
  # edit csproj and set IIS Express port number
  # this is a raw copy of ReplaceIISExpressPortNumber.exe
  # it probably can be achieved in a much nicer way - l8tr
  $source = @"
  using System;
  using System.IO;
  using System.Xml;
  using System.Globalization;

  namespace Umbraco
  {
    public static class PortUpdater
    {
      public static void Update(string path, string release)
      {
        XmlDocument xmlDocument = new XmlDocument();
        string fullPath = Path.GetFullPath(path);
        xmlDocument.Load(fullPath);
        int result = 1;
        int.TryParse(release.Replace(`".`", `"`"), out result);
        while (result < 1024)
          result *= 10;
        XmlNode xmlNode1 = xmlDocument.GetElementsByTagName(`"IISUrl`").Item(0);
        if (xmlNode1 != null)
          xmlNode1.InnerText = `"http://localhost:`" + (object) result;
        XmlNode xmlNode2 = xmlDocument.GetElementsByTagName(`"DevelopmentServerPort`").Item(0);
        if (xmlNode2 != null)
          xmlNode2.InnerText = result.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        xmlDocument.Save(fullPath);
      }
    }
  }
"@

  $assem = (
    "System.Xml",
    "System.IO",
    "System.Globalization"
  )

  write-host "Update Umbraco.Web.UI.csproj"
  add-type -referencedAssemblies $assem -typeDefinition $source -language CSharp
  $csproj = "$($uenv.SolutionRoot)\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj"
  [Umbraco.PortUpdater]::Update($csproj, $release)

  return $semver
}

function Get-UmbracoVersion()
{  
  $uenv = Get-UmbracoBuildEnv
  try
  {
    [Reflection.Assembly]::LoadFile($uenv.Semver) > $null
  }
  catch
  {
    write-error "Failed to load $uenv.Semver"
    break
  }
  
  # parse SolutionInfo and retrieve the version string
  $filepath = "$($uenv.SolutionRoot)\src\SolutionInfo.cs"
  $text = [System.IO.File]::ReadAllText($filepath)
  $match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
  $version = $match.Groups[1]
  write-host "AssemblyInformationalVersion: $version"

  # semver-parse the version string
  $semver = [SemVer.SemVersion]::Parse($version)
  $release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch

  $versions = new-object -typeName PsObject
  $versions | add-member -memberType NoteProperty -name Semver -value $semver
  $versions | add-member -memberType NoteProperty -name Release -value $release
  $versions | add-member -memberType NoteProperty -name Comment -value $semver.PreRelease
  $versions | add-member -memberType NoteProperty -name Build -value $semver.Build
  
  return $versions
}

function Build-Pre($uenv)
{
  write-host "Pre-Compile"
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"

  # clear
  write-host "Clear folders and files"

  rmrf "$src\Umbraco.Web.UI.Client\build"
  rmrf "$src\Umbraco.Web.UI.Client\bower_components"

  rmrf "$tmp"
  mkdir "$tmp" > $null
  
  rmrf "$out"
  mkdir "$out" > $null

  # prepare
  # fixme - if we have a completely weird local one, it will
  # use it? this is bad, should always use the proper one!
  write-host "Making sure we have a clean web.config"

  $webUi = "$src\Umbraco.Web.UI"
  if (test-path "$webUi\web.config")
  {
    write-host "Saving existing web.config to web.config.temp-build"
    mv "$webUi\web.config" "$webUi\web.config.temp-build"
  }
  cpf "$webUi\web.Template.config" "$webUi\web.config"
}

function Build-Belle($uenv, $version)
{
  $tmp = "$($uenv.SolutionRoot)\build.tmp"

  write-host "Build Belle (logging to $tmp\belle.log)"

  push-location "$($uenv.SolutionRoot)\src\Umbraco.Web.UI.Client"
  $p = $env:path
  $env:path = $uenv.NpmPath + ";" + $uenv.NodePath + ";" + $env:path
  
  write "cache clean" > $tmp\belle.log
  &npm cache clean --quiet >> $tmp\belle.log 2>&1
  &npm install --quiet >> $tmp\belle.log 2>&1
  &npm install -g grunt-cli --quiet >> $tmp\belle.log 2>&1
  &npm install -g bower --quiet >> $tmp\belle.log 2>&1
  &grunt build --buildversion=$version.Release >> $tmp\belle.log 2>&1
  
  # fixme - should we filter the log to find errors?
  #get-content .\build.tmp\belle.log | %{ if ($_ -match "build") { write $_}}
  
  pop-location
  $env:path = $p
}

function Build-UmbracoBelle()
{
  $uenv = Get-UmbracoBuildEnv
  $version = Get-UmbracoVersion
  Build-Belle $uenv $version
}

function Build-Compile($uenv)
{
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"

  $buildConfiguration = "Release"
  
  $toolsVersion = "4.0"
  if ($uenv.VisualStudio.Major -eq 15)
  {
    $toolsVersion = "15.0"
  }
    
  write-host "Compile (logging to $tmp\msbuild.log)"

  # beware of the weird double \\ at the end of paths
  # see http://edgylogic.com/blog/powershell-and-external-commands-done-right/
  &$uenv.VisualStudio.MsBuild "$src\Umbraco.Web.UI\Umbraco.Web.UI.csproj" `
    /p:WarningLevel=0 `
    /p:Configuration=$buildConfiguration `
    /p:UseWPP_CopyWebApplication=True `
    /p:PipelineDependsOnBuild=False `
    /p:OutDir=$tmp\bin\\ `
    /p:WebProjectOutputDir=$tmp\WebApp\\ `
    /p:Verbosity=minimal `
    /t:Clean`;Rebuild `
    /tv:$toolsVersion `
    /p:UmbracoBuild=True `
    > $tmp\msbuild.log
    
  # /p:UmbracoBuild tells the csproj that we are building from PS
}

function Build-Post($uenv)
{
  write-host "Post-Compile" 
  
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"

  $buildConfiguration = "Release"

  # restore web.config
  $webUi = "$src\Umbraco.Web.UI"
  if (test-path "$webUi\web.config.temp-build")
  {
    write-host "Restoring existing web.config"
    rmf "$webUi\web.config"
    mv "$webUi\web.config.temp-build" "$webUi\web.config"
  }

  # cleanup build
  write "Clean build"

  rmf "$tmp\bin\*.dll.config"
  rmf "$tmp\WebApp\bin\*.dll.config"

  # cleanup presentation
  write "Cleanup presentation"

  rmrf "$tmp\WebApp\umbraco.presentation"

  # create directories
  write "Create directories"

  mkdir "$tmp\Configs" > $null
  mkdir "$tmp\Configs\Lang" > $null
  mkdir "$tmp\WebApp\App_Data" > $null
  #mkdir "$tmp\WebApp\Media" > $null
  #mkdir "$tmp\WebApp\Views" > $null

  # copy various files
  write "Copy xml documentation"

  cp -force "$tmp\bin\*.xml" "$tmp\WebApp\bin"

  write "Copy transformed configs and langs"

  cprf "$tmp\WebApp\config" "*.config" "$tmp\Configs"
  cprf "$tmp\WebApp\config" "*.js" "$tmp\Configs"
  cprf "$tmp\WebApp\config\lang" "*.xml" "$tmp\Configs\Lang"
  cpf "$tmp\WebApp\web.config" "$tmp\Configs\web.config.transform"

  write "Copy transformed web.config"

  cpf "$src\Umbraco.Web.UI\web.$buildConfiguration.Config.transformed" "$tmp\WebApp\web.config"

  # offset the modified timestamps on all umbraco dlls, as WebResources
  # break if date is in the future, which, due to timezone offsets can happen.
  write "Offset dlls timestamps"
  ls -r "$tmp\*.dll" | foreach {
    $_.CreationTime = $_.CreationTime.AddHours(-11)
    $_.LastWriteTime = $_.LastWriteTime.AddHours(-11)
  }

  # copy libs
  write "Copy SqlCE libraries"

  cprf "$src\packages\SqlServerCE.4.0.0.1" "*.*" "$tmp\bin" `
    { -not $_.Extension.StartsWith(".nu") -and -not $_.RelativeName.StartsWith("lib\") }
  cprf "$src\packages\SqlServerCE.4.0.0.1" "*.*" "$tmp\WebApp\bin" `
    { -not $_.Extension.StartsWith(".nu") -and -not $_.RelativeName.StartsWith("lib\") }
          
  # copy Belle
  write "Copy Belle"

  cprf "$src\Umbraco.Web.UI.Client\build\belle" "*" "$tmp\WebApp\umbraco" `
    { -not ($_.RelativeName -eq "index.html") }

  # zip webapp
  write "Zip WebApp"

  &$uenv.Zip a -r "$out\UmbracoCms.AllBinaries.$version.zip" `
    "$tmp\bin\*" `
    -x!dotless.Core.dll `
    > $null
    
  &$uenv.Zip a -r "$out\UmbracoCms.$version.zip" `
    "$tmp\WebApp\*" `
    -x!dotless.Core.dll -x!Content_Types.xml `
    > $null

  # prepare and zip WebPI
  write "Zip WebPI"

  rmrf "$tmp\WebPi"
  mkdir "$tmp\WebPi" > $null
  mkdir "$tmp\WebPi\umbraco" > $null

  cprf "$tmp\WebApp" "*" "$tmp\WebPi\umbraco"
  cprf "$src\WebPi" "*" "$tmp\WebPi"
      
  &$uenv.Zip a -r "$out\UmbracoCms.WebPI.$($version.Semver).zip" `
    "$tmp\WebPi\*" `
    -x!dotless.Core.dll `
    > $null
    
  # clear
  write "Delete build folder"
  #rmrf "$tmp"

  # hash the webpi file
  write "Hash the WebPI file"

  $hash = genHash "$out\UmbracoCms.WebPI.$($version.Semver).zip"
  write $hash | out-file "$out\webpihash.txt" -encoding ascii

  # setting node_modules folder to hidden to prevent VS13 from
  # crashing on it while loading the websites project (still needed?)
  # use get-item -force 'cos it might be hidden already
  write "Set hidden attribute on node_modules"

  $dir = get-item -force "$src\Umbraco.Web.UI.Client\node_modules"
  $dir.Attributes = $dir.Attributes -bor ([System.IO.FileAttributes]::Hidden)

  # add Web.config transform files to the NuGet package
  write "Add web.config transforms to NuGet package"

  mv "$tmp\WebApp\Views\Web.config" "$tmp\WebApp\Views\Web.config.transform"
  
  # fixme - that one does not exist in .bat build either?
  #mv "$tmp\WebApp\Xslt\Web.config" "$tmp\WebApp\Xslt\Web.config.transform"
}

function Build-NuGet($uenv)
{
  $src = "$($uenv.SolutionRoot)\src"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"
  $out = "$($uenv.SolutionRoot)\build.out"
  $nuspecs = "$($uenv.SolutionRoot)\build\NuSpecs"
  
  $version = Get-UmbracoVersion

  write-host "Create NuGet packages"

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

function Build-Umbraco($target)
{
  $uenv = Get-UmbracoBuildEnv
  $version = Get-UmbracoVersion
  
  # set environment variables
  #$env:UMBRACO_VERSION=$version.Semver.ToString()
  #$env:UMBRACO_RELEASE=$version.Release
  #$env:UMBRACO_COMMENT=$version.Comment
  #$env:UMBRACO_BUILD=$version.Build

  # set environment variable for VSO
  # https://github.com/Microsoft/vsts-tasks/issues/375
  # https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
  if ($vso)
  {
    write-host ("##vso[task.setvariable variable=UMBRACO_VERSION;]$($version.Semver.ToString())")
    write-host ("##vso[task.setvariable variable=UMBRACO_RELEASE;]$($version.Release)")
    write-host ("##vso[task.setvariable variable=UMBRACO_COMMENT;]$($version.Comment)")
    write-host ("##vso[task.setvariable variable=UMBRACO_BUILD;]$($version.Build)")
  }
  
  Build-Pre $uenv
  Build-Belle $uenv $version
  Build-Compile $uenv
  Build-Post $uenv
  Build-NuGet $uenv
}

Export-ModuleMember -function Get-UmbracoBuildEnv
Export-ModuleMember -function Set-UmbracoVersion
Export-ModuleMember -function Get-UmbracoVersion
Export-ModuleMember -function Build-Umbraco
Export-ModuleMember -function Build-UmbracoBelle

# fixme
# stop using relative paths, need $uenv.SolutionRoot
# build-pre ok (test?)
# build-belle ok (needs better error detection?)
# build-compile
#  imports .nuget targets wtf <<<<<<<<<<<<<<<
# build-post ?
# build-nuget ?