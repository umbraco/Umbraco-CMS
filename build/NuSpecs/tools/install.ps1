param($rootPath, $toolsPath, $package, $project)

if ($project) {	
    $dateTime = Get-Date -Format yyyyMMdd-HHmmss
    $backupPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Data\NuGetBackup\$dateTime"
	
	# Create backup folder if it doesn't exist yet
	New-Item -ItemType Directory -Force -Path $backupPath
	
	# Create a backup of original web.config
	$projectDestinationPath = Split-Path $project.FullName -Parent
	$webConfigSource = Join-Path $projectDestinationPath "Web.config"
	Copy-Item $webConfigSource $backupPath -Force
	
	# Copy Web.config from package to project folder
	$umbracoFilesPath = Join-Path $rootPath "UmbracoFiles\Web.config"
	Copy-Item $umbracoFilesPath $projectDestinationPath -Force

	# Copy umbraco and umbraco_files from package to project folder
	# This is only done when these folders already exist because we 
	# only want to do this for upgrades
    $umbracoFolder = Join-Path $projectDestinationPath "Umbraco\"
	if(Test-Path $umbracoFolder) {
        $umbracoFolderSource = Join-Path $rootPath "UmbracoFiles\Umbraco"
		Copy-Item $umbracoFolder $backupPath -Force
        robocopy $umbracoFolderSource $umbracoFolder /e /xf UI.xml
    }

    $umbracoClientFolder = Join-Path $projectDestinationPath "Umbraco_Client"	
    if(Test-Path $umbracoClientFolder) {
        $umbracoClientFolderSource = Join-Path $rootPath "UmbracoFiles\Umbraco_Client"
		Copy-Item $umbracoClientFolder $backupPath -Force
        robocopy $umbracoFolderSource $umbracoClientFolder /e
    }

	# Open readme.txt file
	$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
}