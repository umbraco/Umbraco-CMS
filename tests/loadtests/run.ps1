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


function Start-CounterCollection {

    # Start counters collection, which will either be by Windows counters
    # for .NET Framework, else it will be dotnet-counters for dotnet core
    # apps. Both will collect the same statistics.

    # good posts on Private Bytes vs Working Set
    #   https://stackoverflow.com/a/1986486/694494
    #   https://michaelscodingspot.com/performance-counters/

    param (
        [Parameter(Mandatory=$true)]
        [string]
        $CounterType,

        [Parameter(Mandatory=$true)]
        [string]
        $ProcessName,

        [Parameter(Mandatory=$true)]
        [string]
        $ServerName,

        [Parameter(Mandatory=$true)]
        [string]
        $ReportName
    )

    if ($CounterType -eq "windows") {
        return Start-WindowsCounters -ProcessName $ProcessName -ServerName $ServerName -ReportName $ReportName
    }
    elseif ($CounterType -eq "dotnet") {
        return Start-DotNetCounters -ProcessName $ProcessName -ReportName $ReportName
    }
    else {
        throw "Counter type not supported"
    }
}

function Start-WindowsCounters {

    # Collect windows counters for .NET Framework apps.
    # Unfortunately this doesn't work in PS7, only PS6 for Windows.
    # Export-Counter not available in PS7!? :( https://stackoverflow.com/questions/64950228/export-counter-missing-from-powershell-7-1

    param (
        [Parameter(Mandatory=$true)]
        [string]
        $ProcessName,

        [Parameter(Mandatory=$true)]
        [string]
        $ServerName,

        [Parameter(Mandatory=$true)]
        [string]
        $ReportName
    )

    $Counters = @(
        "\\$($ServerName)\Process($($ProcessName))\% Processor Time",
        # We won't use Private Bytes because this is not compat with dotnet-counters
        #"\\$($ServerName)\Process($($ProcessName))\Private Bytes",
        # Working set docs are here https://docs.microsoft.com/en-us/windows/win32/memory/working-set
        # Also, https://docs.microsoft.com/en-us/aspnet/core/performance/memory?view=aspnetcore-5.0
        # "The working set shown is the same value Task Manager displays."
        "\\$($ServerName)\Process($($ProcessName))\Working Set",
        # In theory this is the same as the dotnet-counter value for gc-heap-size which
        # is "Total heap size reported by the GC (MB)"
        # See https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/performance#to-determine-how-much-memory-the-managed-heap-is-committing
        # Which states to not use Bytes In All Heaps as a way to determine what managed bytes are committed
        # and this metric to be used instead. We'll track both.
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\# Total committed bytes",
        # We don't care too much about this one much:
        # See https://docs.microsoft.com/en-us/previous-versions/x2tyfybc(v=vs.110)?redirectedfrom=MSDN
        # Gen 0 displays the maximum bytes that can be allocated in generation 0; it does not indicate the current number of bytes allocated in generation 0.
        # "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Gen 0 heap size",
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Gen 1 heap size",
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Gen 2 heap size",
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\Large Object Heap size",
        # Includes the sum of all managed heaps – Gen 1 + Gen 2 + LOH. This represents the allocated managed memory size.
        # NOTE: Some sites say its Gen0,1,2,LOH and that is untrue - it is only 1,2,LOH
        "\\$($ServerName)\.NET CLR Memory($($ProcessName))\# Bytes in all Heaps")

    # script to stream counter data that will be used on the Start-Job background job
    # will collect in 1 second intervals
    $GetCountersScript = {
        Get-Counter -Counter $args[0] -ComputerName $args[1] -Continuous | Export-Counter -path "$($args[2])" -FileFormat csv -Force
    }

    # start the counter collection on a background task, since we specified continuous this will keep going till we stop the task
    $Job = Start-Job $GetCountersScript -Name "MyCounters" -ArgumentList $Counters,$($ServerName),"$PSScriptRoot\output\$ReportName.csv"
    $Job | Receive-Job -Keep

    return $Job
}

function Start-DotNetCounters {
    param (
        [Parameter(Mandatory=$true)]
        [string]
        $ProcessName,

        [Parameter(Mandatory=$true)]
        [string]
        $ReportName
    )

    # NOTE: Not including Gen0 (see notes above, it's not really relevant for comparison)
    $Counters = "System.Runtime[cpu-usage,working-set,gen-1-size,gen-2-size,loh-size,gc-heap-size]"

    # We need to use Start-Process and cannot use Start-Job because the exe file doesn't handle console
    # redirects properly. See https://github.com/dotnet/diagnostics/issues/451#issuecomment-842741702
    $Job = Start-Process -FilePath "dotnet-counters" -PassThru -Verb RunAs -ArgumentList "collect","--name",$($ProcessName),"--refresh-interval","1","--format","csv","--output","$PSScriptRoot\output\$ReportName.csv","--counters",$($Counters)

    Write-Verbose "Process Start Info: $($Job.StartInfo.Arguments)"

    return $Job
}

