param($rootPath, $toolsPath, $package, $project)

if ($project) {	
	# Copy umbraco files from package to project folder
	$projectDestinationPath = Split-Path $project.FullName -Parent
	$umbracoFilesPath = Join-Path $rootPath "UmbracoFiles\*"
	Copy-Item $umbracoFilesPath $projectDestinationPath -recurse -force
	
	# Open readme.txt file
	$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
}