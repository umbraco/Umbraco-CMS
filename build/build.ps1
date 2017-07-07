param (
    [Parameter(Mandatory=$false)]
    [string]
    $version
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
  Write-Error "Could not locate Umbraco.Build Powershell module."
  break
}

# add the module path (if not already there)
if (-not $env:PSModulePath.Contains($mpath))
{
  $env:PSModulePath = "$mpath;$env:PSModulePath"
}

# force-import (or re-import) the module
Import-Module Umbraco.Build -Force -DisableNameChecking

# set the version if any
if (-not [string]::IsNullOrWhiteSpace($version))
{
  Set-UmbracoVersion $version
}

# full umbraco build
Build-Umbraco