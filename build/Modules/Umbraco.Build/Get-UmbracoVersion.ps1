#
# Get-UmbracoVersion
# Gets the Umbraco version
#
function Get-UmbracoVersion
{  
  $uenv = Get-UmbracoBuildEnv
  
  # parse SolutionInfo and retrieve the version string
  $filepath = "$($uenv.SolutionRoot)\src\SolutionInfo.cs"
  $text = [System.IO.File]::ReadAllText($filepath)
  $match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
  $version = $match.Groups[1]

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
