#

$line = 0
$release = ""
$comment = ""

get-content version.txt | foreach {
  if ($line -eq 1)  {
    $release = $_
  }
  if ($line -eq 2)  {
    $comment = $_
  }
  $line = $line + 1
}

if ($release -eq "") {
  Write-Error "Invalid release."
  Exit 1
}

# set environment variable
# https://github.com/Microsoft/vsts-tasks/issues/375
# https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md

Write-Host ("##vso[task.setvariable variable=UMBRACO_RELEASE;]$release")
Write-Host ("##vso[task.setvariable variable=UMBRACO_COMMENT;]$comment")
