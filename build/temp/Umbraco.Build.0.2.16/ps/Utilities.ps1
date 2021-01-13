
# returns a string containing the hash of $file
$ubuild.DefineMethod("GetFileHash",
{
  param ( $file )

  try 
  {
    $crypto = New-Object System.Security.Cryptography.SHA1CryptoServiceProvider
    $stream = [System.IO.File]::OpenRead($file)
    $hash = $crypto.ComputeHash($stream)
    $text = ""
    $hash | ForEach-Object `
    {
      $text = $text + $_.ToString("x2")
    }
    return $text
  }
  finally
  {
    if ($stream)
    {
      $stream.Dispose()
    }
    $crypto.Dispose()
  }
})

# returns the full path if $file is relative to $pwd
$ubuild.DefineMethod("GetFullPath",
{
  param ( $file )

  $path = [System.IO.Path]::Combine($pwd, $file)
  $path = [System.IO.Path]::GetFullPath($path)
  return $path
})

# removes a directory, doesn't complain if it does not exist
$ubuild.DefineMethod("RemoveDirectory",
{
  param ( $dir )

  $errCount = $error.Count
  Remove-Item $dir -force -recurse -errorAction SilentlyContinue > $null

  # stupid PS "silently continues" yet creates an error
  while ($error.Count -ne $errCount)
  {
    $error.RemoveAt(0)
  }
})

# removes a file, doesn't complain if it does not exist
$ubuild.DefineMethod("RemoveFile",
{
  param ( $file )

  $errCount = $error.Count
  Remove-Item $file -force -errorAction SilentlyContinue > $null

  # stupid PS "silently continues" yet creates an error
  while ($error.Count -ne $errCount)
  {
    $error.RemoveAt(0)
  }
})

# copies a file, creates target dir if needed
$ubuild.DefineMethod("CopyFile",
{
  param ( $source, $target )

  if ($target.EndsWith("/"))
  {
    $ignore = new-item -itemType directory -path $target -force
  }
  else
  {
    $path = [System.IO.Path]::GetDirectoryName($target)
    $ignore = new-item -itemType directory -path $path -force
  }
  Copy-Item -force $source $target
})

# copies files to a directory
$ubuild.DefineMethod("CopyFiles",
{
  param ( $source, $select, $target, $filter )

  $files = Get-ChildItem -r "$source\$select"
  $files | Foreach-Object {
    $relative = $_.FullName.SubString($source.Length+1)
    $_ | add-member -memberType NoteProperty -name RelativeName -value $relative
  }
  if ($filter -ne $null) {
    $files = $files | Where-Object $filter 
  }
  $files |
    Foreach-Object {
      if ($_.PsIsContainer) {
        $ignore = new-item -itemType directory -path "$target\$($_.RelativeName)" -force
      }
      else {
        $this.CopyFile($_.FullName, "$target\$($_.RelativeName)")
      }
    }
})

# regex-replaces content in a file
$ubuild.DefineMethod("ReplaceFileText",
{
  param ( $filename, $source, $replacement )

  $filepath = $this.GetFullPath($filename)
  $text = [System.IO.File]::ReadAllText($filepath)
  $text = [System.Text.RegularExpressions.Regex]::Replace($text, $source, $replacement)
  $utf8bom = New-Object System.Text.UTF8Encoding $true
  [System.IO.File]::WriteAllText($filepath, $text, $utf8bom)
})

# VS online export env variable
$ubuild.DefineMethod("SetEnv",
{
  param ( $name, $value )
  [Environment]::SetEnvironmentVariable($name, $value)

  # set environment variable for VSO
  # https://github.com/Microsoft/vsts-tasks/issues/375
  # https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
  Write-Host ("##vso[task.setvariable variable=$name;]$($value)")
})

# temp store file under file.temp-build
$ubuild.DefineMethod("TempStoreFile",
{
  param ( $path )
  $name = [System.IO.Path]::GetFileName($path)

  if (Test-Path "$path")
  {
    if (Test-Path "$path.temp-build")
    {
      Write-Host "Found already existing $name.temp-build"
      Write-Host "(will be restored after build)"
    }
    else
    {
      Write-Host "Save existing $name as $name.temp-build"
      Write-Host "(will be restored after build)"
      Move-Item "$path" "$path.temp-build"
    }
  }
})

# restores a file that was temp stored under file.temp-build
$ubuild.DefineMethod("TempRestoreFile",
{
  param ( $path )
  $name = [System.IO.Path]::GetFileName($path)

  if (Test-Path "$path.temp-build")
  {
    Write-Host "Restoring existing $name"
    $this.RemoveFile("$path")
    Move-Item "$path.temp-build" "$path"
  }
})

# clears an environment variable
$ubuild.DefineMethod("ClearEnvVar",
{
  param ( $var )
  $value = [Environment]::GetEnvironmentVariable($var)
  if (Test-Path "env:$var") { Remove-Item "env:$var" }
  return $value
})

# sets an environment variable
$ubuild.DefineMethod("SetEnvVar",
{
  param ( $var, $value )
  if ($value)
  {
    [Environment]::SetEnvironmentVariable($var, $value)
  }
  else
  {
    if (Test-Path "env:$var") { rm "env:$var" }
  }
})

# unrolls errors
$ubuild.DefineMethod("WriteException",
{
  param ( $e )

  Write-Host "Exception!"
  while ($e -ne $null)
  {
    $ii = $e.ErrorRecord.InvocationInfo
    Write-Host "$($e.GetType().Name): $($e.Message)"
    if ($ii -ne $null)
    {
      Write-Host "## $($ii.ScriptName) $($ii.ScriptLineNumber):$($ii.OffsetInLine)"
      Write-Host "   $($ii.Line.Trim())"
    }
    Write-Host " "
    $e = $e.InnerException
  }
})