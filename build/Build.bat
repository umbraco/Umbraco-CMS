@ECHO OFF

:: UMBRACO BUILD FILE

:: ensure we have UmbracoVersion.txt
IF NOT EXIST UmbracoVersion.txt (
	ECHO UmbracoVersion.txt is missing!
	GOTO error
)

REM Get the version and comment from UmbracoVersion.txt lines 2 and 3
SET RELEASE=
SET COMMENT=
FOR /F "skip=1 delims=" %%i IN (UmbracoVersion.txt) DO IF NOT DEFINED RELEASE SET RELEASE=%%i
FOR /F "skip=2 delims=" %%i IN (UmbracoVersion.txt) DO IF NOT DEFINED COMMENT SET COMMENT=%%i

REM process args

SET INTEGRATION=0
SET nuGetFolder=%CD%\..\src\packages
SET SKIPNUGET=0

:processArgs

:: grab the first parameter as a whole eg "/action:start"
:: end if no more parameter
SET SWITCHPARSE=%1
IF [%SWITCHPARSE%] == [] goto endProcessArgs

:: get switch and value
SET SWITCH=
SET VALUE=
FOR /F "tokens=1,* delims=: " %%a IN ("%SWITCHPARSE%") DO SET SWITCH=%%a& SET VALUE=%%b

:: route arg
IF '%SWITCH%'=='/release' GOTO argRelease
IF '%SWITCH%'=='-release' GOTO argRelease
IF '%SWITCH%'=='/comment' GOTO argComment
IF '%SWITCH%'=='-comment' GOTO argComment
IF '%SWITCH%'=='/integration' GOTO argIntegration
IF '%SWITCH%'=='-integration' GOTO argIntegration
IF '%SWITCH%'=='/nugetfolder' GOTO argNugetFolder
IF '%SWITCH%'=='-nugetfolder' GOTO argNugetFolder
IF '%SWITCH%'=='/skipnuget' GOTO argSkipNuget
IF '%SWITCH%'=='-skipnuget' GOTO argSkipNuget
ECHO "Invalid switch %SWITCH%"
GOTO error

:: handle each arg

:argRelease
set RELEASE=%VALUE%
SHIFT
goto processArgs

:argComment
SET COMMENT=%VALUE%
SHIFT
GOTO processArgs

:argIntegration
SET INTEGRATION=1
SHIFT
GOTO processArgs

:argNugetFolder
SET nuGetFolder=%VALUE%
SHIFT
GOTO processArgs

:argSkipNuget
SET SKIPNUGET=1
SHIFT
GOTO processArgs

:endProcessArgs 

REM run

SET VERSION=%RELEASE%
IF [%COMMENT%] EQU [] (SET VERSION=%RELEASE%) ELSE (SET VERSION=%RELEASE%-%COMMENT%)

ECHO ################################################################
ECHO Building Umbraco %VERSION%
ECHO ################################################################

SET MSBUILDPATH=C:\Program Files (x86)\MSBuild\14.0\Bin
SET MSBUILD="%MSBUILDPATH%\MsBuild.exe"
SET PATH="%MSBUILDPATH%";%PATH%

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %RELEASE%

ECHO.
ECHO Removing the belle build folder and bower_components folder to make sure everything is clean as a whistle
RD ..\src\Umbraco.Web.UI.Client\build /Q /S
RD ..\src\Umbraco.Web.UI.Client\bower_components /Q /S

ECHO.
ECHO Removing existing built files to make sure everything is clean as a whistle
RMDIR /Q /S _BuildOutput
DEL /F /Q UmbracoCms.*.zip 2>NUL
DEL /F /Q UmbracoExamine.*.zip 2>NUL
DEL /F /Q UmbracoCms.*.nupkg 2>NUL
DEL /F /Q webpihash.txt 2>NUL

ECHO.
ECHO Making sure Git is in the path so that the build can succeed
CALL InstallGit.cmd

REM Adding the default Git path so that if it's installed it can actually be found
REM This is necessary because SETLOCAL is on in InstallGit.cmd so that one might find Git, 
REM but the path setting is lost due to SETLOCAL 
SET PATH="C:\Program Files (x86)\Git\cmd";"C:\Program Files\Git\cmd";%PATH%

ECHO.
ECHO Making sure we have a web.config
IF NOT EXIST "%CD%\..\src\Umbraco.Web.UI\web.config" COPY "%CD%\..\src\Umbraco.Web.UI\web.Template.config" "%CD%\..\src\Umbraco.Web.UI\web.config"

ECHO.
ECHO Reporting NuGet version
"%CD%\..\src\.nuget\NuGet.exe" help | findstr "^NuGet Version:"

ECHO.
ECHO Restoring NuGet packages
ECHO Into %nuGetFolder%
"%CD%\..\src\.nuget\NuGet.exe" restore "%CD%\..\src\umbraco.sln" -Verbosity Quiet -NonInteractive -PackagesDirectory "%nuGetFolder%"
IF ERRORLEVEL 1 GOTO :error

ECHO.
ECHO.
ECHO Performing MSBuild and producing Umbraco binaries zip files
ECHO This takes a few minutes and logging is set to report warnings
ECHO and errors only so it might seems like nothing is happening for a while. 
ECHO You can check the msbuild.log file for progress.
ECHO.
%MSBUILD% "Build.proj" /p:BUILD_RELEASE=%RELEASE% /p:BUILD_COMMENT=%COMMENT% /p:NugetPackagesDirectory="%nuGetFolder%" /consoleloggerparameters:Summary;ErrorsOnly /fileLogger
IF ERRORLEVEL 1 GOTO error

ECHO.
ECHO Setting node_modules folder to hidden to prevent VS13 from crashing on it while loading the websites project
attrib +h ..\src\Umbraco.Web.UI.Client\node_modules

IF %SKIPNUGET% EQU 1 GOTO success

ECHO.
ECHO Adding Web.config transform files to the NuGet package
REN .\_BuildOutput\WebApp\Views\Web.config Web.config.transform

ECHO.
ECHO Packing the NuGet release files
..\src\.nuget\NuGet.exe Pack NuSpecs\UmbracoCms.Core.nuspec -Version %VERSION% -Symbols -Verbosity quiet
..\src\.nuget\NuGet.exe Pack NuSpecs\UmbracoCms.Compat7.nuspec -Version %VERSION% -Symbols -Verbosity quiet
..\src\.nuget\NuGet.exe Pack NuSpecs\UmbracoCms.nuspec -Version %VERSION% -Verbosity quiet
IF ERRORLEVEL 1 GOTO error

:success
ECHO.
ECHO No errors were detected!
ECHO There may still be some in the output, which you would need to investigate.
ECHO Warnings are usually normal.
ECHO.
ECHO.
GOTO :EOF

:error

ECHO.
ECHO Errors were detected!
ECHO.

REM don't pause if continuous integration else the build server waits forever
REM before cancelling the build (and, there is noone to read the output anyways)
IF %INTEGRATION% NEQ 1 PAUSE
