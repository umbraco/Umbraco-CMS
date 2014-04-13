@ECHO OFF
ECHO Installing Npm NuGet Package

SET nuGetFolder=%CD%\..\src\packages\
ECHO Configured packages folder: %nuGetFolder%
ECHO Current folder: %CD%

%CD%\..\src\.nuget\NuGet.exe install Npm.js -OutputDirectory %nuGetFolder%

for /f "delims=" %%A in ('dir %nuGetFolder%node.js.* /b') do set "nodePath=%nuGetFolder%%%A\"
for /f "delims=" %%A in ('dir %nuGetFolder%npm.js.* /b') do set "npmPath=%nuGetFolder%%%A\tools\"

ECHO Temporarily adding Npm and Node to path
SET oldPath=%PATH%

path=%npmPath%;%nodePath%;%PATH%

SET buildFolder=%CD%

ECHO Change directory to %CD%\..\src\Umbraco.Web.UI.Client\
CD %CD%\..\src\Umbraco.Web.UI.Client\

ECHO Do npm install and the grunt build of Belle
call npm install
call npm install -g grunt-cli
call grunt build

ECHO Reset path to what it was before
path=%oldPath%

ECHO Move back to the build folder
CD %buildFolder% 