#
# get-version
#  extracts version from SolutionInfo.cs
#  and populates UMBRACO_... environment vars
#

# import build module
import-module ".\build-module.psm1"

# parse SolutionInfo and retrieve the version string
$filepath = $(fullPath "..\src\SolutionInfo.cs")
$text = [System.IO.File]::ReadAllText($filepath)
$match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
$version = $match.Groups[1]
Write-Host "Text version: $version"

# semver-parse the version string
loadSemVer
$semver = [SemVer.SemVersion]::Parse($version)
write "Sem version:  $semver"

# extract release, comment and build back from semver
# rebuild a clean version string
$release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch
$comment = $semver.PreRelease
$build = $semver.Build
if ($release -eq "") {
  Write-Error "Could not get version."
  Exit 1
}
$version = $release
if ($comment -ne "") {
  $version = "$version-$comment"
}
if ($build -ne "") {
  $version = "$version+$build"
}
# obtain the file version from the semver
$filever = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch
if ($build -ne "") {
  $filever = "$filever.$build"
}
write "File version: $filever"

# set environment variables
$env:UMBRACO_VERSION=$version
$env:UMBRACO_RELEASE=$release
$env:UMBRACO_COMMENT=$comment
$env:UMBRACO_BUILD=$build

# set environment variable for VSO
# https://github.com/Microsoft/vsts-tasks/issues/375
# https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
Write-Host ("##vso[task.setvariable variable=UMBRACO_VERSION;]$version")
Write-Host ("##vso[task.setvariable variable=UMBRACO_RELEASE;]$release")
Write-Host ("##vso[task.setvariable variable=UMBRACO_COMMENT;]$comment")
Write-Host ("##vso[task.setvariable variable=UMBRACO_BUILD;]$build")

# eof