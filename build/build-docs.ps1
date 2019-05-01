$uenv=build/build.ps1 -get

$src = "$($uenv.SolutionRoot)\src"
$tmp = $uenv.BuildTemp
$out = $uenv.BuildOutput
$DocFxJson = "$src\ApiDocs\docfx.json"
$DocFxSiteOutput = "$tmp\_site\*.*"

################ Do the UI docs
$uenv.CompileBelle()

"Moving to Umbraco.Web.UI.Client folder"
cd .\src\Umbraco.Web.UI.Client

"Generating the docs and waiting before executing the next commands"
& gulp docs | Out-Null

# change baseUrl
$BaseUrl = "https://our.umbraco.com/apidocs/v8/ui/"
$IndexPath = "./docs/api/index.html"
(Get-Content $IndexPath).replace('location.href.replace(rUrl, indexFile)', "`'" + $BaseUrl + "`'") | Set-Content $IndexPath

# zip it
& $uenv.BuildEnv.Zip a -tzip -r "$out\ui-docs.zip" "$src\Umbraco.Web.UI.Client\docs\api\*.*"


################ Do the c# docs

# Build the solution in debug mode
$SolutionPath = Join-Path -Path $src -ChildPath "umbraco.sln"
#$uenv.CompileUmbraco()

#restore nuget packages
$uenv.RestoreNuGet()

# run DocFx
$DocFx = $uenv.BuildEnv.DocFx

Write-Host "$DocFxJson"
& $DocFx metadata $DocFxJson
& $DocFx build $DocFxJson

# zip it
& $uenv.BuildEnv.Zip a -tzip -r "$out\csharp-docs.zip" $DocFxSiteOutput
