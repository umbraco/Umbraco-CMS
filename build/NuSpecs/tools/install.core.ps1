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
	$umbracoBinFolder = Join-Path $projectPath "bin"
	Write-Host "umbracoBinFolder:" "${umbracoBinFolder}"		

	# Create backup folder and logs folder if it doesn't exist yet
	New-Item -ItemType Directory -Force -Path $backupPath
	New-Item -ItemType Directory -Force -Path $copyLogsPath
	
	# After backing up, remove all umbraco dlls from bin folder in case dll files are included in the VS project
	# See: http://issues.umbraco.org/issue/U4-4930
	
	if(Test-Path $umbracoBinFolder) {
		$umbracoBinBackupPath = Join-Path $backupPath "bin"
		
		New-Item -ItemType Directory -Force -Path $umbracoBinBackupPath
		
		robocopy $umbracoBinFolder $umbracoBinBackupPath /e /LOG:$copyLogsPath\UmbracoBinBackup.log
		
		# Delete files Umbraco ships with
		if(Test-Path $umbracoBinFolder\businesslogic.dll) { Remove-Item $umbracoBinFolder\businesslogic.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\cms.dll) { Remove-Item $umbracoBinFolder\cms.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\controls.dll) { Remove-Item $umbracoBinFolder\controls.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\interfaces.dll) { Remove-Item $umbracoBinFolder\interfaces.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\log4net.dll) { Remove-Item $umbracoBinFolder\log4net.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Microsoft.ApplicationBlocks.Data.dll) { Remove-Item $umbracoBinFolder\Microsoft.ApplicationBlocks.Data.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\SQLCE4Umbraco.dll) { Remove-Item $umbracoBinFolder\SQLCE4Umbraco.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Data.SqlServerCe.dll) { Remove-Item $umbracoBinFolder\System.Data.SqlServerCe.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Data.SqlServerCe.Entity.dll) { Remove-Item $umbracoBinFolder\System.Data.SqlServerCe.Entity.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\TidyNet.dll) { Remove-Item $umbracoBinFolder\TidyNet.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\umbraco.dll) { Remove-Item $umbracoBinFolder\umbraco.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Umbraco.Core.dll) { Remove-Item $umbracoBinFolder\Umbraco.Core.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\umbraco.DataLayer.dll) { Remove-Item $umbracoBinFolder\umbraco.DataLayer.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\umbraco.editorControls.dll) { Remove-Item $umbracoBinFolder\umbraco.editorControls.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\umbraco.MacroEngines.dll) { Remove-Item $umbracoBinFolder\umbraco.MacroEngines.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\umbraco.providers.dll) { Remove-Item $umbracoBinFolder\umbraco.providers.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Umbraco.Web.UI.dll) { Remove-Item $umbracoBinFolder\Umbraco.Web.UI.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\UmbracoExamine.dll) { Remove-Item $umbracoBinFolder\UmbracoExamine.dll -Force -Confirm:$false }
		
		$amd64Folder = Join-Path $projectPath "bin\amd64"
		if(Test-Path $amd64Folder) { Remove-Item $amd64Folder -Force -Recurse -Confirm:$false }
		$x86Folder = Join-Path $projectPath "bin\x86"
		if(Test-Path $x86Folder) { Remove-Item $x86Folder -Force -Recurse -Confirm:$false }		
		
	}
}