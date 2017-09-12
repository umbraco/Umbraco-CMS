#
# Set-UmbracoContinuousVersion
# Sets the Umbraco version for continuous integration
#
#   -Version <version>
#   where <version> is a Semver valid version
#   eg 1.2.3, 1.2.3-alpha, 1.2.3-alpha+456
#
#   -BuildNumber <buildNumber>
#   where <buildNumber> is an integer coming from the build server
#
function Set-UmbracoContinuousVersion
{
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $version,
    [Parameter(Mandatory=$true)]
    [int]
    $buildNumber
  )
  
  $uenv = Get-UmbracoBuildEnv
    
  Write-Host "Version is currently set to $version"
  
  $versionComment = $buildNumber
  $versionComment = $versionComment.ToString().PadLeft(4,"0")
  $umbracoVersion = "$($version.Trim())-alpha$versionComment"
  
  Write-Host "Setting Umbraco Version to $umbracoVersion"
  Set-UmbracoVersion $umbracoVersion
}