# Umbraco CMS Performance Benchmarks

This application is used to run load tests against different Umbraco CMS versions to report on the performance differences.

This application consists of:

* Powershell - the host and entry point of the application
* ArtilleryIO - the load testing tool
* NodeJs - extensions for ArtilleryIO and a small app to send data results to the data store
* Windows performance counters - to report on memory and CPU usage for each scenario for .NET Framework based CMS versions
* dotnet-counters - to report on memory and CPU usage for each scenario for .NET 5 based CMS versions
* CosmosDb - the data store for the report output
* PowerBI - to create the reports

## Requirements

* Windows 10/Server OS
* Windows Powershell 6 (because we need to collection windows performance counters, Powershell 7 doesn't support the reporting requirements)
* Execution must run with a UI context (i.e. cannot be automated in the background - see below under Known Issues)
* IIS configured for each CMS version
* MSSQL configured for each version - ideally hosted on a different server
* The Starter Kit installed for each version - and nothing else - the data sets between versions must be consistent
* The custom Umbraco.TestData.dll is part of the app (either in the /bin or published as part of a netcore build)
* A CosmosDB service
* Power BI

## Usage

You must setup a `config.json` file to use. An example of this config file is the `config.example.json`. Each IIS site needs to be listed that is going to be tested.

Start by launching Powershell 6 console as Administrator.

* Entry point: `.\run.ps1`
* Arguments
  * `SpecName` - This is used for reporting to differentiate different test environments. If this tool was used on a VM that was setup to only ever run these tests then this value should remain constant so that these tests can be run anytime and the statistics gathered will be comparable. If this tool is run on your own computer then this value should be something unique to that execution since running it again on your computer at a different time would probably yield non-comparable metrics since your computer would probably be under a different load at any given time.
  * `Duration` - How long in Seconds each scenario will run for. This value is passed to ArtilleryIO as it's `duration` value.
  * `Rate` - How many concurrent virtual users will execute for each scenario. By default this is one and is recommended to leave as 1. The front-end load tests use their own duration/rate values to measure maximum throughput.
  * `ServerName` - the server name of the IIS host. This defaults to the current machine name.

  __Example__

  `.\run.ps1 -SpecName MyComputerTest105 -Duration 30`

Run the test suite with 30 seconds for each scenario and a custom spec name. When reporting, for this test run you would then use the `MyComputerTest105` spec name for the value lookup.

## Reporting

Reporting is done via PowerBI with a Cosmos DB connection. The `pbix` file used for HQ reports is not available in this repository.

It is important that the data source is filtered on the `SpecName` that you want to have results for. Without the filter, reporting all figures for all names will have misleading results.

## Future

### Host

The ideal setup would be:

* A load test host would be started from a docker image
* The setup and hosting of each version would be automated including installing the Starter Kit (i.e with Azure web apps and an Azure SQL database).

Unfortunately the automation could only be possible for later Umbraco versions that support unattended installs which means we wouldn't be able to compare older versions. The other problem with this type of automation is that it won't be possible (as far as I know) to collect performance counter metrics. There may be a way to do that but it will be difficult to figure out a way to do this for both .net framework and .net 5 web apps. Also because Windows Powershell 6 is needed for collecting perf counters, the host could not be a linux host so a docker image won't really work.

Alternatively - an easier and quicker solution is:

* Setup an Azure VM with IIS configured with all of the sites.
* Configure Azure SQL DBs for each version.
* We would need to manually setup an IIS and DB for each new version we want to add for testing - but this doesn't take too long.
* When we want to re-run the script, we can boot the VM and execute the script.

### Tests

We need to keep adding more scenarios. Each scenario added may take some work if it relies on previously created data from another scenario. This is supported in this framework but something to keep in mind when creating new ones.

Currently this is only testing the CMS with the Starter Kit installed which is very little data. Ideally we'd run tests against versions with the SK and also when there is tons of data/content/media in the DB.

## How it works

When the powershell script executes, the config file is loaded and debug flags are set for environment variables to configure Artillery execution. Then for each IIS site configured:

* Reset IIS
* Stop all IIS sites
* Start the single IIS site that is being tested
* Recycle that IIS site's app pool
* Environment variables are set based on the current site config values - these values can then be used inside of Artillery
* A request is made to start the w3wp process
* A request is made to cleanup any old test data (the endpoint is based on Umbraco.TestData)
* The list of artillery scenarios are executed in order. Some collect counters, some do not. Before each scenario, a request is made to a custom endpoint (in Umbraco.TestData) to perform a GC.Collect call. For a scenario that collects counters, before the scenario is run, the script to collect counters in initialized. The script started is different for .net framework vs .net 5. Once the scenario is complete the counters are stopped and the stats are written to a csv file and the artillery output for that scenario is written to a json file.
* When all scenarios are complete, a NodeJs application is executed which will aggregate and format the data in the files and the reports are sent to CosmosDb.

The current scenarios and order is:
  * warmup - warms up the site from not being started by making requests to the home page
  * coldboot - makes a request to a custom endpoint (in Umbraco.TestData) to delete the lastsynced.txt file and restart the site. Then requests are made to the front-end which will be in a cold boot state.
  * warmboot - makes a request to a custom endpoint (in Umbraco.TestData) to restart the site. Then requests are made to the front-end which will be in a warm boot state.
  * login and load - makes requests for the login and load process for the back office
  * create doc type - makes requests to create doc types. The created doc type GUIDs are persisted in a file so they can be re-used in later tests.
  * create content - makes requests to create content based on the doc types created. The INT ids of the content created are saved to disk for later re-use.
  * update content - makes requests to update content based on the created content.
  * delete content - makes requests to delete content based on the created content.
  * front-end - performs throughput tests on the homepage to determine maximum requests per second throughput and latency.

## Known issues

`dotnet-counters` doesn't allow for automation currently due to it's requirement for console in "Q" key for closing and collecting the figures. See https://github.com/dotnet/diagnostics/issues/451#issuecomment-843650234. Due to this problem, when the script is run, it needs to be run in the context of a human and once it starts running, no window/UI interaction can be done so that the script hack to set active windows and send a "Q" key to the dotnet-counters process works.

Cold boot + Warm boot metrics are currently not calculated correctly for netcore. This is because when netcore restarts, IIS behaves differently than net framework and as such it will result in several 302 errors for a few seconds during restart which skews all figures. For reporting currently, netcore is excluded for Cold/Warm boot scenarios.
