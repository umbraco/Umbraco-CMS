#
# Get-UmbracoBuildEnv
# Gets the Umbraco build environment
# Downloads tools if necessary
#
function Get-UmbracoBuildEnv
{
  # store tools in the module's directory
  # and cache them for two days
  $path = "$PSScriptRoot\temp"
  $cache = 2
  
  if (-not (test-path $path))
  {
    mkdir $path > $null
  }

  # ensure we have NuGet
  $nuget = "$path\nuget.exe"
  $source = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
  if ((test-path $nuget) -and ((ls $nuget).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    Remove-File $nuget
  }
  if (-not (test-path $nuget))
  {
    Write-Host "Download NuGet..."
    Invoke-WebRequest $source -OutFile $nuget
  }
  
  # ensure we have 7-Zip
  $sevenZip = "$path\7za.exe"
  if ((test-path $sevenZip) -and ((ls $sevenZip).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    Remove-File $sevenZip
  }
  if (-not (test-path $sevenZip))
  {
    Write-Host "Download 7-Zip..."
    &$nuget install 7-Zip.CommandLine -OutputDirectory $path -Verbosity quiet
    $dir = ls "$path\7-Zip.CommandLine.*" | sort -property Name -descending | select -first 1
    $file = ls -path "$dir" -name 7za.exe -recurse
    $file = ls -path "$dir" -name 7za.exe -recurse | select -first 1 #A select is because there is tools\7za.exe & tools\x64\7za.exe
    mv "$dir\$file" $sevenZip
    Remove-Directory $dir
  }
  
  # ensure we have vswhere
  $vswhere = "$path\vswhere.exe"
  if ((test-path $vswhere) -and ((ls $vswhere).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    Remove-File $vswhere
  }
  if (-not (test-path $vswhere))
  {
    Write-Host "Download VsWhere..."
    &$nuget install vswhere -OutputDirectory $path -Verbosity quiet
    $dir = ls "$path\vswhere.*" | sort -property Name -descending | select -first 1
    $file = ls -path "$dir" -name vswhere.exe -recurse
    mv "$dir\$file" $vswhere
    Remove-Directory $dir
  }
  
  # ensure we have semver
  $semver = "$path\Semver.dll"
  if ((test-path $semver) -and ((ls $semver).CreationTime -lt [DateTime]::Now.AddDays(-$cache)))
  {
    Remove-File $semver
  }
  if (-not (test-path $semver))
  {
    Write-Host "Download Semver..."
    &$nuget install semver -OutputDirectory $path -Verbosity quiet
    $dir = ls "$path\semver.*" | sort -property Name -descending | select -first 1
    $file = "$dir\lib\net452\Semver.dll"
    if (-not (test-path $file))
    {
      Write-Error "Failed to file $file"
      return
    }
    mv "$file" $semver
    Remove-Directory $dir
  }

  try
  {
    [Reflection.Assembly]::LoadFile($semver) > $null
  }
  catch
  {
    Write-Error -Exception $_.Exception -Message "Failed to load $semver"
    break
  }
  
  # ensure we have node
  $node = "$path\node-v6.9.1-win-x86"
  $source = "http://nodejs.org/dist/v6.9.1/node-v6.9.1-win-x86.7z"
  if (-not (test-path $node))
  {
    Write-Host "Download Node..."
    Invoke-WebRequest $source -OutFile "$path\node-v6.9.1-win-x86.7z"
    &$sevenZip x "$path\node-v6.9.1-win-x86.7z" -o"$path" -aos > $nul
    Remove-File "$path\node-v6.9.1-win-x86.7z"    
  }
  
  # note: why? node already brings everything we need!
  ## ensure we have npm
  #$npm = "$path\npm.*"
  #$getNpm = $true
  #if (test-path $npm)
  #{
  #  $getNpm = $false
  #  $tmpNpm = ls "$path\npm.*" | sort -property Name -descending | select -first 1
  #  if ($tmpNpm.CreationTime -lt [DateTime]::Now.AddDays(-$cache))
  #  {
  #    $getNpm = $true
  #  }
  #  else
  #  {
  #    $npm = $tmpNpm.ToString()
  #  }
  #}
  #if ($getNpm)
  #{
  #  Write-Host "Download Npm..."
  #  &$nuget install npm -OutputDirectory $path -Verbosity quiet
  #  $npm = ls "$path\npm.*" | sort -property Name -descending | select -first 1
  #  $npm.CreationTime = [DateTime]::Now
  #  $npm = $npm.ToString()
  #}
  
  # find visual studio
  # will not work on VSO but VSO does not need it
  $vsPath = ""
  $vsVer = ""
  $msBuild = $null
  &$vswhere | foreach {
    if ($_.StartsWith("installationPath:")) { $vsPath = $_.SubString("installationPath:".Length).Trim() }
    if ($_.StartsWith("installationVersion:")) { $vsVer = $_.SubString("installationVersion:".Length).Trim() }
  }
  if ($vsPath -ne "")
  {
    $vsVerParts = $vsVer.Split('.')
    $vsMajor = [int]::Parse($vsVerParts[0])
    $vsMinor = [int]::Parse($vsVerParts[1])
    if ($vsMajor -eq 15) {
      $msBuild = "$vsPath\MSBuild\$vsMajor.0\Bin"
    }
    elseif ($vsMajor -eq 14) {
      $msBuild = "c:\Program Files (x86)\MSBuild\$vsMajor\Bin"
    }
    else 
    {
      $msBuild = $null
    }
  }
 
  $vs = $null
  if ($msBuild)
  {
    $vs = new-object -typeName PsObject
    $vs | add-member -memberType NoteProperty -name Path -value $vsPath
    $vs | add-member -memberType NoteProperty -name Major -value $vsMajor
    $vs | add-member -memberType NoteProperty -name Minor -value $vsMinor
    $vs | add-member -memberType NoteProperty -name MsBuild -value "$msBuild\MsBuild.exe"
  }
  
  $solutionRoot = Get-FullPath "$PSScriptRoot\..\..\.."
  
  $uenv = new-object -typeName PsObject
  $uenv | add-member -memberType NoteProperty -name SolutionRoot -value $solutionRoot
  $uenv | add-member -memberType NoteProperty -name VisualStudio -value $vs
  $uenv | add-member -memberType NoteProperty -name NuGet -value $nuget
  $uenv | add-member -memberType NoteProperty -name Zip -value $sevenZip
  $uenv | add-member -memberType NoteProperty -name VsWhere -value $vswhere
  $uenv | add-member -memberType NoteProperty -name Semver -value $semver
  $uenv | add-member -memberType NoteProperty -name NodePath -value $node
  #$uenv | add-member -memberType NoteProperty -name NpmPath -value $npm
  
  return $uenv
}
