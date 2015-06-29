param($rootPath, $toolsPath, $package, $project)

if ($project) {
	$dateTime = Get-Date -Format yyyyMMdd-HHmmss
	$backupPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Data\NuGetBackup\$dateTime"
	$copyLogsPath = Join-Path $backupPath "CopyLogs"
	$projectDestinationPath = Split-Path $project.FullName -Parent

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
	
	$copyWebconfig = $false
	try		
	{
	  # SJ - What can I say: big up for James Newton King for teaching us a hack for detecting if this is a new install vs. an upgrade!
	  # https://github.com/JamesNK/Newtonsoft.Json/pull/387 - would never have seen this without the controversial pull request..
	  $dte2 = Get-Interface $dte ([EnvDTE80.DTE2])		
			
	  if ($dte2.ActiveWindow.Caption -eq "Package Manager Console")		
	  {		
		# user is installing from VS NuGet console		
		# get reference to the window, the console host and the input history		
		# copy web.config if "install-package UmbracoCms" was last input		
			
		$consoleWindow = $(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow])		
			
		$props = $consoleWindow.GetType().GetProperties([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)		
			
		$prop = $props | ? { $_.Name -eq "ActiveHostInfo" } | select -first 1		
		if ($prop -eq $null) { return }		
			
		$hostInfo = $prop.GetValue($consoleWindow)		
		if ($hostInfo -eq $null) { return }		
			
		$history = $hostInfo.WpfConsole.InputHistory.History		
			
		$lastCommand = $history | select -last 1		
			
		if ($lastCommand)		
		{		
		  $lastCommand = $lastCommand.Trim().ToLower()		
		  if ($lastCommand.StartsWith("install-package") -and $lastCommand.Contains("umbracocms"))		
		  {		
			$copyWebconfig = $true
		  }
		}		
	  }		
	  else		
	  {		
		# user is installing from VS NuGet dialog		
		# get reference to the window, then smart output console provider		
		# copy web.config if messages in buffered console contains "installing...UmbracoCms" in last operation		
		
		$instanceField = [NuGet.Dialog.PackageManagerWindow].GetField("CurrentInstance", [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::NonPublic)		
		$consoleField = [NuGet.Dialog.PackageManagerWindow].GetField("_smartOutputConsoleProvider", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)		
		if ($instanceField -eq $null -or $consoleField -eq $null) { return }		
			
		$instance = $instanceField.GetValue($null)		
		if ($instance -eq $null) { return }		
			
		$consoleProvider = $consoleField.GetValue($instance)		
		if ($consoleProvider -eq $null) { return }		
			
		$console = $consoleProvider.CreateOutputConsole($false)		
			
		$messagesField = $console.GetType().GetField("_messages", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)		
		if ($messagesField -eq $null) { return }		
			
		$messages = $messagesField.GetValue($console)		
		if ($messages -eq $null) { return }		
			
		$operations = $messages -split "=============================="		
			
		$lastOperation = $operations | select -last 1		
			
		if ($lastOperation)		
		{		
		  $lastOperation = $lastOperation.ToLower()		
			
		  $lines = $lastOperation -split "`r`n"		
			
		  $installMatch = $lines | ? { $_.StartsWith("------- installing...umbracocms ") } | select -first 1		
			
		  if ($installMatch)		
		  {		
			$copyWebconfig = $true
		  }		
		}		
	  }		
	}		
	catch		
	{		
	  # stop potential errors from bubbling up		
	  $ErrorMessage = $_.Exception.Message
	  $FailedItem = $_.Exception.ItemName
	  $installLogFile = Join-Path $projectDestinationPath "NuGetInstallError.log"
	  $text = "Error occurred: " + $ErrorMessage + " failure: " + $FailedItem
	  $text | Out-File $installLogFile
	}

	if($copyWebconfig -eq $true) 
	{
		$packageWebConfigSource = Join-Path $rootPath "UmbracoFiles\Web.config"
		$destinationWebConfig = Join-Path $projectDestinationPath "Web.config"
		Copy-Item $packageWebConfigSource $destinationWebConfig -Force
	}

	$installFolder = Join-Path $projectDestinationPath "Install"
	if(Test-Path $installFolder) {
		Remove-Item $installFolder -Force -Recurse -Confirm:$false
	}
	
	# Open readme.txt file
	$DTE.ItemOperations.OpenFile($toolsPath + '\Readme.txt')
}