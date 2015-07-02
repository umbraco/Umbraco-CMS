param($rootPath, $toolsPath, $package, $project)

if ($project) {
	$dateTime = Get-Date -Format yyyyMMdd-HHmmss
	$projectDestinationPath = $project.Properties.Item("FullPath").Value
	$backupPath = Join-Path $projectDestinationPath "\App_Data\NuGetBackup\$dateTime"
	$copyLogsPath = Join-Path $backupPath "CopyLogs" 

	# Create backup folder and logs folder if it doesn't exist yet
	New-Item -ItemType Directory -Force -Path $backupPath
	New-Item -ItemType Directory -Force -Path $copyLogsPath
	
	# Create a backup of original web.config
	$webConfigSource = Join-Path $projectDestinationPath "Web.config"
	Copy-Item $webConfigSource $backupPath -Force
	
	# Backup config files folder
	$configFolder = Join-Path $projectDestinationPath "Config"
	if(Test-Path $configFolder) {
		$umbracoBackupPath = Join-Path $backupPath "Config"
		New-Item -ItemType Directory -Force -Path $umbracoBackupPath
		
		robocopy $configFolder $umbracoBackupPath /e /LOG:$copyLogsPath\ConfigBackup.log
	}
	
	# Check to see if this is a new install or not
	$newInstall = $true
	$destinationWebConfig = Join-Path $projectDestinationPath "Web.config"

	if(Test-Path $destinationWebConfig) 
	{
		Try 
		{
			[xml]$config = Get-Content $destinationWebConfig
			
			$config.configuration.appSettings.ChildNodes | ForEach-Object { 
				if($_.key -eq "umbracoConfigurationStatus") 
				{
					# The web.config has an umbraco-specific appSetting in it
					# so must be an upgrade rather than a clean install
					$newInstall = $false 
				}
			}
		} 
		Catch { }
	}

	# Copy umbraco and umbraco_files from package to project folder
	# This is only done when these folders already exist because we 
	# only want to do this for upgrades
	$umbracoFolder = Join-Path $projectDestinationPath "Umbraco"
	if(Test-Path $umbracoFolder) {
		$umbracoFolderSource = Join-Path $rootPath "UmbracoFiles\Umbraco"
		
		$umbracoBackupPath = Join-Path $backupPath "Umbraco"
		New-Item -ItemType Directory -Force -Path $umbracoBackupPath
		
		robocopy $umbracoFolder $umbracoBackupPath /e /LOG:$copyLogsPath\UmbracoBackup.log
		robocopy $umbracoFolderSource $umbracoFolder /is /it /e /xf UI.xml /LOG:$copyLogsPath\UmbracoCopy.log
	}

	$umbracoClientFolder = Join-Path $projectDestinationPath "Umbraco_Client"	
	if(Test-Path $umbracoClientFolder) {
		$umbracoClientFolderSource = Join-Path $rootPath "UmbracoFiles\Umbraco_Client"
		
		$umbracoClientBackupPath = Join-Path $backupPath "Umbraco_Client"
		New-Item -ItemType Directory -Force -Path $umbracoClientBackupPath
		
		robocopy $umbracoClientFolder $umbracoClientBackupPath /e /LOG:$copyLogsPath\UmbracoClientBackup.log
		robocopy $umbracoClientFolderSource $umbracoClientFolder /is /it /e /LOG:$copyLogsPath\UmbracoClientCopy.log		
	}

	if($newInstall -eq $true) 
	{
		$packageWebConfigSource = Join-Path $rootPath "UmbracoFiles\Web.config"
		Copy-Item $packageWebConfigSource $destinationWebConfig -Force
	} 

	# Always remove the install folder if one exists
	$installFolder = Join-Path $projectDestinationPath "Install"
	if(Test-Path $installFolder) {
		Remove-Item $installFolder -Force -Recurse -Confirm:$false
	}
	
    # Setup umbraco files for new installs
    if($newInstall -eq $true) {
    	if($project.Object -is "VSWebSite.VSWebSite") {

    		# Website project, so copy umbraco files manually

    		$umbracoFilesSource = Join-Path $rootPath "UmbracoFiles"

    		robocopy $umbracoFilesSource $projectDestinationPath /e /xf $umbracoFilesSource\web.config /LOG:$copyLogsPath\UmbracoFilesBackup.log

    	}
    	else
    	{

    		# Web application project so inject msbuild props/target

    		# Get props / target paths
		    $propsFile = [System.IO.Path]::Combine($toolsPath, '..\msbuild\UmbracoCms.props')
		    $targetsFile = [System.IO.Path]::Combine($toolsPath, '..\msbuild\UmbracoCms.targets')
		 
		    # Need to load MSBuild assembly if it's not loaded yet.
		    Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

		    # Grab the loaded MSBuild project for the project
		    $msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1
		 
		    # Make paths relative to project folder.
		    $projectUri = new-object Uri($project.FullName, [System.UriKind]::Absolute)

		    $propsUri = new-object Uri($propsFile, [System.UriKind]::Absolute)
		    $propsRelPath = [System.Uri]::UnescapeDataString($projectUri.MakeRelativeUri($propsUri).ToString()).Replace([System.IO.Path]::AltDirectorySeparatorChar, [System.IO.Path]::DirectorySeparatorChar)

		    $targetUri = new-object Uri($targetsFile, [System.UriKind]::Absolute)
		    $targetRelPath = [System.Uri]::UnescapeDataString($projectUri.MakeRelativeUri($targetUri).ToString()).Replace([System.IO.Path]::AltDirectorySeparatorChar, [System.IO.Path]::DirectorySeparatorChar)
		 
		    # Add the import with a condition, to allow the project to load without the files being present.
		    $propsImport = $msbuild.Xml.AddImport($propsRelPath)
		    $propsImport.Condition = "Exists('$propsRelPath')"

		    $targetImport = $msbuild.Xml.AddImport($targetRelPath)
		    $targetImport.Condition = "Exists('$targetRelPath')"

		    $project.Save()

    	}
    }

	# Open appropriate readme
	if($newInstall -eq $true)  
	{
		$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
	} 
	else 
	{	
		$DTE.ItemOperations.OpenFile($toolsPath + '\ReadmeUpgrade.txt')
	}
}