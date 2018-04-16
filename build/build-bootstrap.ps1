
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
      Invoke-WebRequest $source -OutFile $nuget
      if (-not $?) { throw "Failed to download NuGet." }
    }
  }
  elseif (-not (test-path $nuget))
  {
    throw "Failed to locate NuGet.exe."
  }

  # get the build system
  if (-not $local)
  {
    $solutionRoot = "$scriptRoot\.."
    $nugetConfig = @{$true="$solutionRoot\src\NuGet.config.user";$false="$solutionRoot\src\NuGet.config"}[(test-path "$solutionRoot\src\NuGet.config.user")]
    &$nuget install Umbraco.Build -OutputDirectory $scriptTemp -Verbosity quiet -PreRelease -ConfigFile $nugetConfig
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