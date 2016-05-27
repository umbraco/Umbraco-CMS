$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path);
$RepoRoot = (get-item $PSScriptFilePath).Directory.Parent.FullName;
$SolutionRoot = Join-Path -Path $RepoRoot "src";
$ToolsRoot = Join-Path -Path $RepoRoot "tools";
$DocFx = Join-Path -Path $ToolsRoot "docfx\docfx.exe"
$DocFxFolder = (Join-Path -Path $ToolsRoot "docfx")
$DocFxJson = Join-Path -Path $RepoRoot "apidocs\docfx.json"
$7Zip = Join-Path -Path $ToolsRoot "7zip\7za.exe"
$DocSiteOutput = Join-Path -Path $RepoRoot "apidocs\_site\*.*"

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
	Remove-Item $DocSiteOutput -recurse
}

# run it!
& $DocFx metadata $DocFxJson
& $DocFx build $DocFxJson

# zip it

& $7Zip a -tzip docs.zip $DocSiteOutput -r