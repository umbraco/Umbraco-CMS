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
build.bat -integration -nugetfolder:%PACKAGES% -nugetpkg

COPY UmbracoCms.* ..\