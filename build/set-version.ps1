#
# set-version
#  updates the various files (SolutionInfo.cs...)
#  with the supplied version infos
#
#  release: eg 7.6.0
#  comment: eg alpha074, or missing
#  build: eg 1234, or missing
#
param (
  [parameter(mandatory=$true)]
  [string]
  [alias("r")]
  $release,

  [string]
  [alias("c")]
  $comment,

  [int]
  [alias("b")]
  $build = 0

  #[bool]$p3 = $true,
  #[switch][alias("sw")]$p5 = $false
)

# import build module
import-module ".\build-module.psm1"
loadSemVer

# validate params and get version string
$release = $release.Trim()
$version = $release
if ($version -eq "") {
  write-error "Invalid release."
  exit 1
}
$comment = $comment.Trim()
if ($comment -ne "") {
  $version = "$version-$comment"
}
if ($build -ne 0) {
  $version = "$version+$build"
}
write "Text version: $version"

# semver-parse the version string
# just to ensure that it can be parsed
$semver = [SemVer.SemVersion]::Parse($version)
write "Sem version:  $semver"

# obtain the file version from the semver
$filever = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch
if ($build -ne 0) {
  $filever = "$filever.$build"
}
write "File version: $filever"

# edit files and set the proper versions and dates
write "Update UmbracoVersion.cs"
fileReplace "..\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
  "(\d+)\.(\d+)\.(\d+)(.(\d+))?" `
  "$filever" 
fileReplace "..\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
  "CurrentComment { get { return `"(.+)`"" `
  "CurrentComment { get { return `"$comment`""
write "Update SolutionInfo.cs"
fileReplace "..\src\SolutionInfo.cs" `
  "AssemblyFileVersion\(`"(.+)?`"\)" `
  "AssemblyFileVersion(`"$filever`")"
fileReplace "..\src\SolutionInfo.cs" `
  "AssemblyInformationalVersion\(`"(.+)?`"\)" `
  "AssemblyInformationalVersion(`"$semver`")"
$year = [System.DateTime]::Now.ToString("yyyy")
fileReplace "..\src\SolutionInfo.cs" `
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

write "Update Umbraco.Web.UI.csproj"
add-type -referencedAssemblies $assem -typeDefinition $source -language CSharp
$csproj = fullPath "..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj"
[Umbraco.PortUpdater]::Update($csproj, $release)

# eof