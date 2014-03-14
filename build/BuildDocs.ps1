##We cannot continue if sandcastle is not installed determined by env variable: SHFBROOT

if (-not (Test-Path Env:\SHFBROOT))
{
	throw "The docs cannot be build, install Sandcastle help file builder"
}

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName
$BuildRoot = Split-Path -Path $PSScriptFilePath -Parent
$OutputPath = Join-Path -Path $BuildRoot -ChildPath "ApiDocs\Output"
$ProjFile = Join-Path -Path $BuildRoot -ChildPath "ApiDocs\csharp-api-docs.shfbproj"

"Building docs with project file:  $ProjFile"

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

# build it!
& $MSBuild "$ProjFile"

# remove files left over
Remove-Item $BuildRoot\* -include csharp-api-docs.shfbproj_*

# copy our custom styles in
Copy-Item $BuildRoot\ApiDocs\TOC.css $OutputPath\TOC.css

""
"Done!"