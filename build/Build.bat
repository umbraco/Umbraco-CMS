@ECHO OFF
SET release=7.0.2
SET comment=
SET version=%release%

IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)
ECHO Building Umbraco %version%

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %release%

SET nuGetFolder=%CD%\..\src\packages\

ECHO Installing Npm NuGet Package
..\src\.nuget\NuGet.exe install Npm.js -OutputDirectory %nuGetFolder%

for /f "delims=" %%A in ('dir %nuGetFolder%node.js.* /b') do set "nodePath=%nuGetFolder%%%A\"
for /f "delims=" %%A in ('dir %nuGetFolder%npm.js.* /b') do set "npmPath=%nuGetFolder%%%A\tools\"

ECHO Temporarily adding Npm and Node to path
SET oldPath=%PATH%

path=%npmPath%;%nodePath%;%PATH%

SET buildFolder="%CD%"

ECHO Go to Umbraco.Web.UI.Client folder
CD ..\src\Umbraco.Web.UI.Client\

ECHO Do npm install and the grunt build of Belle
call npm install
call npm install -g grunt-cli
call grunt

ECHO Reset path to what it was before
path=%oldPath%

ECHO Move back to the build folder
CD %buildFolder% 

ECHO Installing the Microsoft.Bcl.Build package before anything else, otherwise you'd have to run build.cmd twice
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Web.UI\packages.config -OutputDirectory %nuGetFolder%

ECHO Performing MSBuild and producing Umbraco binaries zip files
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment%

ECHO Adding dummy files to include in the NuGet package so that empty folders actually get created
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\App_Code\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\App_Data\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\App_Plugins\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\css\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\masterpages\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\media\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\scripts\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\usercontrols\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\Views\Partials\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\Views\MacroPartials\dummy.txt

ECHO Adding Web.config transform files to the NuGet package
ren .\_BuildOutput\WebApp\MacroScripts\Web.config Web.config.transform
ren .\_BuildOutput\WebApp\Views\Web.config Web.config.transform

ECHO Packing the NuGet release files
..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.Core.nuspec -Version %version%
..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.nuspec -Version %version%

IF ERRORLEVEL 1 GOTO :showerror

GOTO :EOF

:showerror
PAUSE
