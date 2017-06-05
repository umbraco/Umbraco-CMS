
# build-module.psm1
#
# import-module .\build-module.psm1
# get-module
# remove-module build-module

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
  remove-item $dir -force -recurse -errorAction SilentlyContinue
}

# removes a file, doesn't complain if it does not exist
function rmf($file)
{
  remove-item $file -force -errorAction SilentlyContinue
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
    mkdir $path
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
    $dir = ls $path\7-Zip.CommandLine.* | sort -property Name -descending | select -first 1
    $file = ls -path $dir -name 7za.exe -recurse
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
    $dir = ls $path\vswhere.* | sort -property Name -descending | select -first 1
    $file = ls -path $dir -name vswhere.exe -recurse
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
    $dir = ls $path\semver.* | sort -property Name -descending | select -first 1
    $file = "$dir\lib\net452\Semver.dll"
    if (-not (test-path $file))
    {
      write-error "Failed to file $file"
      return
    }
    mv "$file" $semver
    rmrf $dir
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
  $uenv | add-member -memberType NoteProperty -name NuGet -value $nuget
  $uenv | add-member -memberType NoteProperty -name Zip -value $sevenZip
  $uenv | add-member -memberType NoteProperty -name VsWhere -value $vswhere
  $uenv | add-member -memberType NoteProperty -name Semver -value $semver
  $uenv | add-member -memberType NoteProperty -name VisualStudio -value $vs
  
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

function Build-Umbraco()
{
  write-host "so what?"
}

function Test-UmbracoModule()
{
  write-host "bam"
  return 123
}

Export-ModuleMember -function Get-UmbracoBuildEnv
Export-ModuleMember -function Set-UmbracoVersion
Export-ModuleMember -function Get-UmbracoVersion
Export-ModuleMember -function Build-Umbraco

# fixme
# stop using relative paths, need $uenv.SolutionRoot