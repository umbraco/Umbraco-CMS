
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
