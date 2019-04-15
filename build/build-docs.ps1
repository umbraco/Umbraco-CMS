$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path);
$RepoRoot = (get-item $PSScriptFilePath).Directory.Parent.FullName;
$SolutionRoot = Join-Path -Path $RepoRoot "src";
$ToolsRoot = Join-Path -Path $RepoRoot "\build\temp";
$DocFx = Join-Path -Path $ToolsRoot "docfx\docfx.exe"
$DocFxFolder = (Join-Path -Path $ToolsRoot "docfx")
$DocFxJson = Join-Path -Path $SolutionRoot "ApiDocs\docfx.json"
$7Zip = Join-Path -Path $ToolsRoot "7za.exe"
$DocFxSiteOutput = Join-Path -Path $SolutionRoot "ApiDocs\_site\*.*"
#$NgDocsSiteOutput = Join-Path -Path $RepoRoot "src\Umbraco.Web.UI.Client\docs\api\*.*"
$ProgFiles86 = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)");
$MSBuild = "$ProgFiles86\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"

<#
################ Do the UI docs

"Changing to Umbraco.Web.UI.Client folder"
cd ..
cd src\Umbraco.Web.UI.Client
Write-Host $(Get-Location)

"Creating build folder so MSBuild doesn't run the whole grunt build"
if (-Not (Test-Path "build")) {
    md "build"
}

"Installing node"
# Check if Install-Product exists, should only exist on the build server
if (Get-Command Install-Product -errorAction SilentlyContinue)
{
    Install-Product node ''
}

"Installing node modules"
& npm install

"Installing grunt"
& npm install -g grunt-cli

"Moving back to build folder"
cd ..
cd ..
cd build
Write-Host $(Get-Location)

 & grunt --gruntfile ../src/umbraco.web.ui.client/gruntfile.js docs

# change baseUrl
$BaseUrl = "https://our.umbraco.org/apidocs/ui/"
$IndexPath = "../src/umbraco.web.ui.client/docs/api/index.html"
(Get-Content $IndexPath).replace('location.href.replace(rUrl, indexFile)', "`'" + $BaseUrl + "`'") | Set-Content $IndexPath
# zip it

& $7Zip a -tzip ui-docs.zip $NgDocsSiteOutput -r

#>

################ Do the c# docs

# Build the solution in debug mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "umbraco.sln"

# Go get nuget.exe if we don't have it
$NuGet = "$ToolsRoot\nuget.exe"
$FileExists = Test-Path $NuGet
If ($FileExists -eq $False) {
	Write-Host "Retrieving nuget.exe..."
	$SourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
	Invoke-WebRequest $SourceNugetExe -OutFile $NuGet
}

#restore nuget packages
Write-Host "Restoring nuget packages..."
& $NuGet restore $SolutionPath

& $MSBuild "$SolutionPath" /p:Configuration=Debug /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}
& $MSBuild "$SolutionPath" /p:Configuration=Debug /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

# Go get docfx if we don't have it
$FileExists = Test-Path $DocFx 
If ($FileExists -eq $False) {

	If(!(Test-Path $DocFxFolder))
	{
		New-Item $DocFxFolder -type directory
	}	
	
	Write-Host "Getting docfx..."
	$DocFxZip = Join-Path -Path $ToolsRoot "docfx\docfx.zip"
	$DocFxSource = "https://github.com/dotnet/docfx/releases/download/v2.41/docfx.zip"
	# Create SSL/TLS secure channel to access GitHub API endpoint 
	[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	Invoke-WebRequest $DocFxSource -OutFile $DocFxZip

	#unzip it	
	& $7Zip e $DocFxZip "-o$DocFxFolder"
}

#clear site
If(Test-Path(Join-Path -Path $RepoRoot "ApiDocs\_site"))
{
	Remove-Item $DocFxSiteOutput -recurse
}

# run it!
& $DocFx metadata $DocFxJson
& $DocFx $DocFxJson --serve

# zip it

& $7Zip a -tzip csharp-docs.zip $DocFxSiteOutput -r