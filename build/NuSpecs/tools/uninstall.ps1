param($installPath, $toolsPath, $package, $project)

Write-Host "installPath:" "${installPath}"
Write-Host "toolsPath:" "${toolsPath}"

Write-Host " "

if ($project) {
	
	# Create paths and list them
	$projectPath = (Get-Item $project.Properties.Item("FullPath").Value).FullName
	Write-Host "projectPath:" "${projectPath}"
	$backupPath = Join-Path $projectPath "App_Data\NuGetBackup"
	Write-Host "backupPath:" "${backupPath}"
	$appBrowsers = Join-Path $projectPath "App_Browsers"
	Write-Host "appBrowsers:" "${appBrowsers}"
	$appData = Join-Path $projectPath "App_Data"
	Write-Host "appData:" "${appData}"
	
	# Remove backups
	Write-Host "removing backups:" "${backupPath}"
	if(Test-Path $backupPath) { Remove-Item -Recurse -Force $backupPath -Confirm:$false }	

	# Remove app_data files
	Write-Host "removing app_data files:" "${appData}"
	if(Test-Path $appData\packages) { Remove-Item $appData\packages -Recurse -Force -Confirm:$false }

	Write-Host "removing app_browsers:" "${appBrowsers}"
	if(Test-Path $appBrowsers\Form.browser) { Remove-Item $appBrowsers\Form.browser -Force -Confirm:$false }
	if(Test-Path $appBrowsers\w3cvalidator.browser) { Remove-Item $appBrowsers\w3cvalidator.browser -Force -Confirm:$false }

	# Remove umbraco and umbraco_files	
	$umbracoFolder = Join-Path $projectPath "Umbraco"	
	Write-Host "removing umbraco folder:" "${umbracoFolder}"
	if(Test-Path $umbracoFolder) { Remove-Item $umbracoFolder -Recurse -Force -Confirm:$false }
	$umbracoClientFolder = Join-Path $projectPath "Umbraco_Client"	
	Write-Host "removing umbraco client folder:" "${umbracoClientFolder}"
	if(Test-Path $umbracoClientFolder) { Remove-Item $umbracoClientFolder -Recurse -Force -Confirm:$false }
}
