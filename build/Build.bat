@ECHO OFF
IF NOT EXIST UmbracoVersion.txt (
	ECHO UmbracoVersion.txt missing!
	GOTO :showerror
)

REM Get the version and comment from UmbracoVersion.txt lines 2 and 3
SET "release="
SET "comment="
FOR /F "skip=1 delims=" %%i IN (UmbracoVersion.txt) DO IF NOT DEFINED release SET "release=%%i"
FOR /F "skip=2 delims=" %%i IN (UmbracoVersion.txt) DO IF NOT DEFINED comment SET "comment=%%i"

REM If there's arguments on the command line overrule UmbracoVersion.txt and use that as the version
IF [%2] NEQ [] (SET release=%2)
IF [%3] NEQ [] (SET comment=%3) ELSE (IF [%2] NEQ [] (SET "comment="))

REM Get the "is continuous integration" from the parameters
SET "isci=0"
IF [%1] NEQ [] (SET isci=1)

SET version=%release%
IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)

ECHO.
ECHO Building Umbraco %version%
ECHO.

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %release%

ECHO.
ECHO Removing the belle build folder and bower_components folder to make sure everything is clean as a whistle
RD ..\src\Umbraco.Web.UI.Client\build /Q /S
RD ..\src\Umbraco.Web.UI.Client\bower_components /Q /S

ECHO.
ECHO Removing existing built files to make sure everything is clean as a whistle
RMDIR /Q /S _BuildOutput
DEL /F /Q UmbracoCms.*.zip
DEL /F /Q UmbracoExamine.*.zip
DEL /F /Q UmbracoCms.*.nupkg
DEL /F /Q webpihash.txt

ECHO.
ECHO Making sure Git is in the path so that the build can succeed
CALL InstallGit.cmd

REM Adding the default Git path so that if it's installed it can actually be found
REM This is necessary because SETLOCAL is on in InstallGit.cmd so that one might find Git, 
REM but the path setting is lost due to SETLOCAL 
path=C:\Program Files (x86)\Git\cmd;C:\Program Files\Git\cmd;%PATH%

ECHO.
ECHO Making sure we have a web.config
IF NOT EXIST %CD%\..\src\Umbraco.Web.UI\web.config COPY %CD%\..\src\Umbraco.Web.UI\web.Template.config %CD%\..\src\Umbraco.Web.UI\web.config

ECHO.
ECHO Restoring NuGet packages
SET nuGetFolder=%CD%\..\src\packages\
..\src\.nuget\NuGet.exe restore ..\src\Umbraco.Core\project.json -OutputDirectory %nuGetFolder% -Verbosity quiet
..\src\.nuget\NuGet.exe restore ..\src\umbraco.datalayer\packages.config -OutputDirectory %nuGetFolder% -Verbosity quiet
..\src\.nuget\NuGet.exe restore ..\src\Umbraco.Web\project.json -OutputDirectory %nuGetFolder% -Verbosity quiet
..\src\.nuget\NuGet.exe restore ..\src\Umbraco.Web.UI\packages.config -OutputDirectory %nuGetFolder% -Verbosity quiet

ECHO.
ECHO Performing MSBuild and producing Umbraco binaries zip files
ECHO This takes a few minutes and logging is set to report warnings
ECHO and errors only so it might seems like nothing is happening for a while. 
ECHO You can check the msbuild.log file for progress.
ECHO.
"%ProgramFiles(x86)%"\MSBuild\14.0\Bin\MSBuild.exe "Build.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment% /p:NugetPackagesDirectory=%nuGetFolder% /consoleloggerparameters:Summary;ErrorsOnly;WarningsOnly /fileLogger
IF ERRORLEVEL 1 GOTO :error

ECHO.
ECHO Setting node_modules folder to hidden to prevent VS13 from crashing on it while loading the websites project
attrib +h ..\src\Umbraco.Web.UI.Client\node_modules

ECHO.
ECHO Adding Web.config transform files to the NuGet package
REN .\_BuildOutput\WebApp\Views\Web.config Web.config.transform
REN .\_BuildOutput\WebApp\Xslt\Web.config Web.config.transform

ECHO.
ECHO Packing the NuGet release files
..\src\.nuget\NuGet.exe Pack NuSpecs\UmbracoCms.Core.nuspec -Version %version% -Symbols -Verbosity quiet
..\src\.nuget\NuGet.exe Pack NuSpecs\UmbracoCms.nuspec -Version %version% -Verbosity quiet
IF ERRORLEVEL 1 GOTO :error

:success
ECHO.
ECHO No errors were detected!
ECHO There may still be some in the output, which you would need to investigate.
ECHO Warnings are usually normal.
GOTO :EOF

:error

ECHO.
ECHO. Errors were detected!

REM don't pause if continuous integration else the build server waits forever
REM before cancelling the build (and, there is noone to read the output anyways)
IF isci NEQ 1 PAUSE
