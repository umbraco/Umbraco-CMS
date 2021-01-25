
# create $ubuild
$ubuild = @{ }

# adds a method
Add-Member -InputObject $ubuild -MemberType ScriptMethod "DefineMethod" -value `
{
  param ( [string]$name, [scriptblock]$block )
  Add-Member -InputObject $this -MemberType ScriptMethod $name -Value $block
}

# looks for a method
$ubuild.DefineMethod("HasMethod",
{
  param ( $name )
  return $this.PSObject.Methods.Name -eq $name
})

# runs a method
$ubuild.DefineMethod("RunMethod",
{
  param ( [String[]] $command)
  $name, $arguments = $command
  if ($arguments -eq $null)
  {
    $arguments = @()
  }
  $method = ($this.PSObject.Methods | Where { $_.Name -eq $name } | Select -First 1)
  if ($method -eq $null)
  {
    throw "Unknown method: $name."
  }
  else
  {
    $method.Invoke($arguments)
  }
})

# shows error
$ubuild.DefineMethod("OnError",
{
  if ($error)
  {
    write-host -ForegroundColor red "Error!"
    write-host " "
    for ($i = 0; $i -lt $error.Count; $i++)
    {
      $e = $error[$i]
      write-host "$($e.Exception.GetType().Name): $($e.Exception.Message)"
      write-host "At $($e.InvocationInfo.ScriptName):$($e.InvocationInfo.ScriptLineNumber):$($e.InvocationInfo.OffsetInLine)"
      write-host "+ $($e.InvocationInfo.Line.Trim())"
      write-host " "
    }
    write-host -ForegroundColor red "Abort"
    return $true
  }
  return $false
})

# register boot method
$ubuild.DefineMethod("Boot",
{
  param (
    [Parameter(Mandatory=$true)]
    [string] $buildRoot,

    [Parameter(Mandatory=$false)]
    $uenvOptions,

    # .IsUmbracoBuild
    #     indicates whether we are building Umbraco.Build
    #     as .BuildPath and .BuildVersion are obviously different
    # .Continue
    #     do not clear the tmp and out directories
    [Parameter(Mandatory=$false)]
    $switches
  )

  if ($switches)
  {
    $isUmbracoBuild = $switches.IsUmbracoBuild
    $continue = $switches.Continue
  }

  $this.BuildPath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..")
  $this.SolutionRoot = [System.IO.Path]::GetFullPath("$buildRoot\..")

  if ($isUmbracoBuild)
  {
    $this.BuildVersion = "? (building)"
  }
  else
  {
    $this.BuildVersion = [System.IO.Path]::GetFileName($this.BuildPath).Substring("Umbraco.Build.".Length)
    Write-Host "Umbraco.Build v$($this.BuildVersion)"

    # load the lib
    Add-Type -Path "$($this.BuildPath)\lib\Umbraco.Build.dll"
    if (-not $?) { throw "Failed to load Umbraco.Build.dll." }
  }

  $scripts = (
    "Utilities",
    "GetUmbracoBuildEnv",
    "GetUmbracoVersion",
    "SetUmbracoVersion",
    "SetClearBuildVersion",
    "VerifyNuGet"
  )

  # source the scripts
  foreach ($script in $scripts) {
    &"$($this.BuildPath)\ps\$script.ps1"
    if (-not $?) { throw "Failed to source $script.ps1" }
  }

  # ensure we have empty build.tmp and build.out folders
  $buildTemp = "$($this.SolutionRoot)\build.tmp"
  $buildOutput = "$($this.SolutionRoot)\build.out"
  if ($continue)
  {
    if (-not (test-path $buildTemp)) { mkdir $buildTemp > $null }
    if (-not (test-path $buildOutput)) { mkdir $buildOutput > $null }
  }
  else
  {
    if (test-path $buildTemp) { remove-item $buildTemp -force -recurse -errorAction SilentlyContinue > $null }
    if (test-path $buildOutput) { remove-item $buildOutput -force -recurse -errorAction SilentlyContinue > $null }
    mkdir $buildTemp > $null
    mkdir $buildOutput > $null
  }

  $this.BuildTemp = $buildTemp
  $this.BuildOutput = $buildOutput
  $this.BuildNumber = $env:BUILD_BUILDNUMBER

  # ensure we have temp folder for downloads
  $scriptTemp = "$($this.SolutionRoot)\build\temp"
  if (-not (test-path $scriptTemp)) { mkdir $scriptTemp > $null }

  # initialize the build environment
  $this.BuildEnv = $this.GetUmbracoBuildEnv($uenvOptions, $scriptTemp)
  if ($this.OnError()) { return }

  # initialize the version
  $this.Version = $this.GetUmbracoVersion()
  if ($this.OnError()) { return }

  # source the hooks
  $hooks = "$($this.SolutionRoot)\build\hooks"
  if ([System.IO.Directory]::Exists($hooks))
  {
    ls "$hooks\*.ps1" | ForEach-Object {
      #Write-Host "Hook script $_"
      &"$_"
    }
  }
})

return $ubuild