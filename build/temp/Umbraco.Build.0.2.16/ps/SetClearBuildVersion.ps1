
# continous:
# 1.2.3          1.2.3-aleph.20171012.0001 @abcdef+
# 1.2.3-alpha    1.2.3-alpha.0.20171012.0001 @abcdef+
# 1.2.3-alpha.3  1.2.3-alpha.3.20171012.0001 @abcdef+
#
# release:
# 1.2.3          1.2.3 @abcdef+
# 1.2.3-alpha    1.2.3-alpha @abcdef+
# 1.2.3-alpha.3  1.2.3-alpha.3 @abcdef+
#
# Semver-wise, this means that each continuous build of a pre-release version
# comes *after* that version, ie -alpha.0.20171012.0001 > -alpha, and so we
# should upgrade the alpha/beta number at the moment we release it

$ubuild.DefineMethod("SetBuildVersion",
{
  $buildNumber = $this.BuildNumber
  if (-not $buildNumber)
  {
    $buildNumber = [DateTime]::Now.ToString("yyyyMMdd") + ".0"
    $this.BuildNumber = $buildNumber
  }

  $releaseBranches = $this.ReleaseBranches
  if (-not $releaseBranches) { $releaseBranches = @() }

  # parse SolutionInfo and retrieve the version string
  $filepath = "$($this.SolutionRoot)\src\SolutionInfo.cs"
  $text = [System.IO.File]::ReadAllText($filepath)
  $match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
  $version = $match.Groups[1].ToString()

  # clear
  $match = [System.Text.RegularExpressions.Regex]::Matches($version, "([0-9]+\.[0-9]+\.[0-9]+)((-[0-9a-z]+(\.[0-9]+)?))?")
  $version = $match.Groups[0].ToString()

  $haspre = $match.Groups[2].ToString().Length -ne 0
  $hasprenum = $match.Groups[4].ToString().Length -ne 0

  # get git stuff
  # note that rev-parse returns 'HEAD' not 'master' on VS Online, have to use env var
  $githash = &git rev-parse --short HEAD
  $gitbranch = $env:BUILD_SOURCEBRANCHNAME
  if (-not $gitbranch) { $gitbranch = &git rev-parse --abbrev-ref HEAD }
  $gitstatus = &git status -uno -s

  # get build
  $build = ""
  $isContinuous = -not $releaseBranches.Contains($gitbranch)
  if ($isContinuous)
  {
    if (-not $haspre) { $build = $build + "-aleph" }
    elseif (-not $hasprenum) { $build = $build + ".0"}
    $build = $build + "." + $buildNumber
  }

  # figure out local changes
  # does not take ?? files in account (only M,D,A...)
  $dirty = ""
  if ($gitstatus) { $dirty = "+" }

  # update SolutionInfo with completed version string
  $this.ReplaceFileText("$($this.SolutionRoot)\src\SolutionInfo.cs", `
    "AssemblyInformationalVersion\(`".+`"\)", `
    "AssemblyInformationalVersion(`"$version$build @$githash$dirty`")")
})

$ubuild.DefineMethod("ClearBuildVersion",
{
  # parse SolutionInfo and retrieve the version string
  $filepath = "$($this.SolutionRoot)\src\SolutionInfo.cs"
  $text = [System.IO.File]::ReadAllText($filepath)
  $match = [System.Text.RegularExpressions.Regex]::Matches($text, "AssemblyInformationalVersion\(`"(.+)?`"\)")
  $version = $match.Groups[1].ToString()

  # clear
  $match = [System.Text.RegularExpressions.Regex]::Matches($version, "([0-9]+\.[0-9]+\.[0-9]+)((-[0-9a-z]+(\.[0-9]+)?))?")
  $version = $match.Groups[0].ToString()

  # update SolutionInfo with cleared version string
  $this.ReplaceFileText("$($this.SolutionRoot)\src\SolutionInfo.cs", `
    "AssemblyInformationalVersion\(`".+`"\)", `
    "AssemblyInformationalVersion(`"$version`")")
})
