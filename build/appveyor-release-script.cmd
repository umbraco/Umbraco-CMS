REM this script is used by the AppVeyor Release Build process
REM to build the release version from branch master-v7 and push
REM the produced items to NuGet
REM
REM it needs to be copied into AppVeyor interface, and is maintained
REM in the Core solution in build/appveyor-release-script.cmd

SET SLN=%CD%\
SET SRC=%SLN%\src
SET PACKAGES=%SRC%\packages
CD build
SET PATH=C:\Program Files (x86)\MSBuild\14.0\Bin;%PATH%

%SRC%\.nuget\NuGet.exe sources Add -Name MyGetUmbracoCore -Source https://www.myget.org/F/umbracocore/api/v2/ >NUL
build.bat -integration -nugetfolder:%PACKAGES%

COPY UmbracoCms.* ..\


REM -- below is original script

cd build

SET nuGetFolder=%CD%\..\src\packages\
..\src\.nuget\NuGet.exe sources Add -Name MyGetUmbracoCore -Source https://www.myget.org/F/umbracocore/api/v2/ >NUL
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Web.UI\packages.config -OutputDirectory %nuGetFolder% -Verbosity quiet

IF EXIST ..\src\umbraco.businesslogic\packages.config ..\src\.nuget\NuGet.exe install ..\src\umbraco.businesslogic\packages.config -OutputDirectory %nuGetFolder% -Verbosity quiet
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Core\packages.config -OutputDirectory %nuGetFolder% -Verbosity quiet

SET MSBUILD="C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe"
%MSBUILD% "../src/Umbraco.Tests/Umbraco.Tests.csproj" /consoleloggerparameters:Summary;ErrorsOnly;WarningsOnly

build.bat

COPY UmbracoCms.* ..\