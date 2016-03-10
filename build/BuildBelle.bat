@ECHO OFF
SETLOCAL

SET release=%1
ECHO Installing Npm NuGet Package

SET nuGetFolder=%CD%\..\src\packages\
ECHO Configured packages folder: %nuGetFolder%
ECHO Current folder: %CD%

%CD%\..\src\.nuget\NuGet.exe install Npm.js -OutputDirectory %nuGetFolder%  -Verbosity quiet

for /f "delims=" %%A in ('dir %nuGetFolder%node.js.* /b') do set "nodePath=%nuGetFolder%%%A\"
for /f "delims=" %%A in ('dir %nuGetFolder%npm.js.* /b') do set "npmPath=%nuGetFolder%%%A\tools\"

ECHO Adding Npm and Node to path 
REM SETLOCAL is on, so changes to the path not persist to the actual user's path
PATH=%npmPath%;%nodePath%;%PATH%

SET buildFolder=%CD%

ECHO Change directory to %CD%\..\src\Umbraco.Web.UI.Client\
CD %CD%\..\src\Umbraco.Web.UI.Client\

ECHO Do npm install and the grunt build of Belle
call npm install --quiet
call npm install -g grunt-cli --quiet
call npm install -g bower --quiet
call grunt build --buildversion=%release%

ECHO Move back to the build folder
CD %buildFolder% 