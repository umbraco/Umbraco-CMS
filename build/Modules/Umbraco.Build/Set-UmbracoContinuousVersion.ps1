#
# Set-UmbracoContinuousVersion
# Sets the Umbraco version for continuous integration
#
#   -Version <version>
#   where <version> is a Semver valid version
#   eg 1.2.3, 1.2.3-alpha, 1.2.3-alpha+456
#
#   -BuildNumber <buildNumber>
#   where <buildNumber> is a string coming from the build server
#   eg 34, 126, 1
#
function Set-UmbracoContinuousVersion
{
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $version,
    [Parameter(Mandatory=$true)]
    [string]
    $buildNumber
  )
  
  Write-Host "Version is currently set to $version"

  $umbracoVersion = "$($version.Trim())-alpha$($buildNumber)"
  Write-Host "Setting Umbraco Version to $umbracoVersion"

  Set-UmbracoVersion $umbracoVersion
}