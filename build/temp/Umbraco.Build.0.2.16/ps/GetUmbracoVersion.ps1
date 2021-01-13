
$ubuild.DefineMethod("GetUmbracoVersion",
{
  # parse SolutionInfo and retrieve the version string
  $filepath = "$($this.SolutionRoot)\src\SolutionInfo.cs"
  $text = [System.IO.File]::ReadAllText($filepath)
  $match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
  $version = $match.Groups[1].ToString()

  # clear
  $pos = $version.IndexOf(' ')
  if ($pos -gt 0) { $version = $version.Substring(0, $pos) }

  # semver-parse the version string
  $semver = [SemVer.SemVersion]::Parse($version)
  $release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch

  $versions = @{
    Semver = $semver
    Release = $release
    Comment = $semver.PreRelease
    Build = $semver.Build
  }

  return $versions
})
