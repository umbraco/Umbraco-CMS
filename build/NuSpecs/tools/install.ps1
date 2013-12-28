param($rootPath, $toolsPath, $package, $project)

if ($project) {	
	# Create a backup of extisting umbraco config files
	$configPath = Join-Path ($project.Properties.Item("FullPath").Value) "\config"
    $backupPath = Join-Path $configPath "\backup"
	Get-ChildItem -path $configPath |
	   Where -filterscript {($_.Name.EndsWith("config"))} | Foreach-Object {
	    $newFileName = Join-Path $backupPath $_.Name.replace(".config",".config.backup")
        New-Item -ItemType File -Path $newFileName -Force
	    Copy-Item $_.FullName $newFileName -Force
	   }
		
	# Create a backup of original web.config
	$projectDestinationPath = $project.Properties.Item("FullPath").Value
	$webConfigSource = Join-Path $projectDestinationPath "web.config"
	$webConfigDestination = Join-Path $projectDestinationPath "web.config.backup"
	Copy-Item $webConfigSource $webConfigDestination
	
	# Copy umbraco files from package to project folder
	$projectDestinationPath = $project.Properties.Item("FullPath").Value
	$umbracoFilesPath = Join-Path $rootPath "UmbracoFiles\*"
	Copy-Item $umbracoFilesPath $projectDestinationPath -recurse -force
	
	# Open readme.txt file
	$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
}