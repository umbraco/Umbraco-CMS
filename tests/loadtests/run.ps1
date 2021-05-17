param (
    # Something unique for the specific testing hardware/specs used for this test.
    # Used for logging to the reporting data store.
    # Example: "TEAMCANADA3-CG21"
	[Parameter(Mandatory=$true)]
	[string]
	$SpecName,

    [Parameter(Mandatory=$false)]
	[int]
	$RunCount = 1,

    [Parameter(Mandatory=$false)]
	[int]
	$Rate = 1
)

# Set environment variables for artillery to use:
# TODO: Cypress generates this information so either use what cypress generates or use the code that it uses

# for verbose logging, env var is used in artillery too
if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent) {
    $Env:U_DEBUG = 'true'
}
else {
    $Env:U_DEBUG = 'false'
}

$Env:U_SCRIPTROOT = $PSScriptRoot

# TODO: Read the config
$ConfigFile = "$PSScriptRoot\config.json"
$Config = Get-Content $ConfigFile | ConvertFrom-Json

$ServerName = $Config.serverName

$CosmosDbEndpoint = $Config.cosmos.endpoint
$CosmosDbKey = $Config.cosmos.key
$CosmosDbDatabaseId = $Config.cosmos.databaseId
$CosmosDbContainerId = $Config.cosmos.containerId

$ArtilleryOverrides = '{""config"": {""phases"": [{""duration"": ' + $RunCount + ', ""arrivalRate"": ' + $Rate + '}]}}'

## Get the w3wp processes
## The process id for counters is like w3wp#4, but the number is just an incremented number
## it is not guessable. The only way is to query the id processes to get the process id
## for anything starting with w3wp*.
## Then we need to use these process id to query for the corresponding app domain
#$w3wpCounterProcesses = (Get-Counter -Counter "\Process(w3wp*)\ID Process").CounterSamples

