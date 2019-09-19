param (
    [Parameter(Mandatory=$false)]
    [string]
    $version,

    [Parameter(Mandatory=$false)]
    [Alias("mo")]
    [switch]
    $moduleOnly = $false
)

# the script can run either from the solution root,
# or from the ./build directory - anything else fails
if ([System.IO.Path]::GetFileName($pwd) -eq "build")
{
  $mpath = [System.IO.Path]::GetDirectoryName($pwd) + "\build\Modules\"
}
else
{
  $mpath = "$pwd\build\Modules\"
}

# look for the module and throw if not found
if (-not [System.IO.Directory]::Exists($mpath + "Umbraco.Build"))
{
  Write-Error "Could not locate Umbraco build Powershell module."
  break
}

# add the module path (if not already there)
if (-not $env:PSModulePath.Contains($mpath))
{
  $env:PSModulePath = "$mpath;$env:PSModulePath"
}

# force-import (or re-import) the module
Write-Host "Import Umbraco build Powershell module"
Import-Module Umbraco.Build -Force -DisableNameChecking

# module only?
if ($moduleOnly)
{
  if (-not [string]::IsNullOrWhiteSpace($version))
  {
    Write-Host "(module only: ignoring version parameter)"
  }
  else
  {
    Write-Host "(module only)"
  }
  break
}

# get build environment
Write-Host "Setup Umbraco build Environment"
$uenv = Get-UmbracoBuildEnv

# set the version if any
if (-not [string]::IsNullOrWhiteSpace($version))
{
  Write-Host "Set Umbraco version to $version"
  Set-UmbracoVersion $version
}

# full umbraco build
Write-Host "Build Umbraco"
Build-Umbraco