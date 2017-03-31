[CmdletBinding()]
$value = $Env:TESTVALUE
if ($value -ne "kawabu")
{
	Write-Error "Invalid value"
	Exit 1
}