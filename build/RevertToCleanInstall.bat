@ECHO OFF

:choice
set /P c=WARNING! Are you sure you want to continue, this will remove all package files, view files, sqlce database, etc... Press 'Y' to auto-remove all files/folders, 'N' to cancel or 'C' to prompt for each folder removal?
if /I "%c%" EQU "C" goto :prompt
if /I "%c%" EQU "Y" goto :auto
if /I "%c%" EQU "N" goto :exit
goto :choice


:prompt

echo Current folder: %CD%

echo Removing sqlce database
del ..\src\Umbraco.Web.UI\App_Data\Umbraco.sdf

echo Resetting installedPackages.config
echo ^<?xml version="1.0" encoding="utf-8"?^>^<packages^>^</packages^> >..\src\Umbraco.Web.UI\App_Data\packages\installed\installedPackages.config

echo Removing plugin cache files
del ..\src\Umbraco.Web.UI\App_Data\TEMP\PluginCache\*.*

echo Removing log files
del ..\src\Umbraco.Web.UI\App_Data\Logs\*.*

echo Removing xslt files
del ..\src\Umbraco.Web.UI\Xslt\*.*

echo Removing user control files
del ..\src\Umbraco.Web.UI\UserControls\*.*

echo Removing view files
del ..\src\Umbraco.Web.UI\Views\*.*

echo Removing razor files
del ..\src\Umbraco.Web.UI\MacroScripts\*.*

echo Removing media files
del ..\src\Umbraco.Web.UI\Media\*.*

echo Removing script files
del ..\src\Umbraco.Web.UI\Scripts\*.*

echo Removing css files
del ..\src\Umbraco.Web.UI\Css\*.*

echo "Umbraco install reverted to clean install"
pause 
exit



:auto

echo Current folder: %CD%

echo Removing sqlce database
del ..\src\Umbraco.Web.UI\App_Data\Umbraco.sdf

echo Resetting installedPackages.config
echo ^<?xml version="1.0" encoding="utf-8"?^>^<packages^>^</packages^> >..\src\Umbraco.Web.UI\App_Data\packages\installed\installedPackages.config

echo Removing plugin cache files
FOR %%A IN (..\src\Umbraco.Web.UI\App_Data\TEMP\PluginCache\*.*) DO DEL %%A

echo Removing log files
FOR %%A IN (..\src\Umbraco.Web.UI\App_Data\Logs\*.*) DO DEL %%A

echo Removing xslt files
FOR %%A IN (..\src\Umbraco.Web.UI\Xslt\*.*) DO DEL %%A

echo Removing user control files
FOR %%A IN (..\src\Umbraco.Web.UI\UserControls\*.*) DO DEL %%A

echo Removing view files
FOR %%A IN (..\src\Umbraco.Web.UI\Views\*.*) DO DEL %%A

echo Removing razor files
FOR %%A IN (..\src\Umbraco.Web.UI\MacroScripts\*.*) DO DEL %%A

echo Removing media files
FOR %%A IN (..\src\Umbraco.Web.UI\Media\*.*) DO DEL %%A

echo Removing script files
FOR %%A IN (..\src\Umbraco.Web.UI\Scripts\*.*) DO DEL %%A

echo Removing css files
FOR %%A IN (..\src\Umbraco.Web.UI\Css\*.*) DO DEL %%A

echo "Umbraco install reverted to clean install"
pause 
exit



:exit
exit