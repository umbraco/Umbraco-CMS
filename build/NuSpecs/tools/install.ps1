param($rootPath, $toolsPath, $package, $project)

if ($project) {	
    $dateTime = Get-Date -Format yyyyMMdd-HHmmss
    $backupPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Data\ConfigBackup\$dateTime"
	
	# Create backup folder if it doesn't exist yet
	New-Item -ItemType Directory -Force -Path $backupPath
	
	# Create a backup of original web.config
	$projectDestinationPath = Split-Path $project.FullName -Parent
	$webConfigSource = Join-Path $projectDestinationPath "Web.config"
	Copy-Item $webConfigSource $backupPath -Force
	
	# Copy Web.config from package to project folder
	$umbracoFilesPath = Join-Path $rootPath "UmbracoFiles\Web.config"
	Copy-Item $umbracoFilesPath $projectDestinationPath -Force
	
	# Open readme.txt file
	$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
}