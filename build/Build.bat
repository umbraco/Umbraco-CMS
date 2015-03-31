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
IF [%1] NEQ [] (SET release=%1)
IF [%2] NEQ [] (SET comment=%2) ELSE (IF [%1] NEQ [] (SET "comment="))

SET version=%release%

IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)
ECHO Building Umbraco %version%

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %release%

ECHO Installing the Microsoft.Bcl.Build package before anything else, otherwise you'd have to run build.cmd twice
SET nuGetFolder=%CD%\..\src\packages\
..\src\.nuget\NuGet.exe sources Remove -Name MyGetUmbracoCore
..\src\.nuget\NuGet.exe sources Add -Name MyGetUmbracoCore -Source https://www.myget.org/F/umbracocore/api/v2/ >NUL
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Web.UI\packages.config -OutputDirectory %nuGetFolder%
..\src\.nuget\NuGet.exe install ..\src\umbraco.businesslogic\packages.config -OutputDirectory %nuGetFolder%
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Core\packages.config -OutputDirectory %nuGetFolder%

ECHO Removing the belle build folder and bower_components folder to make sure everything is clean as a whistle
RD ..\src\Umbraco.Web.UI.Client\build /Q /S
RD ..\src\Umbraco.Web.UI.Client\bower_components /Q /S

ECHO Removing existing built files to make sure everything is clean as a whistle
RMDIR /Q /S _BuildOutput
DEL /F /Q UmbracoCms.*.zip
DEL /F /Q UmbracoExamine.*.zip
DEL /F /Q UmbracoCms.*.nupkg
DEL /F /Q webpihash.txt

ECHO Making sure Git is in the path so that the build can succeed
CALL InstallGit.cmd
ECHO Performing MSBuild and producing Umbraco binaries zip files
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment%

ECHO Setting node_modules folder to hidden to prevent VS13 from crashing on it while loading the websites project
attrib +h ..\src\Umbraco.Web.UI.Client\node_modules

ECHO Adding Web.config transform files to the NuGet package
REN .\_BuildOutput\WebApp\MacroScripts\Web.config Web.config.transform
REN .\_BuildOutput\WebApp\Views\Web.config Web.config.transform
REN .\_BuildOutput\WebApp\Xslt\Web.config Web.config.transform

ECHO Packing the NuGet release files
..\src\.nuget\NuGet.exe Pack NuSpecs\UmbracoCms.Core.nuspec -Version %version%
..\src\.nuget\NuGet.exe Pack NuSpecs\UmbracoCms.nuspec -Version %version%
                        
IF ERRORLEVEL 1 GOTO :showerror

GOTO :EOF

:showerror
PAUSE
