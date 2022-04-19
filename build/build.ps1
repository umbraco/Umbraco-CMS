
  param (
    # get, don't execute
    [Parameter(Mandatory=$false)]
    [Alias("g")]
    [switch] $get = $false,

    # run local, don't download, assume everything is ready
    [Parameter(Mandatory=$false)]
    [Alias("l")]
    [Alias("loc")]
    [switch] $local = $false,

	  # enable docfx
    [Parameter(Mandatory=$false)]
    [Alias("doc")]
    [switch] $docfx = $false,

    # keep the build directories, don't clear them
    [Parameter(Mandatory=$false)]
    [Alias("c")]
    [Alias("cont")]
    [switch] $continue = $false,

    # execute a command
    [Parameter(Mandatory=$false, ValueFromRemainingArguments=$true)]
    [String[]]
    $command
  )

  # ################################################################
  # BOOTSTRAP
  # ################################################################

  # create and boot the buildsystem
  $ubuild = &"$PSScriptRoot\build-bootstrap.ps1"
  if (-not $?) { return }
  $ubuild.Boot($PSScriptRoot,
    @{ Local = $local; WithDocFx = $docfx },
    @{ Continue = $continue })
  if ($ubuild.OnError()) { return }

  Write-Host "Umbraco CMS Build"
  Write-Host "Umbraco.Build v$($ubuild.BuildVersion)"

  # ################################################################
  # TASKS
  # ################################################################

  $ubuild.DefineMethod("SetMoreUmbracoVersion",
  {
    param ( $semver )

    $port = "" + $semver.Major + $semver.Minor + ("" + $semver.Patch).PadLeft(2, '0')
    Write-Host "Update port in launchSettings.json to $port"
    $filePath = "$($this.SolutionRoot)\src\Umbraco.Web.UI\Properties\launchSettings.json"
    $this.ReplaceFileText($filePath, `
      "http://localhost:(\d+)?", `
      "http://localhost:$port")
  })

  $ubuild.DefineMethod("SandboxNode",
  {
    $global:node_path = $env:path
    $nodePath = $this.BuildEnv.NodePath
    $gitExe = (Get-Command git).Source
    if (-not $gitExe) { $gitExe = (Get-Command git).Path }
    $gitPath = [System.IO.Path]::GetDirectoryName($gitExe)
    $env:path = "$nodePath;$gitPath"

    $global:node_nodepath = $this.ClearEnvVar("NODEPATH")
    $global:node_npmcache = $this.ClearEnvVar("NPM_CONFIG_CACHE")
    $global:node_npmprefix = $this.ClearEnvVar("NPM_CONFIG_PREFIX")

    # https://github.com/gruntjs/grunt-contrib-connect/issues/235
    $this.SetEnvVar("NODE_NO_HTTP2", "1")
  })

  $ubuild.DefineMethod("RestoreNode",
  {
    $env:path = $node_path

    $this.SetEnvVar("NODEPATH", $node_nodepath)
    $this.SetEnvVar("NPM_CONFIG_CACHE", $node_npmcache)
    $this.SetEnvVar("NPM_CONFIG_PREFIX", $node_npmprefix)

    $this.ClearEnvVar("NODE_NO_HTTP2")
  })

  $ubuild.DefineMethod("CompileBelle",
  {
    $src = "$($this.SolutionRoot)\src"
    $log = "$($this.BuildTemp)\belle.log"


    Write-Host "Compile Belle"
    Write-Host "Logging to $log"

    # get a temp clean node env (will restore)
    $this.SandboxNode()

    # stupid PS is going to gather all "warnings" in $error
    # so we have to take care of it else they'll bubble and kill the build
    if ($error.Count -gt 0) { return }

    try {
        Push-Location "$($this.SolutionRoot)\src\Umbraco.Web.UI.Client"
        Write-Output "" > $log

        Write-Output "### node version is:" > $log
        node -v >> $log 2>&1
        if (-not $?) { throw "Failed to report node version." }

        Write-Output "### npm version is:" >> $log 2>&1
        npm -v >> $log 2>&1
        if (-not $?) { throw "Failed to report npm version." }

        Write-Output "### clean npm cache" >> $log 2>&1
        npm cache clean --force >> $log 2>&1
        $error.Clear() # that one can fail 'cos security bug - ignore

        Write-Output "### npm ci" >> $log 2>&1
        npm ci >> $log 2>&1
        Write-Output ">> $? $($error.Count)" >> $log 2>&1
        # Don't really care about the messages from npm ci making us think there are errors
        $error.Clear()

        Write-Output "### gulp build for version $($this.Version.Release)" >> $log 2>&1
        npm run build --buildversion=$this.Version.Release >> $log 2>&1

		# We can ignore this warning, we need to update to node 12 at some point - https://github.com/jsdom/jsdom/issues/2939
		$indexes = [System.Collections.ArrayList]::new()
		$index = 0;
		$error | ForEach-Object {
			# Find which of the errors is the ExperimentalWarning
			if($_.ToString().Contains("ExperimentalWarning: The fs.promises API is experimental")) {
				[void]$indexes.Add($index)
			}
			$index++
		}
		$indexes | ForEach-Object {
			# Loop through the list of indexes and remove the errors that we expect and feel confident we can ignore
			$error.Remove($error[$_])
		}

        if (-not $?) { throw "Failed to build" } # that one is expected to work
    } finally {
        Pop-Location

        # FIXME: should we filter the log to find errors?
        #get-content .\build.tmp\belle.log | %{ if ($_ -match "build") { write $_}}

        # restore
        $this.RestoreNode()
    }

    # setting node_modules folder to hidden
    # used to prevent VS13 from crashing on it while loading the websites project
    # also makes sure aspnet compiler does not try to handle rogue files and chokes
    # in VSO with Microsoft.VisualC.CppCodeProvider -related errors
    # use get-item -force 'cos it might be hidden already
    Write-Host "Set hidden attribute on node_modules"
    $dir = Get-Item -force "$src\Umbraco.Web.UI.Client\node_modules"
    $dir.Attributes = $dir.Attributes -bor ([System.IO.FileAttributes]::Hidden)
  })

  $ubuild.DefineMethod("CompileUmbraco",
  {
    $buildConfiguration = "Release"

    $src = "$($this.SolutionRoot)\src"
    $log = "$($this.BuildTemp)\build.umbraco.log"

    Write-Host "Compile Umbraco"
    Write-Host "Logging to $log"

   & dotnet build "$src\Umbraco.Web.UI\Umbraco.Web.UI.csproj" `
      --configuration $buildConfiguration `
      --output "$($this.BuildTemp)\bin\\" `
      > $log

   # get files into WebApp\bin
    & dotnet publish "$src\Umbraco.Web.UI\Umbraco.Web.UI.csproj" `
      --configuration Release --output "$($this.BuildTemp)\WebApp\bin\\" `
      > $log

    & dotnet publish "$src\Umbraco.Persistence.SqlCe\Umbraco.Persistence.SqlCe.csproj" `
      --configuration Release --output "$($this.BuildTemp)\SqlCe\" `
      > $log

    # remove extra files
    $webAppBin = "$($this.BuildTemp)\WebApp\bin"
    $excludeDirs = @("$($webAppBin)\refs","$($webAppBin)\runtimes","$($webAppBin)\umbraco","$($webAppBin)\wwwroot")
    $excludeFiles = @("$($webAppBin)\appsettings.*","$($webAppBin)\*.deps.json","$($webAppBin)\*.exe","$($webAppBin)\*.config","$($webAppBin)\*.runtimeconfig.json")
    $this.RemoveDirectory($excludeDirs)
    $this.RemoveFile($excludeFiles)

    # copy rest of the files into WebApp
    $this.CopyFiles("$($this.SolutionRoot)\src\Umbraco.Web.UI\umbraco", "*", "$($this.BuildTemp)\WebApp\umbraco")
    $excludeUmbracoDirs = @("$($this.BuildTemp)\WebApp\umbraco\lib","$($this.BuildTemp)\WebApp\umbraco\Data","$($this.BuildTemp)\WebApp\umbraco\Logs")
    $this.RemoveDirectory($excludeUmbracoDirs)
    $this.CopyFiles("$($this.SolutionRoot)\src\Umbraco.Web.UI\Views", "*", "$($this.BuildTemp)\WebApp\Views")
    Copy-Item "$($this.SolutionRoot)\src\Umbraco.Web.UI\appsettings.json" "$($this.BuildTemp)\WebApp"

    if (-not $?) { throw "Failed to compile Umbraco.Web.UI." }

    # /p:UmbracoBuild tells the csproj that we are building from PS, not VS
  })

  $ubuild.DefineMethod("CompileJsonSchema",
  {
    Write-Host "Generating JSON Schema for AppSettings"
    Write-Host "Logging to $($this.BuildTemp)\json.schema.log"

    ## NOTE: Need to specify the outputfile to point to the build temp folder
    &dotnet run --project "$($this.SolutionRoot)\src\JsonSchema\JsonSchema.csproj" `
        -c Release > "$($this.BuildTemp)\json.schema.log" `
        -- `
        --outputFile "$($this.BuildTemp)\WebApp\umbraco\config\appsettings-schema.json"
  })

  $ubuild.DefineMethod("PrepareTests",
  {
    Write-Host "Prepare Tests"

    # FIXME: - idea is to avoid rebuilding everything for tests
    # but because of our weird assembly versioning (with .* stuff)
    # everything gets rebuilt all the time...
    #Copy-Files "$tmp\bin" "." "$tmp\tests"

    # data
    Write-Host "Copy data files"
    if (-not (Test-Path -Path "$($this.BuildTemp)\tests\Packaging" ))
    {
      Write-Host "Create packaging directory"
      mkdir "$($this.BuildTemp)\tests\Packaging" > $null
    }
    #$this.CopyFiles("$($this.SolutionRoot)\src\Umbraco.Tests\Packaging\Packages", "*", "$($this.BuildTemp)\tests\Packaging\Packages")

    # required for package install tests
    if (-not (Test-Path -Path "$($this.BuildTemp)\tests\bin" ))
    {
      Write-Host "Create bin directory"
      mkdir "$($this.BuildTemp)\tests\bin" > $null
    }
  })

  $ubuild.DefineMethod("CompileTests",
  {
    $buildConfiguration = "Release"
    $log = "$($this.BuildTemp)\msbuild.tests.log"

    Write-Host "Compile Tests"
    Write-Host "Logging to $log"

    # beware of the weird double \\ at the end of paths
    # see http://edgylogic.com/blog/powershell-and-external-commands-done-right/
    &dotnet msbuild "$($this.SolutionRoot)\tests\Umbraco.Tests\Umbraco.Tests.csproj" `
      -target:Build `
      -property:WarningLevel=0 `
      -property:Configuration=$buildConfiguration `
      -property:Platform=AnyCPU `
      -property:UseWPP_CopyWebApplication=True `
      -property:PipelineDependsOnBuild=False `
      -property:OutDir="$($this.BuildTemp)\tests\\" `
      -property:Verbosity=minimal `
      -property:UmbracoBuild=True `
      > $log

      # copy Umbraco.Persistence.SqlCe files into WebApp
      Copy-Item "$($this.BuildTemp)\tests\Umbraco.Persistence.SqlCe.*" "$($this.BuildTemp)\WebApp\bin"

    if (-not $?) { throw "Failed to compile tests." }

    # /p:UmbracoBuild tells the csproj that we are building from PS
  })

  $ubuild.DefineMethod("PreparePackages",
  {
    Write-Host "Prepare Packages"

    $src = "$($this.SolutionRoot)\src"
    $tmp = "$($this.BuildTemp)"

    # cleanup build
    Write-Host "Clean build"
    $this.RemoveFile("$tmp\bin\*.dll.config")
    $this.RemoveFile("$tmp\WebApp\bin\*.dll.config")

    # cleanup presentation
    Write-Host "Cleanup presentation"
    $this.RemoveDirectory("$tmp\WebApp\umbraco.presentation")

    # create directories
    Write-Host "Create directories"
    mkdir "$tmp\WebApp\App_Data" > $null
    #mkdir "$tmp\WebApp\Media" > $null
    #mkdir "$tmp\WebApp\Views" > $null

    # copy various files
    Write-Host "Copy xml documentation"
    Copy-Item -force "$tmp\bin\*.xml" "$tmp\WebApp\bin"

    # offset the modified timestamps on all umbraco dlls, as WebResources
    # break if date is in the future, which, due to timezone offsets can happen.
    Write-Host "Offset dlls timestamps"
    Get-ChildItem -r "$tmp\*.dll" | ForEach-Object {
      $_.CreationTime = $_.CreationTime.AddHours(-11)
      $_.LastWriteTime = $_.LastWriteTime.AddHours(-11)
    }

    # copy libs
    Write-Host "Copy SqlCE libraries"
    $nugetPackages = $env:NUGET_PACKAGES
    if (-not $nugetPackages)
    {
      $nugetPackages = [System.Environment]::ExpandEnvironmentVariables("%userprofile%\.nuget\packages")
    }

    # copy Belle
    Write-Host "Copy Belle"
    $this.CopyFiles("$src\Umbraco.Web.UI\wwwroot\umbraco\assets", "*", "$tmp\WebApp\wwwroot\umbraco\assets")
    $this.CopyFiles("$src\Umbraco.Web.UI\wwwroot\umbraco\js", "*", "$tmp\WebApp\wwwroot\umbraco\js")
    $this.CopyFiles("$src\Umbraco.Web.UI\wwwroot\umbraco\lib", "*", "$tmp\WebApp\wwwroot\umbraco\lib")
    $this.CopyFiles("$src\Umbraco.Web.UI\wwwroot\umbraco\views", "*", "$tmp\WebApp\wwwroot\umbraco\views")
  })


  $ubuild.DefineMethod("PrepareBuild",
  {
    Write-host "Set environment"
    $env:UMBRACO_VERSION=$this.Version.Semver.ToString()
    $env:UMBRACO_RELEASE=$this.Version.Release
    $env:UMBRACO_COMMENT=$this.Version.Comment
    $env:UMBRACO_BUILD=$this.Version.Build
    $env:UMBRACO_TMP="$($this.SolutionRoot)\build.tmp"

    if ($args -and $args[0] -eq "vso")
    {
      Write-host "Set VSO environment"
      # set environment variable for VSO
      # https://github.com/Microsoft/vsts-tasks/issues/375
      # https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
      Write-Host ("##vso[task.setvariable variable=UMBRACO_VERSION;]$($this.Version.Semver.ToString())")
      Write-Host ("##vso[task.setvariable variable=UMBRACO_RELEASE;]$($this.Version.Release)")
      Write-Host ("##vso[task.setvariable variable=UMBRACO_COMMENT;]$($this.Version.Comment)")
      Write-Host ("##vso[task.setvariable variable=UMBRACO_BUILD;]$($this.Version.Build)")

      Write-Host ("##vso[task.setvariable variable=UMBRACO_TMP;]$($this.SolutionRoot)\build.tmp")
    }
  })

  $nugetsourceUmbraco = "https://api.nuget.org/v3/index.json"

  $ubuild.DefineMethod("RestoreNuGet",
  {
    Write-Host "Restore NuGet"
    Write-Host "Logging to $($this.BuildTemp)\nuget.restore.log"
  	$params = "-Source", $nugetsourceUmbraco
    &$this.BuildEnv.NuGet restore "$($this.SolutionRoot)\umbraco-netcore-only.sln" > "$($this.BuildTemp)\nuget.restore.log" @params
    if (-not $?) { throw "Failed to restore NuGet packages." }
  })

  $ubuild.DefineMethod("PackageNuGet",
  {
    $nuspecs = "$($this.SolutionRoot)\build\NuSpecs"
    $templates = "$($this.SolutionRoot)\templates"

    Write-Host "Create NuGet packages"

    &dotnet pack "$($this.SolutionRoot)\umbraco-netcore-only.sln" `
        --output "$($this.BuildOutput)" `
        --verbosity detailed `
        -c Release `
        -p:PackageVersion="$($this.Version.Semver.ToString())" > "$($this.BuildTemp)\pack.umbraco.log"

    &$this.BuildEnv.NuGet Pack "$nuspecs\UmbracoCms.nuspec" `
        -Properties BuildTmp="$($this.BuildTemp)" `
        -Version "$($this.Version.Semver.ToString())" `
        -Verbosity detailed -outputDirectory "$($this.BuildOutput)" > "$($this.BuildTemp)\nupack.cms.log"
    if (-not $?) { throw "Failed to pack NuGet UmbracoCms." }

    &$this.BuildEnv.NuGet Pack "$nuspecs\UmbracoCms.SqlCe.nuspec" `
        -Properties BuildTmp="$($this.BuildTemp)" `
        -Version "$($this.Version.Semver.ToString())" `
        -Verbosity detailed -outputDirectory "$($this.BuildOutput)" > "$($this.BuildTemp)\nupack.cmssqlce.log"
    if (-not $?) { throw "Failed to pack NuGet UmbracoCms.SqlCe." }

    &$this.BuildEnv.NuGet Pack "$nuspecs\UmbracoCms.StaticAssets.nuspec" `
        -Properties BuildTmp="$($this.BuildTemp)" `
        -Version "$($this.Version.Semver.ToString())" `
        -Verbosity detailed -outputDirectory "$($this.BuildOutput)" > "$($this.BuildTemp)\nupack.cmsstaticassets.log"
    if (-not $?) { throw "Failed to pack NuGet UmbracoCms.StaticAssets." }

    &$this.BuildEnv.NuGet Pack "$templates\Umbraco.Templates.nuspec" `
        -Properties BuildTmp="$($this.BuildTemp)" `
        -Version "$($this.Version.Semver.ToString())" `
        -NoDefaultExcludes `
        -Verbosity detailed -outputDirectory "$($this.BuildOutput)" > "$($this.BuildTemp)\nupack.templates.log"
    if (-not $?) { throw "Failed to pack NuGet Umbraco.Templates." }

    # run hook
    if ($this.HasMethod("PostPackageNuGet"))
    {
      Write-Host "Run PostPackageNuGet hook"
      $this.PostPackageNuGet();
      if (-not $?) { throw "Failed to run hook." }
    }
  })

  $ubuild.DefineMethod("VerifyNuGet",
  {
    $this.VerifyNuGetConsistency(
      ("UmbracoCms"),
      ("Umbraco.Core", "Umbraco.Infrastructure", "Umbraco.Web.UI", "Umbraco.Examine.Lucene", "Umbraco.PublishedCache.NuCache", "Umbraco.Web.Common", "Umbraco.Web.Website", "Umbraco.Web.BackOffice", "Umbraco.Persistence.SqlCe"))
    if ($this.OnError()) { return }
  })

  $ubuild.DefineMethod("PrepareCSharpDocs",
  {
    Write-Host "Prepare C# Documentation"

    $src = "$($this.SolutionRoot)\src"
    $tmp = $this.BuildTemp
    $out = $this.BuildOutput
    $DocFxJson = Join-Path -Path $src "\ApiDocs\docfx.json"
    $DocFxSiteOutput = Join-Path -Path $tmp "\_site\*.*"

    # run DocFx
    $DocFx = $this.BuildEnv.DocFx

    & $DocFx metadata $DocFxJson
    & $DocFx build $DocFxJson

    # zip it
    & $this.BuildEnv.Zip a -tzip -r "$out\csharp-docs.zip" $DocFxSiteOutput
  })

  $ubuild.DefineMethod("PrepareAngularDocs",
  {
    Write-Host "Prepare Angular Documentation"

    $src = "$($this.SolutionRoot)\src"
    $out = $this.BuildOutput

    # Check if the solution has been built
    if (!(Test-Path "$src\Umbraco.Web.UI.Client\node_modules")) {throw "Umbraco needs to be built before generating the Angular Docs"}

    "Moving to Umbraco.Web.UI.Docs folder"
    cd $src\Umbraco.Web.UI.Docs

    "Generating the docs and waiting before executing the next commands"
	  & npm ci
    & npx gulp docs

    Pop-Location

    # change baseUrl
    $BaseUrl = "https://apidocs.umbraco.com/v9/ui/"
    $IndexPath = "./api/index.html"
    (Get-Content $IndexPath).replace('origin + location.href.substr(origin.length).replace(rUrl, indexFile)', "`'" + $BaseUrl + "`'") | Set-Content $IndexPath

    # zip it
    & $this.BuildEnv.Zip a -tzip -r "$out\ui-docs.zip" "$src\Umbraco.Web.UI.Docs\api\*.*"
  })

  $ubuild.DefineMethod("Build",
  {
    $error.Clear()

    $this.PrepareBuild()
    if ($this.OnError()) { return }
    $this.RestoreNuGet()
    if ($this.OnError()) { return }
    $this.CompileBelle()
    if ($this.OnError()) { return }
    $this.CompileUmbraco()
    if ($this.OnError()) { return }
    $this.CompileJsonSchema()
    if ($this.OnError()) { return }
    $this.PrepareTests()
    if ($this.OnError()) { return }
    $this.CompileTests()
    if ($this.OnError()) { return }
    # not running tests
    $this.PreparePackages()
    if ($this.OnError()) { return }
    $this.VerifyNuGet()
    if ($this.OnError()) { return }
    $this.PackageNuGet()
    if ($this.OnError()) { return }
    $this.PostPackageHook()
    if ($this.OnError()) { return }

    Write-Host "Done"
  })

  $ubuild.DefineMethod("PostPackageHook",
  {
    # run hook
    if ($this.HasMethod("PostPackage"))
    {
      Write-Host "Run PostPackage hook"
      $this.PostPackage();
      if (-not $?) { throw "Failed to run hook." }
    }
  })

  # ################################################################
  # RUN
  # ################################################################

  # configure
  $ubuild.ReleaseBranches = @( "master" )

  # run
  if (-not $get)
  {
    if ($command.Length -eq 0)
    {
      $command = @( "Build" )
    }
    $ubuild.RunMethod($command);
    if ($ubuild.OnError()) { return }
  }
  if ($get) { return $ubuild }
