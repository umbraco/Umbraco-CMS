param($rootPath, $toolsPath, $package, $project)

if ($project) {
	$dateTime = Get-Date -Format yyyyMMdd-HHmmss
	$backupPath = Join-Path (Split-Path $project.FullName -Parent) "\App_Data\NuGetBackup\$dateTime"
	$copyLogsPath = Join-Path $backupPath "CopyLogs"
	$projectDestinationPath = Split-Path $project.FullName -Parent
	
	# Create backup folder and logs folder if it doesn't exist yet
	New-Item -ItemType Directory -Force -Path $backupPath
	New-Item -ItemType Directory -Force -Path $copyLogsPath
	
	# After backing up, remove all umbraco dlls from bin folder in case dll files are included in the VS project
	# See: http://issues.umbraco.org/issue/U4-4930
	$umbracoBinFolder = Join-Path $projectDestinationPath "bin"
	if(Test-Path $umbracoBinFolder) {
		$umbracoBinBackupPath = Join-Path $backupPath "bin"
		
		New-Item -ItemType Directory -Force -Path $umbracoBinBackupPath
		
		robocopy $umbracoBinFolder $umbracoBinBackupPath /e /LOG:$copyLogsPath\UmbracoBinBackup.log
		
		# Delete files Umbraco brings in
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
		if(Test-Path $umbracoBinFolder\UrlRewritingNet.UrlRewriter.dll) { Remove-Item $umbracoBinFolder\UrlRewritingNet.UrlRewriter.dll -Force -Confirm:$false }
		
		# Delete files Umbraco depends upon
		$amd64Folder = Join-Path $projectDestinationPath "bin\amd64"
		if(Test-Path $amd64Folder) { Remove-Item $amd64Folder -Force -Recurse -Confirm:$false }
		$x86Folder = Join-Path $projectDestinationPath "bin\x86"
		if(Test-Path $x86Folder) { Remove-Item $x86Folder -Force -Recurse -Confirm:$false }		
		if(Test-Path $umbracoBinFolder\AutoMapper.dll) { Remove-Item $umbracoBinFolder\AutoMapper.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\AutoMapper.Net4.dll) { Remove-Item $umbracoBinFolder\AutoMapper.Net4.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\ClientDependency.Core.dll) { Remove-Item $umbracoBinFolder\ClientDependency.Core.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\ClientDependency.Core.Mvc.dll) { Remove-Item $umbracoBinFolder\ClientDependency.Core.Mvc.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\CookComputing.XmlRpcV2.dll) { Remove-Item $umbracoBinFolder\CookComputing.XmlRpcV2.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Examine.dll) { Remove-Item $umbracoBinFolder\Examine.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\HtmlAgilityPack.dll) { Remove-Item $umbracoBinFolder\HtmlAgilityPack.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\ICSharpCode.SharpZipLib.dll) { Remove-Item $umbracoBinFolder\ICSharpCode.SharpZipLib.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\ImageProcessor.dll) { Remove-Item $umbracoBinFolder\ImageProcessor.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\ImageProcessor.Web.dll) { Remove-Item $umbracoBinFolder\ImageProcessor.Web.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Lucene.Net.dll) { Remove-Item $umbracoBinFolder\Lucene.Net.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Microsoft.Web.Infrastructure.dll) { Remove-Item $umbracoBinFolder\Microsoft.Web.Infrastructure.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Microsoft.Web.Helpers.dll) { Remove-Item $umbracoBinFolder\Microsoft.Web.Helpers.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Microsoft.Web.Mvc.FixedDisplayModes.dll) { Remove-Item $umbracoBinFolder\Microsoft.Web.Mvc.FixedDisplayModes.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\MiniProfiler.dll) { Remove-Item $umbracoBinFolder\MiniProfiler.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\MySql.Data.dll) { Remove-Item $umbracoBinFolder\MySql.Data.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\Newtonsoft.Json.dll) { Remove-Item $umbracoBinFolder\Newtonsoft.Json.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Net.Http.Formatting.dll) { Remove-Item $umbracoBinFolder\System.Net.Http.Formatting.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Web.Http.dll) { Remove-Item $umbracoBinFolder\System.Web.Http.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Web.Http.WebHost.dll) { Remove-Item $umbracoBinFolder\System.Web.Http.WebHost.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Web.Mvc.dll) { Remove-Item $umbracoBinFolder\System.Web.Mvc.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Web.Razor.dll) { Remove-Item $umbracoBinFolder\System.Web.Razor.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Web.WebPages.dll) { Remove-Item $umbracoBinFolder\System.Web.WebPages.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Web.WebPages.Deployment.dll) { Remove-Item $umbracoBinFolder\System.Web.WebPages.Deployment.dll -Force -Confirm:$false }
		if(Test-Path $umbracoBinFolder\System.Web.WebPages.Razor.dll) { Remove-Item $umbracoBinFolder\System.Web.WebPages.Razor.dll -Force -Confirm:$false }
	}
}