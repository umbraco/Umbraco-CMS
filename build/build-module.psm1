
# build-module.psm1
#
# import-module .\build-module.psm1
# get-module
# remove-module build-module

# returns a string containing the hash of $file
function genHash($file) 
{
  try 
  {
    $crypto = new-object System.Security.Cryptography.SHA1CryptoServiceProvider
    $stream = [System.IO.File]::OpenRead($file)
    $hash = $crypto.ComputeHash($stream)
    $text = ""
    $hash | foreach `
    {
      $text = $text + $_.ToString("x2")
    }
    return $text
  }
  finally
  {
    $stream.Dispose()
    $crypto.Dispose()
  }
}

# returns the full path if $file is relative to $pwd
function fullPath($file)
{
  $path = [System.IO.Path]::Combine($pwd, $file)
  $path = [System.IO.Path]::GetFullPath($path)
  return $path
}

# removes a directory, doesn't complain if it does not exist
function rmrf($dir)
{
  remove-item $dir -force -recurse -errorAction SilentlyContinue
}

# removes a file, doesn't complain if it does not exist
function rmf($file)
{
  remove-item $file -force -errorAction SilentlyContinue
}

# copies a file, creates target dir if needed
function cpf($source, $target)
{
  $ignore = new-item -itemType file -path $target -force
  cp -force $source $target
}

# copies files to a directory
function cprf($source, $select, $target, $filter)
{
  $files = ls -r "$source\$select"
  $files | foreach {
    $relative = $_.FullName.SubString($source.Length+1)
    $_ | add-member -memberType NoteProperty -name RelativeName -value $relative
  }
  if ($filter -ne $null) {
    $files = $files | where $filter 
  }
  $files |
    foreach {
      if ($_.PsIsContainer) {
        $ignore = new-item -itemType directory -path "$target\$($_.RelativeName)" -force
      }
      else {
        cpf $_.FullName "$target\$($_.RelativeName)"
      }
    }
}

# loads the semver dll
function loadSemVer() {
  $semverlib = fullPath("..\src\packages\Semver.2.0.4\lib\net452\Semver.dll")
  if (-not (test-path $semverlib)) {
    write-error "Missing packages\Semver.2.0.4\lib\net452\Semver.dll."
    exit 1
  }
  $assembly = [Reflection.Assembly]::LoadFile($semverlib)
}

# regex-replaces content in a file
function fileReplace($filename, $source, $replacement) {
  $filepath = fullPath $filename
  $text = [System.IO.File]::ReadAllText($filepath)
  $text = [System.Text.RegularExpressions.Regex]::Replace($text, $source, $replacement)
  [System.IO.File]::WriteAllText($filepath, $text)
}

# finds msbuild
function findVisualStudio($vswhere) {
  $vsPath = ""
  $vsVer = ""
  &$vswhere | foreach {
    if ($_.StartsWith("installationPath:")) { $vsPath = $_.SubString("installationPath:".Length).Trim() }
    if ($_.StartsWith("installationVersion:")) { $vsVer = $_.SubString("installationVersion:".Length).Trim() }
  }
  if ($vsPath -eq "") { return $null }
  
  $vsVerParts = $vsVer.Split('.')
  $vsMajor = [int]::Parse($vsVerParts[0])
  $vsMinor = [int]::Parse($vsVerParts[1])
  if ($vsMajor -eq 15) {
    $msBuild = "$vsPath\MSBuild\$vsMajor.$vsMinor\Bin"
  }
  elseif ($vsMajor -eq 14) {
    $msBuild = "c:\Program Files (x86)\MSBuild\$vsMajor\Bin"
  }
  else { return $null }
  $msBuild = "$msBuild\MsBuild.exe"
  
  $vs = new-object -typeName PsObject
  $vs | add-member -memberType NoteProperty -name Path -value $vsPath
  $vs | add-member -memberType NoteProperty -name Major -value $vsMajor
  $vs | add-member -memberType NoteProperty -name Minor -value $vsMinor
  $vs | add-member -memberType NoteProperty -name MsBuild -value $msBuild
  return $vs
}