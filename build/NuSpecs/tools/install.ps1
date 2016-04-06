param($installPath, $toolsPath, $package, $project)

Write-Host "installPath:" "${installPath}"
Write-Host "toolsPath:" "${toolsPath}"

Write-Host " "

if ($project) {
	$dateTime = Get-Date -Format yyyyMMdd-HHmmss

	# Create paths and list them
	$projectPath = (Get-Item $project.Properties.Item("FullPath").Value).FullName
	Write-Host "projectPath:" "${projectPath}"
	$backupPath = Join-Path $projectPath "App_Data\NuGetBackup\$dateTime"
	Write-Host "backupPath:" "${backupPath}"
	$copyLogsPath = Join-Path $backupPath "CopyLogs"
	Write-Host "copyLogsPath:" "${copyLogsPath}"	
	$webConfigSource = Join-Path $projectPath "Web.config"
	Write-Host "webConfigSource:" "${webConfigSource}"
	$configFolder = Join-Path $projectPath "Config"
	Write-Host "configFolder:" "${configFolder}"

	# Create backup folder and logs folder if it doesn't exist yet
	New-Item -ItemType Directory -Force -Path $backupPath
	New-Item -ItemType Directory -Force -Path $copyLogsPath
	
	# Create a backup of original web.config
	Copy-Item $webConfigSource $backupPath -Force
	
	# Backup config files folder	
	if(Test-Path $configFolder) {
		$umbracoBackupPath = Join-Path $backupPath "Config"
		New-Item -ItemType Directory -Force -Path $umbracoBackupPath
		
		robocopy $configFolder $umbracoBackupPath /e /LOG:$copyLogsPath\ConfigBackup.log
	}
	
	# Copy umbraco and umbraco_files from package to project folder
	$umbracoFolder = Join-Path $projectPath "Umbraco"
	New-Item -ItemType Directory -Force -Path $umbracoFolder
	$umbracoFolderSource = Join-Path $installPath "UmbracoFiles\Umbraco"		
	$umbracoBackupPath = Join-Path $backupPath "Umbraco"
	New-Item -ItemType Directory -Force -Path $umbracoBackupPath		
	robocopy $umbracoFolder $umbracoBackupPath /e /LOG:$copyLogsPath\UmbracoBackup.log
	robocopy $umbracoFolderSource $umbracoFolder /is /it /e /xf UI.xml /LOG:$copyLogsPath\UmbracoCopy.log

	$umbracoClientFolder = Join-Path $projectPath "Umbraco_Client"	
	New-Item -ItemType Directory -Force -Path $umbracoClientFolder
	$umbracoClientFolderSource = Join-Path $installPath "UmbracoFiles\Umbraco_Client"		
	$umbracoClientBackupPath = Join-Path $backupPath "Umbraco_Client"
	New-Item -ItemType Directory -Force -Path $umbracoClientBackupPath		
	robocopy $umbracoClientFolder $umbracoClientBackupPath /e /LOG:$copyLogsPath\UmbracoClientBackup.log
	robocopy $umbracoClientFolderSource $umbracoClientFolder /is /it /e /LOG:$copyLogsPath\UmbracoClientCopy.log		

	$copyWebconfig = $true
	$destinationWebConfig = Join-Path $projectPath "Web.config"

	if(Test-Path $destinationWebConfig) 
	{
		Try 
		{
			[xml]$config = Get-Content $destinationWebConfig
			
			$config.configuration.appSettings.ChildNodes | ForEach-Object { 
				if($_.key -eq "umbracoConfigurationStatus") 
				{
					# The web.config has an umbraco-specific appSetting in it
					# don't overwrite it and let config transforms do their thing
					$copyWebconfig = $false 
				}
			}
		} 
		Catch { }
	}
	
	if($copyWebconfig -eq $true) 
	{
		$packageWebConfigSource = Join-Path $installPath "UmbracoFiles\Web.config"
		Copy-Item $packageWebConfigSource $destinationWebConfig -Force
	} 

	$installFolder = Join-Path $projectPath "Install"
	if(Test-Path $installFolder) {
		Remove-Item $installFolder -Force -Recurse -Confirm:$false
	}
	
	# Open appropriate readme
	if($copyWebconfig -eq $true)  
	{
		$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
	} 
	else 
	{	
		$DTE.ItemOperations.OpenFile($toolsPath + '\ReadmeUpgrade.txt')
	}
}