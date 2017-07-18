# returns a string containing the hash of $file
function Get-FileHash($file) 
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
    if ($stream)
    {
      $stream.Dispose()
    }
    $crypto.Dispose()
  }
}

# returns the full path if $file is relative to $pwd
function Get-FullPath($file)
{
  $path = [System.IO.Path]::Combine($pwd, $file)
  $path = [System.IO.Path]::GetFullPath($path)
  return $path
}

# removes a directory, doesn't complain if it does not exist
function Remove-Directory($dir)
{
  remove-item $dir -force -recurse -errorAction SilentlyContinue > $null
}

# removes a file, doesn't complain if it does not exist
function Remove-File($file)
{
  remove-item $file -force -errorAction SilentlyContinue > $null
}

# copies a file, creates target dir if needed
function Copy-File($source, $target)
{
  $ignore = new-item -itemType file -path $target -force
  cp -force $source $target
}

# copies files to a directory
function Copy-Files($source, $select, $target, $filter)
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
        Copy-File $_.FullName "$target\$($_.RelativeName)"
      }
    }
}

# regex-replaces content in a file
function Replace-FileText($filename, $source, $replacement)
{
  $filepath = Get-FullPath $filename
  $text = [System.IO.File]::ReadAllText($filepath)
  $text = [System.Text.RegularExpressions.Regex]::Replace($text, $source, $replacement)
  $utf8bom = New-Object System.Text.UTF8Encoding $true
  [System.IO.File]::WriteAllText($filepath, $text, $utf8bom)
}
