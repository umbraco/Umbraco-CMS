#
# Set-UmbracoVersion
# Sets the Umbraco version
#
#   -Version <version>
#   where <version> is a Semver valid version
#   eg 1.2.3, 1.2.3-alpha, 1.2.3-alpha+456
#
function Set-UmbracoVersion
{
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $version
  )
  
  $uenv = Get-UmbracoBuildEnv
  
  try
  {
    [Reflection.Assembly]::LoadFile($uenv.Semver) > $null
  }
  catch
  {
    Write-Error "Failed to load $uenv.Semver"
    break
  }
  
  # validate input
  $ok = [Regex]::Match($version, "^[0-9]+\.[0-9]+\.[0-9]+(\-[a-z0-9]+)?(\+[0-9]+)?$")
  if (-not $ok.Success)
  {
    Write-Error "Invalid version $version"
    break
  }

  # parse input
  try
  {
    $semver = [SemVer.SemVersion]::Parse($version)
  }
  catch
  {
    Write-Error "Invalid version $version"
    break
  }
  
  #
  $release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch
  
  # edit files and set the proper versions and dates
  Write-Host "Update UmbracoVersion.cs"
  Replace-FileText "$($uenv.SolutionRoot)\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
    "(\d+)\.(\d+)\.(\d+)(.(\d+))?" `
    "$release" 
  Replace-FileText "$($uenv.SolutionRoot)\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
    "CurrentComment { get { return `"(.+)`"" `
    "CurrentComment { get { return `"$($semver.PreRelease)`""
  Write-Host "Update SolutionInfo.cs"
  Replace-FileText "$($uenv.SolutionRoot)\src\SolutionInfo.cs" `
    "AssemblyFileVersion\(`"(.+)?`"\)" `
    "AssemblyFileVersion(`"$release`")"
  Replace-FileText "$($uenv.SolutionRoot)\src\SolutionInfo.cs" `
    "AssemblyInformationalVersion\(`"(.+)?`"\)" `
    "AssemblyInformationalVersion(`"$semver`")"
  $year = [System.DateTime]::Now.ToString("yyyy")
  Replace-FileText "$($uenv.SolutionRoot)\src\SolutionInfo.cs" `
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

  Write-Host "Update Umbraco.Web.UI.csproj"
  add-type -referencedAssemblies $assem -typeDefinition $source -language CSharp
  $csproj = "$($uenv.SolutionRoot)\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj"
  [Umbraco.PortUpdater]::Update($csproj, $release)

  return $semver
}
