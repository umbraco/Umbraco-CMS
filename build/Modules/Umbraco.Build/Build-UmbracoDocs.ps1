#

function Build-UmbracoDocs
{
  $uenv = Get-UmbracoBuildEnv
  
  $src = "$($uenv.SolutionRoot)\src"
  $out = "$($uenv.SolutionRoot)\build.out"
  $tmp = "$($uenv.SolutionRoot)\build.tmp"

  $buildTemp = "$PSScriptRoot\temp"
  $cache = 2

  Prepare-Build -keep $uenv

  ################ Do the UI docs

  # create Belle build folder, so that we don't cause a Belle rebuild
  $belleBuildDir = "$src\Umbraco.Web.UI.Client\build"
  if (-not (Test-Path $belleBuildDir)) 
  {
      mkdir $belleBuildDir > $null
  }

  Write-Host "Build UI documentation"

    # get a temp clean path (will be restored)
  $p = $env:path
  $nodePath = $uenv.NodePath
  $gitExe = (get-command git).Source  
  $gitPath = [System.IO.Path]::GetDirectoryName($gitExe)
  $env:path = "$nodePath;$gitPath"
 
  push-location "$src\Umbraco.Web.UI.Client"
  write "" > $tmp\belle-docs.log
  &npm cache clean --quiet >> $tmp\belle-docs.log 2>&1
  &npm install --quiet >> $tmp\belle-docs.log 2>&1
  &npm install -g grunt-cli --quiet >> $tmp\belle-docs.log 2>&1
  #&npm install -g bower --quiet >> $tmp\belle.log 2>&1
  #&grunt build --buildversion=$version.Release >> $tmp\belle.log 2>&1
  &grunt --gruntfile "$src/Umbraco.Web.UI.Client/gruntfile.js" docs >> $tmp\belle-docs.log 2>&1
  pop-location
  
  # fixme - should we filter the log to find errors?
  #get-content .\build.tmp\belle-docs.log | %{ if ($_ -match "build") { write $_}}

  # change baseUrl
  $baseUrl = "https://our.umbraco.org/apidocs/ui/"
  $indexPath = "$src/Umbraco.Web.UI.Client/docs/api/index.html"
  (Get-Content $indexPath).Replace("location.href.replace(rUrl, indexFile)", "'$baseUrl'") `
    | Set-Content $indexPath
    
  $env:path = $p
  
  # zip
  &$uenv.Zip a -tzip -r "$out\ui-docs.zip" "$src\Umbraco.Web.UI.Client\docs\api\*.*" `
    > $null

  ################ Do the c# docs
  
  Write-Host "Build C# documentation"
  
  # Build the solution in debug mode
  # FIXME no only a simple compilation should be enough!
  # FIXME we MUST handle msbuild & co error codes!
  # FIXME deal with weird things in gitconfig?
  #Build-Umbraco -Configuration Debug
  Restore-NuGet $uenv
  Compile-Umbraco $uenv "Debug" # FIXME different log file!
  Restore-WebConfig "$src\Umbraco.Web.UI"
  
  # ensure we have docfx
  Get-DocFx $uenv $buildTemp

  # clear
  $docFxOutput = "$($uenv.SolutionRoot)\apidocs\_site"
  if (test-path($docFxOutput))
  {
    Remove-Directory $docFxOutput
  }

  # run
  $docFxJson = "$($uenv.SolutionRoot)\apidocs\docfx.json"
  push-location "$($uenv.SolutionRoot)\build" # silly docfx.json wants this
  
  Write-Host "Run DocFx metadata"
  Write-Host "Logging to $tmp\docfx.metadata.log"
  &$uenv.DocFx metadata $docFxJson > "$tmp\docfx.metadata.log"
  Write-Host "Run DocFx build"
  Write-Host "Logging to $tmp\docfx.build.log"
  &$uenv.DocFx build $docFxJson > "$tmp\docfx.build.log"
  
  pop-location

  # zip
  &$uenv.Zip a -tzip -r "$out\csharp-docs.zip" "$docFxOutput\*.*" `
    > $null
}

function Get-DocFx($uenv, $buildTemp)
{
  $docFx = "$buildTemp\docfx"
  if (-not (test-path $docFx))
  {
    Write-Host "Download DocFx..."
    $source = "https://github.com/dotnet/docfx/releases/download/v2.19.2/docfx.zip"
    Invoke-WebRequest $source -OutFile "$buildTemp\docfx.zip"
    
    &$uenv.Zip x "$buildTemp\docfx.zip" -o"$buildTemp\docfx" -aos > $nul
    Remove-File "$buildTemp\docfx.zip"  
  }
  $uenv | add-member -memberType NoteProperty -name DocFx -value "$docFx\docfx.exe"
}