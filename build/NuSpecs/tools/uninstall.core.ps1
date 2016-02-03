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
	$umbracoBinFolder = Join-Path $projectPath "bin"
	Write-Host "umbracoBinFolder:" "${umbracoBinFolder}"

	# Remove backups
	Write-Host "removing backups:" "${backupPath}"
	if(Test-Path $backupPath) { Remove-Item -Recurse -Force $backupPath -Confirm:$false }
	
	# Delete files Umbraco ships with	

	Write-Host "removing dlls:" "${umbracoBinFolder}"
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
		
	$amd64Folder = Join-Path $umbracoBinFolder "amd64"
	if(Test-Path $amd64Folder) { Remove-Item $amd64Folder -Force -Recurse -Confirm:$false }
	$x86Folder = Join-Path $umbracoBinFolder "x86"
	if(Test-Path $x86Folder) { Remove-Item $x86Folder -Force -Recurse -Confirm:$false }		
}