Add-Type -AssemblyName Microsoft.VisualBasic
Add-Type -AssemblyName System.Windows.Forms


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
    # This is actually going to be quite important to have correct metrics, else there's a few risks:
    # There will be a lot of content in each of these DBs the more they are run, therefore possibly
    # slowing things down. If there's instructions in the DB, then background tasks will processs those
    # instructions which will skew metrics because there will be more going on than just what is in the metrics.
    # TODO: We need to also disable all auto health checks on all sites since that is another background task that runs.
    # TODO: There is probably also other background tasks we should disable.
    # TODO: What about ModelsBuilder? Enabled/Disabled? PureLive will cause quite an overhead.

    # Reset iis (stops all w3wp processes)
    & iisreset /restart

    # Stop all IIS Sites since restarting iis can put all sites back into a start phase
    Get-IISSite | Stop-IISSite -Name {$_.Name} -ErrorAction SilentlyContinue -Confirm:$false

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

    if(!$?)
    {
        exit 1
    }

    # These need to be env vars since they are used in artillery
    $Env:U_USERNAME = $app.username
    $Env:U_PASS = $app.password

    ## Clear our output first
    if (Test-Path "$PSScriptRoot\output"){
        Remove-Item "$PSScriptRoot\output\" -Force -Recurse
    }
    New-Item -Path "$PSScriptRoot\" -Name "output" -ItemType "directory" | Out-Null

    $Artillery = @(
        "warmup.yml",
        "login-and-load.yml",
        "create-doctype.yml",
        "create-content.yml",
        "delete-content.yml"
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
        # It is possible on the same machine by using something like this
        # https://github.com/cklutz/EtwForceGC,
        # https://stackoverflow.com/a/26033289
        # So potentially, we may want to host a custom endpoint when we automate the installation of Umbraco to be able to be called
        # to just call GC.Collect on that machine.

        # start the counter collection on a background task, since we specified continuous this will keep going till we stop the task
        $Job = Start-CounterCollection -CounterType $app.counterCollection -ProcessName $ProcessName -ServerName $ServerName -ReportName $a

        if(!$?)
        {
            exit 1
        }

        # Short pause while they initialize
        Start-Sleep -Seconds 1

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
            Write-Host "Stopping counters"

            # If it's the dotnet-counters then the $Job will be a .net Process so we can check for
            # one of it's properties
            if ($Job.CloseMainWindow) {

                # TODO: This is SO HORRIBLE :( but there's nothing we can do because the
                # dotnet team did not make this scriptable. I've looked at the code and can't see
                # a way to terminate a process forcing the console app too exit correctly, only with a 'Q'
                # command.
                # See https://github.com/dotnet/diagnostics/issues/451#issuecomment-843650234
                Write-Host "Sending exit command"
                [Microsoft.VisualBasic.Interaction]::AppActivate($($Job.Id))
                [System.Windows.Forms.SendKeys]::SendWait("Q")
                Start-Sleep -Seconds 3

                if ($Job.ExitCode -ne 0) {
                    Write-Host "Could not Stop dotnet-counters, metrics will not be stored"

                    # we need to kill it if it didn't exit
                    $Job.Close();
                    $Job.Kill();
                }

            }
            else {
                $Job | Receive-Job -Keep # see if there are any errors
                $Batch = Get-Job -Name "MyCounters"
                $Batch | Stop-Job
                $Batch | Remove-Job
            }
        }
    }

    Stop-IISSite -Name $app.iisSiteName -ErrorAction SilentlyContinue -Confirm:$false

    # Run the node app to push our reports
    & node .\app.js --send-report --umb-version $($app.umbracoVersion) --spec-name $SpecName --cosmos-endpoint $CosmosDbEndpoint --cosmos-key $CosmosDbKey --cosmos-database-id $CosmosDbDatabaseId --cosmos-container-id $CosmosDbContainerId --perf-report-type $($app.counterCollection)

    if(!$?)
    {
        exit 1
    }

    # Short pause before switching to the next app
    Start-Sleep -Seconds 5
}

& node .\app.js --show-report --spec-name $SpecName --cosmos-endpoint $CosmosDbEndpoint --cosmos-key $CosmosDbKey --cosmos-database-id $CosmosDbDatabaseId --cosmos-container-id $CosmosDbContainerId
