@ECHO OFF

:choice
set /P c=WARNING! Are you sure you want to continue, this will remove all package files, view files, sqlce database, etc... Press 'Y' to auto-remove all files/folders, 'N' to cancel or 'C' to prompt for each folder removal?
if /I "%c%" EQU "C" goto :prompt
if /I "%c%" EQU "Y" goto :auto
if /I "%c%" EQU "N" goto :exit
goto :choice


:prompt

echo Current folder: %CD%

echo Regenerating SQL CE database
SET buildfolder=%CD%
CD ..\tools\RegenerateUmbracoSQLCEDatabase\
RegenerateUmbracoSQLCEDatabase.exe %CD%\..\..\src\Umbraco.Web.UI
CD %buildfolder%

echo Removing bin files
del ..\src\Umbraco.Web.UI\bin\*.*

echo Building solution
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe ..\src\umbraco.sln /t:Clean,Build

echo Resetting installedPackages.config
echo ^<?xml version="1.0" encoding="utf-8"?^>^<packages^>^</packages^> >..\src\Umbraco.Web.UI\App_Data\packages\installed\installedPackages.config

echo Removing plugin cache files
del ..\src\Umbraco.Web.UI\App_Data\TEMP\PluginCache\*.*

echo Removing cache files and examine index
del ..\src\Umbraco.Web.UI\App_Data\TEMP\*.*

echo Removing log files
del ..\src\Umbraco.Web.UI\App_Data\Logs\*.*

echo Removing packages
del ..\src\Umbraco.Web.UI\App_Data\packages\*.*

echo Removing previews
del ..\src\Umbraco.Web.UI\App_Data\preview\*.*

echo Removing app code files (typically added by starterkits)
del ..\src\Umbraco.Web.UI\App_Code\*.*

echo Removing xslt files
del ..\src\Umbraco.Web.UI\xslt\*.*

echo Removing user control files
del ..\src\Umbraco.Web.UI\UserControls\*.*

echo Removing masterpage files
del ..\src\Umbraco.Web.UI\masterpages\*.*

echo Removing razor files
del ..\src\Umbraco.Web.UI\macroScripts\*.*

echo Removing media files
del ..\src\Umbraco.Web.UI\media\*.*

echo Removing script files
del ..\src\Umbraco.Web.UI\scripts\*.*

echo Removing css files
del ..\src\Umbraco.Web.UI\css\*.*

echo "Umbraco install reverted to clean install"
pause 
exit



:auto

echo Current folder: %CD%

echo Regenerating SQL CE database
SET buildfolder=%CD%
CD ..\tools\RegenerateUmbracoSQLCEDatabase\
RegenerateUmbracoSQLCEDatabase.exe %CD%\..\..\src\Umbraco.Web.UI
CD %buildfolder%

echo Removing bin files
 FOR %%A IN (..\src\Umbraco.Web.UI\bin\*.*) DO DEL %%A

echo Building solution
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe ..\src\umbraco.sln /t:Clean,Build

echo Resetting installedPackages.config
echo ^<?xml version="1.0" encoding="utf-8"?^>^<packages^>^</packages^> >..\src\Umbraco.Web.UI\App_Data\packages\installed\installedPackages.config

echo Removing plugin cache files
FOR %%A IN (..\src\Umbraco.Web.UI\App_Data\TEMP\PluginCache\*.*) DO DEL %%A

echo Removing cache files and examine index
FOR %%A IN (..\src\Umbraco.Web.UI\App_Data\TEMP\*.*) DO DEL %%A

echo Removing log files
FOR %%A IN (..\src\Umbraco.Web.UI\App_Data\Logs\*.*) DO DEL %%A

echo Removing packages
FOR %%A IN (..\src\Umbraco.Web.UI\App_Data\packages\*.*) DO DEL %%A

echo Removing previews
FOR %%A IN (..\src\Umbraco.Web.UI\App_Data\preview\*.*) DO DEL %%A

echo Removing app code files (typically added by starterkits)
FOR %%A IN (..\src\Umbraco.Web.UI\App_Code\*.*) DO DEL %%A

echo Removing xslt files
FOR %%A IN (..\src\Umbraco.Web.UI\xslt\*.*) DO DEL %%A

echo Removing masterpage files
FOR %%A IN (..\src\Umbraco.Web.UI\masterpages\*.*) DO DEL %%A

echo Removing user control files
FOR %%A IN (..\src\Umbraco.Web.UI\usercontrols\*.*) DO DEL %%A

echo Removing view files
ATTRIB +H ..\src\Umbraco.Web.UI\Views\Partials\Grid\*.cshtml /S
FOR %%A IN (..\src\Umbraco.Web.UI\Views\) DO DEL /Q /S *.cshtml -H
ATTRIB -H ..\src\Umbraco.Web.UI\Views\Partials\Grid\*.cshtml /S

echo Removing razor files
FOR %%A IN (..\src\Umbraco.Web.UI\macroScripts\*.*) DO DEL %%A

echo Removing media files
FOR %%A IN (..\src\Umbraco.Web.UI\media\*.*) DO DEL %%A

echo Removing script files
FOR %%A IN (..\src\Umbraco.Web.UI\scripts\*.*) DO DEL %%A

echo Removing css files
FOR %%A IN (..\src\Umbraco.Web.UI\css\*.*) DO DEL %%A

echo Removing Courier files
del ..\src\Umbraco.Web.UI\config\courier.config
del ..\src\Umbraco.Web.UI\umbraco\images\tray\courier.jpg
rmdir "..\src\Umbraco.Web.UI\umbraco\plugins\courier\" /S /Q

echo Removing Contour files
del ..\src\Umbraco.Web.UI\umbraco\images\tray\contour.png
FOR %%A IN (..\src\Umbraco.Web.UI\umbraco\images\umbraco\icon_*.*) DO DEL %%A
rmdir "..\src\Umbraco.Web.UI\umbraco\plugins\umbracoContour\" /S /Q
del ..\src\Umbraco.Web.UI\umbraco\xslt\templates\UmbracoContour*.* /S /Q
rmdir "..\src\Umbraco.Web.UI\usercontrols\umbracoContour\" /S /Q

echo Start with a clean web.config
copy ..\src\Umbraco.Web.UI\web.Template.config ..\src\Umbraco.Web.UI\web.config /Y

echo Start with a clean web.config
copy ..\src\Umbraco.Web.UI\web.Template.config ..\src\Umbraco.Web.UI\web.config /Y

echo "Umbraco install reverted to clean install"
pause 
exit



:exit
exit