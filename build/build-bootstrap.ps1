
  # this script should be dot-sourced into the build.ps1 scripts
  # right after the parameters declaration
  # ie
  # . "$PSScriptRoot\build-bootstrap.ps1"

  # THIS FILE IS DISTRIBUTED AS PART OF UMBRACO.BUILD
  # DO NOT MODIFY IT - ALWAYS USED THE COMMON VERSION

  # ################################################################
  # BOOTSTRAP
  # ################################################################

  # reset errors
  $error.Clear()

  # ensure we have temp folder for downloads
  $scriptRoot = "$PSScriptRoot"
  $scriptTemp = "$scriptRoot\temp"
  if (-not (test-path $scriptTemp)) { mkdir $scriptTemp > $null }

  # get NuGet
  $cache = 4
  $nuget = "$scriptTemp\nuget.exe"
  # ensure the correct NuGet-source is used. This one is used by Umbraco
  $nugetsourceUmbraco = "https://www.myget.org/F/umbracocore/api/v3/index.json"
  if (-not $local)
  {
    $source = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    if ((test-path $nuget) -and ((ls $nuget).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
    {
      Remove-Item $nuget -force -errorAction SilentlyContinue > $null
    }
    if (-not (test-path $nuget))
    {
      Write-Host "Download NuGet..."
      [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
      Invoke-WebRequest $source -OutFile $nuget
      if (-not $?) { throw "Failed to download NuGet." }
    }
  }
  elseif (-not (test-path $nuget))
  {
    throw "Failed to locate NuGet.exe."
  }

  # NuGet notes
  # As soon as we use -ConfigFile, NuGet uses that file, and only that file, and does not
  # merge configuration from system level. See comments in NuGet.Client solution, class
  # NuGet.Configuration.Settings, method LoadDefaultSettings.
  # For NuGet to merge configurations, it needs to "find" the file in the current directory,
  # or above. Which means we cannot really use -ConfigFile but instead have to have Umbraco's
  # NuGet.config file at root, and always run NuGet.exe while at root or in a directory below
  # root.

  $solutionRoot = "$scriptRoot\.."
  $testPwd = [System.IO.Path]::GetFullPath($pwd.Path) + "\"
  $testRoot = [System.IO.Path]::GetFullPath($solutionRoot) + "\"
  if (-not $testPwd.ToLower().StartsWith($testRoot.ToLower()))
  {
      throw "Cannot run outside of the solution's root."
  }

  # get the build system
  if (-not $local)
  {
    $params = "-OutputDirectory", $scriptTemp, "-Verbosity", "quiet", "-PreRelease", "-Source", $nugetsourceUmbraco
    &$nuget install Umbraco.Build @params
    if (-not $?) { throw "Failed to download Umbraco.Build." }
  }

  # ensure we have the build system
  $ubuildPath = ls "$scriptTemp\Umbraco.Build.*" | sort -property CreationTime -descending | select -first 1
  if (-not $ubuildPath)
  {
    throw "Failed to locate the build system."
  }

  # boot the build system
  # this creates $global:ubuild
  return &"$ubuildPath\ps\Boot.ps1"

  # at that point the build.ps1 script must boot the build system
  # eg
  # $ubuild.Boot($ubuildPath.FullName, [System.IO.Path]::GetFullPath("$scriptRoot\.."),
  #   @{ Local = $local; With7Zip = $false; WithNode = $false },
  #   @{ continue = $continue })
  # if (-not $?) { throw "Failed to boot the build system." }
  #
  # and it's good practice to report
  # eg
  # Write-Host "Umbraco.Whatever Build"
  # Write-Host "Umbraco.Build v$($ubuild.BuildVersion)"

  # eof
