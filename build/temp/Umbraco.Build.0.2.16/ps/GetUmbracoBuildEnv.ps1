
$ubuild.DefineMethod("GetUmbracoBuildEnv",
{
  param (
    [Parameter(Mandatory=$true)]
    $uenvOptions,

    [Parameter(Mandatory=$true)]
    [string] $scriptTemp
  )

  function Merge-Options
  {
    param ( $merge, $options )
    $keys = $options.GetEnumerator() | ForEach-Object { $_.Key }
    foreach ($key in $keys)
    {
      if ($merge.ContainsKey($key)) { $options[$key] = $merge[$key] }
    }
    return $options
  }

  # options
  $options = Merge-Options $uenvOptions @{
    Local = $false
    Cache = 4 # days
    With7Zip = $true
    WithVs = $true
    WithSemver = $true
    WithNode = $true
    NodeVersion = '10.15.0'
    WithDocFx = $false
    VsPreview = $true
    VsMajor = $null
  }

  # ensure we have NuGet - not an option really
  $nuget = "$scriptTemp\nuget.exe"
  # ensure the correct NuGet-source is used (not the Umbraco one, but the NuGet-one)
  $nugetsource = "https://api.nuget.org/v3/index.json"
  if (-not $options.Local)
  {
    $source = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    if ((test-path $nuget) -and ((ls $nuget).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
    {
      $this.RemoveFile($nuget)
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

  # see NuGet notes in build-bootstrap.ps1
  $testPwd = [System.IO.Path]::GetFullPath($pwd.Path) + "\"
  $testRoot = [System.IO.Path]::GetFullPath($this.SolutionRoot) + "\"
  if (-not $testPwd.ToLower().StartsWith($testRoot.ToLower()))
  {
      throw "Cannot run outside of the solution's root."
  }

  # ensure we have 7-Zip
  $sevenZip = "$scriptTemp\7za.exe"
  if ($options.With7Zip)
  {
    if (-not $options.Local)
    {
      if ((test-path $sevenZip) -and ((ls $sevenZip).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
      {
        $this.RemoveFile($sevenZip)
      }
      if (-not (test-path $sevenZip))
      {
        Write-Host "Download 7-Zip..."
        $params = "-OutputDirectory", $scriptTemp, "-Verbosity", "quiet", "-Source", $nugetsource
        &$nuget install 7-Zip.CommandLine @params
        if (-not $?) { throw "Failed to download 7-Zip." }
        $dir = ls "$scriptTemp\7-Zip.CommandLine.*" | sort -property Name -descending | select -first 1
        # selecting the first 1 because now there is 7za.exe and x64/7za.exe
        # we could be more clever and detect whether we are x86 or x64
        $file = ls -path "$dir" -name 7za.exe -recurse | select -first 1
        mv "$dir\$file" $sevenZip
        $this.RemoveDirectory($dir)
      }
    }
    elseif (-not (test-path $sevenZip))
    {
      throw "Failed to locate 7za.exe."
    }
  }

  # ensure we have vswhere
  $vswhere = "$scriptTemp\vswhere.exe"
  if ($options.WithVs)
  {
    if (-not $options.Local)
    {
      if ((test-path $vswhere) -and ((ls $vswhere).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
      {
        $this.RemoveFile($vswhere)
      }
      if (-not (test-path $vswhere))
      {
        Write-Host "Download VsWhere..."
        $params = "-OutputDirectory", $scriptTemp, "-Verbosity", "quiet", "-Source", $nugetsource
        &$nuget install vswhere @params
        if (-not $?) { throw "Failed to download VsWhere." }
        $dir = ls "$scriptTemp\vswhere.*" | sort -property Name -descending | select -first 1
        $file = ls -path "$dir" -name vswhere.exe -recurse
        mv "$dir\$file" $vswhere
        $this.RemoveDirectory($dir)
      }
    }
    elseif (-not (test-path $vswhere))
    {
      throw "Failed to locate VsWhere.exe."
    }
  }

  # ensure we have semver
  $semver = "$scriptTemp\Semver.dll"
  if ($options.WithSemver)
  {
    if (-not $options.Local)
    {
      if ((test-path $semver) -and ((ls $semver).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
      {
        $this.RemoveFile($semver)
      }
      if (-not (test-path $semver))
      {
        Write-Host "Download Semver..."
        $params = "-OutputDirectory", $scriptTemp, "-Verbosity", "quiet", "-Source", $nugetsource
        &$nuget install semver @params
        $dir = ls "$scriptTemp\semver.*" | sort -property Name -descending | select -first 1
        $file = "$dir\lib\net452\Semver.dll"
        if (-not (test-path $file))
        {
          throw "Failed to locate $file"
        }
        mv "$file" $semver
        $this.RemoveDirectory($dir)
      }
    }
    elseif (-not (test-path $semver))
    {
      throw "Failed to locate $semver"
    }

    try
    {
      Add-Type -Path $semver > $null
    }
    catch
    {
      throw "Failed to load $semver"
    }
  }

  # ensure we have node
  $nodeVersion = $options.NodeVersion
  $node = "$scriptTemp\node-v$nodeVersion-win-x86"
  if ($options.WithNode)
  {
    if (-not $options.Local)
    {
      $source = "http://nodejs.org/dist/v$nodeVersion/node-v$nodeVersion-win-x86.7z"
      if (-not (test-path $node))
      {
        Write-Host "Download Node..."
        Invoke-WebRequest $source -OutFile "$scriptTemp\node-v$nodeVersion-win-x86.7z"
        if (-not $?) { throw "Failed to download Node." }
        &$sevenZip x "$scriptTemp\node-v$nodeVersion-win-x86.7z" -o"$scriptTemp" -aos > $nul
        $this.RemoveFile("$scriptTemp\node-v$nodeVersion-win-x86.7z")
      }
    }
    elseif (-not (test-path $node))
    {
      throw "Failed to locate Node."
    }
  }

  # ensure we have docfx
  $docfx = "$scriptTemp\docfx.ready"
  $docfxExe =""
  if ($options.WithDocFx)
  {
    if (-not $options.Local)
    {
      if ((test-path $docfx) -and ((ls $docfx).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
      {
        $this.RemoveFile($docfx)
      }
      if (-not (test-path $docfx))
      {
        Write-Host "Download DocFx..."
        $params = "-OutputDirectory", $scriptTemp, "-Verbosity", "quiet", "-Source", $nugetsource
        &$nuget install docfx.console @params
      }
      $dir = ls "$scriptTemp\docfx.console.*" | sort -property Name -descending | select -first 1
      $docfxExe = "$dir\tools\docfx.exe"
      if (-not (test-path $docfx))
      {
        cp "$docfxExe" $docfx
      }
    }
    elseif (-not (Test-Path $docfx))
    {
      throw "Failed to locate DocFx."
    }
  }

  # find visual studio
  # will not work on VS Online but VS Online does not need it
  $vs = $null
  if ($options.WithVs)
  {
    $vsPath = ""
    $vsVer = ""
    $msBuild = $null
    $toolsVersion = ""

    $vsMajor = if ($options.VsMajor) { $options.VsMajor } else { "16" } # default to 16 (VS2019) for now
    $vsMajor = [int]::Parse($vsMajor)

    $vsPaths = new-object System.Collections.Generic.List[System.String]
    $vsVersions = new-object System.Collections.Generic.List[System.Version]

    # parse vswhere output
    $params = @()
    if ($options.VsPreview) { $params += "-prerelease" }
    &$vswhere @params | ForEach-Object {
      if ($_.StartsWith("installationPath:")) { $vsPaths.Add($_.SubString("installationPath:".Length).Trim()) }
      if ($_.StartsWith("installationVersion:")) { $vsVersions.Add([System.Version]::Parse($_.SubString("installationVersion:".Length).Trim())) }
    }

    # get higest version lower than or equal to vsMajor
    $vsIx1 = -1
    $vsIx2 = -1
    $vsVersion = [System.Version]::Parse("0.0.0.0")
    $vsVersions | ForEach-Object {
      $vsIx1 = $vsIx1 + 1
      if ($_.Major -le $vsMajor -and $_ -gt $vsVersion) {
        $vsVersion = $_
        $vsIx2 = $vsIx1
      }
    }
    if ($vsIx2 -ge 0) {
      $vsPath = $vsPaths[$vsIx2]

      if ($vsVersion.Major -eq 16) {
        $msBuild = "$vsPath\MSBuild\Current\Bin"
        $toolsVersion = "Current"
      }
      if ($vsVersion.Major -eq 15) {
        $msBuild = "$vsPath\MSBuild\$($vsVersion.Major).0\Bin"
        $toolsVersion = "15.0"
      }
      elseif ($vsVersion.Major -eq 14) {
        $msBuild = "c:\Program Files (x86)\MSBuild\$($vsVersion.Major)\Bin"
        $toolsVersion = "4.0"
      }
    }

    if ($msBuild)
    {
      $vs = @{
        Path = $vsPath
        Major = $vsMajor
        Minor = $vsMinor
        MsBuild = "$msBuild\MsBuild.exe"
        ToolsVersion = $toolsVersion
      }
    }
  }

  $uenv = @{
    Options = $options
    NuGet = $nuget
  }

  if ($options.With7Zip) { $uenv.Zip = $sevenZip }
  if ($options.WithVs) { $uenv.VisualStudio = $vs ; $uenv.VsWhere = $vswhere }
  if ($options.WithSemver) { $uenv.Semver = $semver }
  if ($options.WithNode) { $uenv.NodePath = $node }
  if ($options.WithDocFx) { $uenv.DocFx = $docfxExe }

  return $uenv
})
