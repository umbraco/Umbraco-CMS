@ECHO OFF
SETLOCAL
	:: SETLOCAL is on, so changes to the path not persist to the actual user's path

SET toolsFolder=%CD%\tools\
ECHO Current folder: %CD%

SET nodeFileName=node-v6.9.1-win-x86.7z
SET nodeExtractFolder=%toolsFolder%node.js.691

IF NOT EXIST "%nodeExtractFolder%" (
	ECHO Downloading http://nodejs.org/dist/v6.9.1/%nodeFileName% to %toolsFolder%%nodeFileName%
	powershell -Command "(New-Object Net.WebClient).DownloadFile('http://nodejs.org/dist/v6.9.1/%nodeFileName%', '%toolsFolder%%nodeFileName%')"
	ECHO Extracting %nodeFileName% to %nodeExtractFolder%
	"%toolsFolder%\7za.exe" x "%toolsFolder%\%nodeFileName%" -o"%nodeExtractFolder%" -aos > nul
)
FOR /f "delims=" %%A in ('dir "%nodeExtractFolder%\node*" /b') DO SET "nodePath=%nodeExtractFolder%\%%A"


SET nuGetExecutable=%CD%\tools\nuget.exe
IF NOT EXIST "%nuGetExecutable%" (
	ECHO Downloading https://dist.nuget.org/win-x86-commandline/latest/nuget.exe to %nuGetExecutable%
	powershell -Command "(New-Object Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe', '%nuGetExecutable%')"
)

SET drive=%CD:~0,2%
SET nuGetFolder=%drive%\packages\
FOR /f "delims=" %%A in ('dir "%nuGetFolder%npm.*" /b') DO SET "npmPath=%nuGetFolder%%%A\"

IF [%npmPath%] == [] GOTO :installnpm 
IF NOT [%npmPath%] == [] GOTO :build

:installnpm
	ECHO Downloading npm
	ECHO Configured packages folder: %nuGetFolder%	
	ECHO Installing Npm NuGet Package
	"%nuGetExecutable%" install Npm -OutputDirectory %nuGetFolder% -Verbosity detailed
	REM Ensures that we look for the just downloaded NPM, not whatever the user has installed on their machine
	FOR /f "delims=" %%A in ('dir %nuGetFolder%npm.* /b') DO SET "npmPath=%nuGetFolder%%%A\"
	GOTO :build

:build
    ECHO Adding Npm and Node to path 
    REM SETLOCAL is on, so changes to the path not persist to the actual user's path
    PATH="%npmPath%";"%nodePath%";%PATH%
    SET buildFolder=%CD%

    ECHO Change directory to %CD%\..\src\Umbraco.Web.UI.Client\
    CD %CD%\..\src\Umbraco.Web.UI.Client\

    ECHO Do npm install and the grunt build of Belle
    call npm cache clean --quiet
    call npm install --quiet
    call npm install -g grunt-cli --quiet
    call npm install -g bower --quiet
    call grunt build --buildversion=%release%

    ECHO Move back to the build folder
    CD "%buildFolder%"