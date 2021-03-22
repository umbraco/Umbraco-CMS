# Set environment variables for artillery to use:
# TODO: Cypress generates this information so either use what cypress generates or use the code that it uses

$Env:U_BASE_URL = "http://localhost:8121/"
$Env:U_USERNAME = "sdeminick2@gmail.com"
$Env:U_PASS = "testtesttest"
$Env:U_SERVERNAME = "TEAMCANADA3"
$Env:U_PROCESSNAME = "iisexpress"
$Env:U_SCRIPTROOT = $PSScriptRoot
$Env:U_DEBUG = 'true'

$RunCount = 1;
$Rate = 1;
$ArtilleryOverrides = '{""config"": {""phases"": [{""duration"": ' + $RunCount + ', ""arrivalRate"": ' + $Rate + '}]}}'

# good posts on Private Bytes vs Working Set
#   https://stackoverflow.com/a/1986486/694494
#   https://michaelscodingspot.com/performance-counters/
# Export-Counter not available in PS7!? :( https://stackoverflow.com/questions/64950228/export-counter-missing-from-powershell-7-1
# Perhaps we can use PS for NET Framework projects and then dotnet-counters global tool for NET Core projects?
# https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters
# Just so long as the values are consistent which I believe they will be

# Start counters
$Counters = @(
    "\\$Env:U_SERVERNAME\Process($Env:U_PROCESSNAME)\% Processor Time",
    # We won't use Private Bytes because this is not compat with dotnet-counters
    #"\\$Env:U_SERVERNAME\Process($Env:U_PROCESSNAME)\Private Bytes",
    "\\$Env:U_SERVERNAME\Process($Env:U_PROCESSNAME)\Working Set",
    # We don't care too much about this one, it normally is just small/consistent Gen1+ is the important ones
    "\\$Env:U_SERVERNAME\.NET CLR Memory($Env:U_PROCESSNAME)\Gen 0 heap size",
    "\\$Env:U_SERVERNAME\.NET CLR Memory($Env:U_PROCESSNAME)\Gen 1 heap size",
    "\\$Env:U_SERVERNAME\.NET CLR Memory($Env:U_PROCESSNAME)\Gen 2 heap size",
    "\\$Env:U_SERVERNAME\.NET CLR Memory($Env:U_PROCESSNAME)\Large Object Heap size",
    # Includes the sum of all managed heaps – Gen 0 + Gen 1 + Gen 2 + LOH. This represents the allocated managed memory size.
    "\\$Env:U_SERVERNAME\.NET CLR Memory($Env:U_PROCESSNAME)\# Bytes in all Heaps")

# script to stream counter data that will be used on the Start-Job background job
$GetCountersScript = {
    Get-Counter -Counter $args[0] -ComputerName $args[1] -Continuous | Export-Counter -path "$($args[2])" -FileFormat csv -Force
}

## Clear our temp storage first
if (Test-Path "$PSScriptRoot\output\run.tmp"){
    Remove-Item "$PSScriptRoot\output\run.tmp"
}

$Artillery = @(
    "login-and-load.yml",

    # TODO: Ok, so now we need to be able to set a few GUIDs as env variables I think:
    # Set a GUID for a new content type that we'll create which can be used for it's aliases, etc...
    "create-doctype.yml"
)

# set special artillery debug switches
if ($Env:U_DEBUG -eq 'true') {
    $Env:DEBUG = "http,http:capture,http:response"
}

foreach ( $a in $Artillery )
{
    # start the counter collection on a background task, since we specified continuous this will keep going till we stop the task
    $Job = Start-Job $GetCountersScript -Name "MyCounters" -ArgumentList $Counters,$Env:U_SERVERNAME,"$PSScriptRoot\output\$a.csv"
    $Job | Receive-Job -Keep

    try {
        # Execute artillery
        # TODO: We can adjust the phase duration time from here: https://webkul.com/blog/artillery-execute-script/
        # like: artillery run –overrides ‘{“config”: {“phases”: [{“duration”: 10, “arrivalRate”: 1}]}}’ test. yaml

        $Artillery | & artillery run "$PSScriptRoot\artillery\$a" --output "$PSScriptRoot\output\$a.json" --target $Env:U_BASE_URL --overrides $ArtilleryOverrides
        & artillery report --output "$PSScriptRoot\output\$a.html" "$PSScriptRoot\output\$a.json"
    }
    finally {
        Write-Verbose "Stopping counters"
        $Job | Receive-Job -Keep # see if there are any errors
        $Batch = Get-Job -Name "MyCounters"
        $Batch | Stop-Job
        $Batch | Remove-Job
    }
}
