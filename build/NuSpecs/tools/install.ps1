param($rootPath, $toolsPath, $package, $project)

if ($project) {	
	# Create a backup of extisting umbraco config files
	$configPath = Join-Path (Split-Path $project.FullName -Parent) "\config"
    $backupPath = Join-Path $configPath "\backup"
	Get-ChildItem -path $configPath |
	   Where -filterscript {($_.Name.EndsWith("config"))} | Foreach-Object {
	    $newFileName = Join-Path $backupPath $_.Name.replace(".config",".config.backup")
        New-Item -ItemType File -Path $newFileName -Force
	    Copy-Item $_.FullName $newFileName -Force
	   }
		
	# Create a backup of original web.config
	$projectDestinationPath = Split-Path $project.FullName -Parent
	$webConfigSource = Join-Path $projectDestinationPath "web.config"
	$webConfigDestination = Join-Path $projectDestinationPath "web.config.backup"
	Copy-Item $webConfigSource $webConfigDestination
	
	# Copy umbraco files from package to project folder
	$projectDestinationPath = Split-Path $project.FullName -Parent
	$umbracoFilesPath = Join-Path $rootPath "UmbracoFiles\*"
	Copy-Item $umbracoFilesPath $projectDestinationPath -recurse -force
	
	# Remove Newtonsoft.Json if present in the bin folder
	# If we don't do this, the correct version never gets copied in
	$projectDestinationPath = Split-Path $project.FullName -Parent
	$jsonDllFile = Join-Path $projectDestinationPath "bin\Newtonsoft.Json.dll"
	if (Test-Path $jsonDllFile) { Remove-Item $jsonDllFile -Confirm:$false }
	
	# Open readme.txt file
	$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
}