param($rootPath, $toolsPath, $package, $project)

if ($project) {
	$dateTime = Get-Date -Format yyyyMMdd-HHmmss
	$backupPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Data\NuGetBackup\$dateTime"
	$copyLogsPath = Join-Path $backupPath "CopyLogs"
	$projectDestinationPath = Split-Path $project.FullName -Parent
	
	# Create backup folder and logs folder if it doesn't exist yet
	New-Item -ItemType Directory -Force -Path $backupPath
	New-Item -ItemType Directory -Force -Path $copyLogsPath
	
	# After backing up, remove all dlls from bin folder in case dll files are included in the VS project
	# See: http://issues.umbraco.org/issue/U4-4930
	$umbracoBinFolder = Join-Path $projectDestinationPath "bin"
	if(Test-Path $umbracoBinFolder) {
		$umbracoBinBackupPath = Join-Path $backupPath "bin"
		
		New-Item -ItemType Directory -Force -Path $umbracoBinBackupPath
		
		robocopy $umbracoBinFolder $umbracoBinBackupPath /e /LOG:$copyLogsPath\UmbracoBinBackup.log
		Remove-Item $umbracoBinFolder\*.dll -Force -Confirm:$false
	}
}