param (
  [parameter(mandatory=$true)]
  [string]
  [alias("r")]
  $release,

  [string]
  [alias("c")]
  $comment,

  [int]
  [alias("b")]
  $build = 0

  #[bool]$p3 = $true,
  #[switch][alias("sw")]$p5 = $false
)

# functions 

function loadSemVer($pwd) {
  $semverlib = [System.IO.Path]::Combine($pwd, "..", "src\packages\Semver.2.0.4\lib\net452\Semver.dll")
  if (-not (test-path $semverlib)) {
    write-error "Missing packages\Semver.2.0.4\lib\net452\Semver.dll."
    exit 1
  }
  [Reflection.Assembly]::LoadFile("d:\d\Umbraco 7.6\src\packages\Semver.2.0.4\lib\net452\Semver.dll")
}

function repl($pwd, $filename, $source, $replacement) {
  $filepath = [System.IO.Path]::Combine($pwd, $filename)
  $text = [System.IO.File]::ReadAllText($filepath)
  $text = [System.Text.RegularExpressions.Regex]::Replace($text, $source, $replacement)
  [System.IO.File]::WriteAllText($filepath, $text)
}

# set version

$release = $release.Trim()
$version = $release
if ($version -eq "") {
  write-error "Invalid release."
  exit 1
}
$comment = $comment.Trim()
if ($comment -ne "") {
  $version = "$version-$comment"
}
if ($build -ne 0) {
  $version = "$version+$build"
}

write "Text $version"

loadSemVer($pwd)
$semver = [SemVer.SemVersion]::Parse($version)

$filever = "" + $semver.Major + "." + $semver.Minor + "." + $semver.Patch
if ($build -ne 0) {
  $filever = "$filever.$build"
}

write "Sem version:  $semver"
write "File version: $filever"

$year = [System.DateTime]::Now.ToString("yyyy")

repl $pwd "..\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
  "(\d+)\.(\d+)\.(\d+)(.(\d+))?" `
  "$filever"
  
repl $pwd "..\src\Umbraco.Core\Configuration\UmbracoVersion.cs" `
  "CurrentComment { get { return `"(.+)`"" `
  "CurrentComment { get { return `"$comment`""
  
repl $pwd "..\src\SolutionInfo.cs" `
  "AssemblyFileVersion\(`"(.+)?`"\)" `
  "AssemblyFileVersion(`"$filever`")"
  
repl $pwd "..\src\SolutionInfo.cs" `
  "AssemblyInformationalVersion\(`"(.+)?`"\)" `
  "AssemblyInformationalVersion(`"$semver`")"
  
repl $pwd "..\src\SolutionInfo.cs" `
  "AssemblyCopyright\(`"Copyright © Umbraco (\d{4})`"\)" `
  "AssemblyCopyright(`"Copyright © Umbraco $year`")"
 
# that one is in build.proj *but* there's no such //x:UmbracoVersion in the file?!
#<XmlPoke XmlInputPath=".\NuSpecs\build\UmbracoCms.props"
#	Namespaces="&lt;Namespace Prefix='x' Uri='http://schemas.microsoft.com/developer/msbuild/2003' /&gt;"
#	Query="//x:UmbracoVersion"
#	Value="$(ReleaseSemVersion)" />
