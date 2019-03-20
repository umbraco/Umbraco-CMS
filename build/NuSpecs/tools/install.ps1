param($installPath, $toolsPath, $package, $project)

Write-Host "installPath:" "${installPath}"
Write-Host "toolsPath:" "${toolsPath}"

Write-Host " "

if ($project) {
	$dateTime = Get-Date -Format yyyyMMdd-HHmmss

	# Create paths and list them
	$projectPath = (Get-Item $project.Properties.Item("FullPath").Value).FullName
	Write-Host "projectPath:" "${projectPath}"
	$webConfigSource = Join-Path $projectPath "Web.config"
	Write-Host "webConfigSource:" "${webConfigSource}"
	$configFolder = Join-Path $projectPath "Config"
	Write-Host "configFolder:" "${configFolder}"

	# Copy umbraco and umbraco_files from package to project folder
	$umbracoFolder = Join-Path $projectPath "Umbraco"
	New-Item -ItemType Directory -Force -Path $umbracoFolder
	$umbracoFolderSource = Join-Path $installPath "UmbracoFiles\Umbraco"		
	robocopy $umbracoFolderSource $umbracoFolder /is /it /e /xf UI.xml /LOG:$copyLogsPath\UmbracoCopy.log

	$copyWebconfig = $true
	$destinationWebConfig = Join-Path $projectPath "Web.config"

	if(Test-Path $destinationWebConfig) 
	{
		Try 
		{
			[xml]$config = Get-Content $destinationWebConfig
			
			$config.configuration.appSettings.ChildNodes | ForEach-Object { 
				if($_.key -eq "Umbraco.Core.ConfigurationStatus") 
				{
					# The web.config has an umbraco-specific appSetting in it
					# don't overwrite it and let config transforms do their thing
					$copyWebconfig = $false 
				}
			}
		} 
		Catch { }
	}
	
	if($copyWebconfig -eq $true) 
	{
		$packageWebConfigSource = Join-Path $installPath "UmbracoFiles\Web.config"
		Copy-Item $packageWebConfigSource $destinationWebConfig -Force

		# Copy files that don't get automatically copied for Website projects
		# We do this here, when copyWebconfig is true because we only want to do it for new installs
		# If this is an upgrade then the files should already be there
		$splashesSource = Join-Path $installPath "UmbracoFiles\Config\splashes\*.*"
		$splashesDestination = Join-Path $projectPath "Config\splashes\"
		New-Item $splashesDestination -Type directory
		Copy-Item $splashesSource $splashesDestination -Force
	} else {
		# This part only runs for upgrades
	
		$upgradeViewSource = Join-Path $umbracoFolderSource "Views\install\*"
		$upgradeView = Join-Path $umbracoFolder "Views\install\"
		Write-Host "Copying2 ${upgradeViewSource} to ${upgradeView}"
		Copy-Item $upgradeViewSource $upgradeView -Force
		
		Try 
		{
			# Disable tours for upgrades, presumably Umbraco experience is already available
			$umbracoSettingsConfigPath = Join-Path $configFolder "umbracoSettings.config"
			$content = (Get-Content $umbracoSettingsConfigPath).Replace('<tours enable="true">','<tours enable="false">')
			# Saves with UTF-8 encoding without BOM which makes sure Umbraco can still read it
			# Reference: https://stackoverflow.com/a/32951824/5018
			[IO.File]::WriteAllLines($umbracoSettingsConfigPath, $content)
		} 
		Catch 
		{
			# Not a big problem if this fails, let it go
		}
	}
	
	# Open appropriate readme
	if($copyWebconfig -eq $true)  
	{
		$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
	} 
	else 
	{	
		$DTE.ItemOperations.OpenFile($toolsPath + '\ReadmeUpgrade.txt')
	}
}
