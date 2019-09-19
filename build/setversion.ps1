# Usage: powershell .\setversion.ps1 7.6.8
# Or: powershell .\setversion 7.6.8-beta001

param (
    [Parameter(Mandatory=$true)]
    [string]
    $version
)

# report
Write-Host "Setting Umbraco version to $version"

# import Umbraco Build PowerShell module - $pwd is ./build
$env:PSModulePath = "$pwd\Modules\;$env:PSModulePath"
Import-Module Umbraco.Build -Force -DisableNameChecking

# run commands
$version = Set-UmbracoVersion -Version $version