#

# functions

function loadSemVer($pwd) {
  $semverlib = [System.IO.Path]::Combine($pwd, "..", "src\packages\Semver.2.0.4\lib\net452\Semver.dll")
  if (-not (test-path $semverlib)) {
    write-error "Missing packages\Semver.2.0.4\lib\net452\Semver.dll."
    exit 1
  }
  [Reflection.Assembly]::LoadFile("d:\d\Umbraco 7.6\src\packages\Semver.2.0.4\lib\net452\Semver.dll")
}

# get version

$filepath = [System.IO.Path]::Combine($pwd, "..\src\SolutionInfo.cs")
$text = [System.IO.File]::ReadAllText($filepath)
$match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
$version = $match.Groups[1]
Write-Host "Version: $version"

loadSemVer($pwd)
$semver = [SemVer.SemVersion]::Parse($version)
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

$env:UMBRACO_RELEASE=$release
$env:UMBRACO_COMMENT=$comment
$env:UMBRACO_BUILD=$build

# set environment variable for VSTS
# https://github.com/Microsoft/vsts-tasks/issues/375
# https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md

Write-Host ("##vso[task.setvariable variable=UMBRACO_RELEASE;]$release")
Write-Host ("##vso[task.setvariable variable=UMBRACO_COMMENT;]$comment")
Write-Host ("##vso[task.setvariable variable=UMBRACO_BUILD;]$build")
