$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path);
$RepoRoot = (get-item $PSScriptFilePath).Directory.Parent.FullName;
$SolutionRoot = Join-Path -Path $RepoRoot "src";
$ToolsRoot = Join-Path -Path $RepoRoot "tools";
$DocFx = Join-Path -Path $ToolsRoot "docfx\docfx.exe"
$DocFxFolder = (Join-Path -Path $ToolsRoot "docfx")
$DocFxJson = Join-Path -Path $RepoRoot "apidocs\docfx.json"
$7Zip = Join-Path -Path $ToolsRoot "7zip\7za.exe"
$DocFxSiteOutput = Join-Path -Path $RepoRoot "apidocs\_site\*.*"
$NgDocsSiteOutput = Join-Path -Path $RepoRoot "src\Umbraco.Web.UI.Client\docs\api\*.*"

$ProgFiles86 = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)");
$MSBuild = "$ProgFiles86\MSBuild\14.0\Bin\MSBuild.exe"


################ Do the UI docs
Install-Product node 4.4.5
"Installing node"
& npm install -g npm
"Installing grunt"
& npm install -g grunt-cli

& grunt --gruntfile ../src/umbraco.web.ui.client/gruntfile.js docs

# zip it

& $7Zip a -tzip ui-docs.zip $NgDocsSiteOutput -r

################ Do the c# docs

# Build the solution in debug mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "umbraco.sln"
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

# Go get docfx if we don't hae it
$FileExists = Test-Path $DocFx 
If ($FileExists -eq $False) {

	If(!(Test-Path $DocFxFolder))
	{
		New-Item $DocFxFolder -type directory
	}	
	
	$DocFxZip = Join-Path -Path $ToolsRoot "docfx\docfx.zip"
	$DocFxSource = "https://github.com/dotnet/docfx/releases/download/v1.9.4/docfx.zip"
	Invoke-WebRequest $DocFxSource -OutFile $DocFxZip

	#unzip it	
	& $7Zip e $DocFxZip "-o$DocFxFolder"
}

#clear site
If(Test-Path(Join-Path -Path $RepoRoot "apidocs\_site"))
{
	Remove-Item $DocFxSiteOutput -recurse
}

# run it!
& $DocFx metadata $DocFxJson
& $DocFx build $DocFxJson

# zip it

& $7Zip a -tzip csharp-docs.zip $DocFxSiteOutput -r