Foreach ($app in $Config.apps)
{
    # TODO: Ideally we'd start with fresh DBs and do the install as part of an artillery run

    $ProcessName = $app.processName

    if ($app.iisSiteName)
    {
        # Reset iis (stops all w3wp processes)
        & iisreset /restart

        # Stop all IIS Sites
        Get-IISSite | Stop-IISSite -Name {$_.Name}  -ErrorAction SilentlyContinue -Confirm:$false

        # This would be required if we don't stop all sites in order to get the correct IIS process Id
        # for perf counters. But since we are stopping them all, then the process name will always
        # just be w3wp.

        ## get iis site by binding info
        #$siteBinding = $app.baseUrl.Split("://", [System.StringSplitOptions]::RemoveEmptyEntries)[1]

        #Write-Host "Looking up site by binding $siteBinding"

        #$iisSite = Get-IISSite | Where-Object {$_.Bindings[0].bindingInformation -Match "$siteBinding$" }
        #if (!$iisSite -or $iisSite.Count -ne 1){
        #    throw "Could not find IIS site with binding $siteBinding"
        #}

        #Write-Host "IIS Site ID = $iisSite.Id"

        ## find the w3wp counter process for the site id
        #$counterProcess = $w3wpCounterProcesses | Where-Object { (Get-WebAppDomain -ProcessId $_.CookedValue).siteId -eq $iisSite.Id }
        ## then we need to get the w3wp part (it will be like "\\teamcanada3\process(w3wp#3)\id process")
        #$ProcessName = ($counterProcess.Path | Select-String -Pattern 'process\((.*?)\)').Matches[0].Groups[1].Value

        $ProcessName = "w3wp"

        Write-Host "IIS Process name = $($ProcessName)"

        Write-Host "Starting Site $($app.iisSiteName)"
        Start-IISSite -Name $app.iisSiteName
    }
    elseif (!$ProcessName) {
        throw "Configuration can only be iis or process name based"
    }

    if(!$?)
    {
        exit 1
    }

    # These need to be env vars since they are used in artillery
    $Env:U_USERNAME = $app.username
    $Env:U_PASS = $app.password

    # good posts on Private Bytes vs Working Set
    #   https://stackoverflow.com/a/1986486/694494
    #   https://michaelscodingspot.com/performance-counters/
    # Export-Counter not available in PS7!? :( https://stackoverflow.com/questions/64950228/export-counter-missing-from-powershell-7-1
    # Perhaps we can use PS for NET Framework projects and then dotnet-counters global tool for NET Core projects?
    # https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters
    # Just so long as the values are consistent which I believe they will be

    # Start counters
    $Counters = @(
        "\\$($ServerName)\Process($($ProcessName))\% Processor Time",
        # We won't use Private Bytes because this is not compat with dotnet-counters
        #"\\$($ServerName)\Process($($ProcessName))\Private Bytes",
        "\\$($ServerName)\Process($($ProcessName))\Working Set",
        # We don't care too much about this one, it normally is just small/consistent Gen1+ is the important ones
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Gen 0 heap size",
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Gen 1 heap size",
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Gen 2 heap size",
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Large Object Heap size",
        # Includes the sum of all managed heaps – Gen 0 + Gen 1 + Gen 2 + LOH. This represents the allocated managed memory size.
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\# Bytes in all Heaps")

    # script to stream counter data that will be used on the Start-Job background job
    $GetCountersScript = {
        Get-Counter -Counter $args[0] -ComputerName $args[1] -Continuous | Export-Counter -path "$($args[2])" -FileFormat csv -Force
    }

    ## Clear our output first
    if (Test-Path "$PSScriptRoot\output"){
        Remove-Item "$PSScriptRoot\output\" -Force -Recurse
    }
    New-Item -Path "$PSScriptRoot\" -Name "output" -ItemType "directory" | Out-Null

    $Artillery = @(
        "warmup.yml",
        "login-and-load.yml",
        "create-doctype.yml",
        "create-content.yml"
    )

    # set special artillery debug switches
    if ($Env:U_DEBUG -eq 'true') {
        $Env:DEBUG = "http,http:capture,http:response"
    }
    else {
        $Env:DEBUG = ""
    }

    # First run the warmup without counters so IIS is started
    Write-Host "Site warmup..."
    $Artillery | & artillery run "$PSScriptRoot\artillery\warmup.yml" --target $($app.baseUrl)

    if ($app.iisSiteName)
    {
        # Recycle the app pool
        # https://docs.microsoft.com/en-us/powershell/module/iisadministration/get-iisservermanager?view=windowsserver2019-ps#example-3--use-the-servermanager-object-to-get-application-pool-objects-and-recycle-an-application-pool-
        $SM = Get-IISServerManager
        Write-Host "Recycle app pool..."
        $SM.ApplicationPools["UmbracoCms.8.7.3"].Recycle()
    }

    # Run the artillery load testing
    foreach ( $a in $Artillery )
    {
        # TODO: Ideally we would force a GC Collect here. BUT that's not really possible when running from a remote machine.
        # It is possible on the same machine by using something like this https://github.com/cklutz/EtwForceGC, https://stackoverflow.com/a/26033289
        # So potentially, we may want to host a custom endpoint when we automate the installation of Umbraco to be able to be called
        # to just call GC.Collect on that machine.

        # start the counter collection on a background task, since we specified continuous this will keep going till we stop the task
        $Job = Start-Job $GetCountersScript -Name "MyCounters" -ArgumentList $Counters,$($ServerName),"$PSScriptRoot\output\$a.csv"
        $Job | Receive-Job -Keep

        try {
            # Execute artillery
            # TODO: We can adjust the phase duration time from here: https://webkul.com/blog/artillery-execute-script/
            # like: artillery run –overrides ‘{“config”: {“phases”: [{“duration”: 10, “arrivalRate”: 1}]}}’ test. yaml

            $Artillery | & artillery run "$PSScriptRoot\artillery\$a" --output "$PSScriptRoot\output\$a.json" --target $($app.baseUrl) --overrides $ArtilleryOverrides

            if($?)
            {
                & artillery report --output "$PSScriptRoot\output\$a.html" "$PSScriptRoot\output\$a.json"
            }
            else
            {
                Write-Error "Artillery request failed. Exiting process."
                exit 1
            }
        }
        finally {
            Write-Verbose "Stopping counters"
            $Job | Receive-Job -Keep # see if there are any errors
            $Batch = Get-Job -Name "MyCounters"
            $Batch | Stop-Job
            $Batch | Remove-Job
        }
    }

    # Run the node app to push our reports
    & node .\app.js $($app.umbracoVersion) $SpecName $CosmosDbEndpoint $CosmosDbKey $CosmosDbDatabaseId $CosmosDbContainerId

}
