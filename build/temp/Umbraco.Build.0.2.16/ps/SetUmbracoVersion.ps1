
$ubuild.DefineMethod("SetUmbracoVersion",
{
  param (
    [Parameter(Mandatory=$true)]
    [string] $version
  )

  try
  {
    Add-Type -Path $this.BuildEnv.Semver > $null
  }
  catch
  {
    throw "Failed to load $this.BuildEnv.Semver."
  }

  # validate input
  $ok = [Regex]::Match($version, "^[0-9]+\.[0-9]+\.[0-9]+(\-[a-z0-9\.]+)?(\+[0-9]+)?$")
  if (-not $ok.Success)
  {
    throw "Invalid version $version."
  }

  # parse input
  try
  {
    $semver = [SemVer.SemVersion]::Parse($version)
  }
  catch
  {
    throw "Invalid version $version."
  }

  #
  $release = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch

  # edit files and set the proper versions and dates
  Write-Host "Update SolutionInfo.cs"
  $this.ReplaceFileText("$($this.SolutionRoot)\src\SolutionInfo.cs", `
    "AssemblyFileVersion\(`".+`"\)", `
    "AssemblyFileVersion(`"$release`")")
  $this.ReplaceFileText("$($this.SolutionRoot)\src\SolutionInfo.cs", `
    "AssemblyInformationalVersion\(`".+`"\)", `
    "AssemblyInformationalVersion(`"$semver`")")
  $year = [System.DateTime]::Now.ToString("yyyy")
  $this.ReplaceFileText("$($this.SolutionRoot)\src\SolutionInfo.cs", `
    "AssemblyCopyright\(`"Copyright © Umbraco (\d{4})`"\)", `
    "AssemblyCopyright(`"Copyright © Umbraco $year`")")

  $this.Version = @{
    Semver = $semver
    Release = $release
    Comment = $semver.PreRelease
    Build = $semver.Build
  }

  if ($this.HasMethod("SetMoreUmbracoVersion"))
  {
    $this.SetMoreUmbracoVersion($semver)
  }

  return $this.Version
})
