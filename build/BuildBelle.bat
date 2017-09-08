@ECHO OFF
SETLOCAL
	:: SETLOCAL is on, so changes to the path not persist to the actual user's path

SET toolsFolder=%CD%\tools\
ECHO Current folder: %CD%

SET nodeVersion=6.11.2

SET nodeFileName=node-v%nodeVersion%-win-x86.7z
SET nodeExtractFolder=%toolsFolder%node.js.%nodeVersion%

SET nuGetExecutable=%CD%\tools\nuget.exe
IF NOT EXIST "%nuGetExecutable%" (
	ECHO Downloading https://dist.nuget.org/win-x86-commandline/latest/nuget.exe to %nuGetExecutable%
	powershell -Command "(New-Object Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe', '%nuGetExecutable%')"
)

:: We need 7za.exe for BuildBelle.bat
IF NOT EXIST "%toolsFolder%7za.exe" (
	ECHO 7zip not found - fetching now
	"%nuGetExecutable%" install 7-Zip.CommandLine -OutputDirectory tools -Verbosity quiet
)

:: Put 7za.exe and vswhere.exe in a predictable path (not version specific)
FOR /f "delims=" %%A in ('dir "%toolsFolder%7-Zip.CommandLine.*" /b') DO SET "sevenZipExePath=%toolsFolder%%%A\"
MOVE "%sevenZipExePath%tools\7za.exe" "%toolsFolder%7za.exe"

IF NOT EXIST "%nodeExtractFolder%" (
	ECHO Downloading http://nodejs.org/dist/v%nodeVersion%/%nodeFileName% to %toolsFolder%%nodeFileName%
	powershell -Command "(New-Object Net.WebClient).DownloadFile('http://nodejs.org/dist/v%nodeVersion%/%nodeFileName%', '%toolsFolder%%nodeFileName%')"
	ECHO Extracting %nodeFileName% to %nodeExtractFolder%
	"%toolsFolder%\7za.exe" x "%toolsFolder%\%nodeFileName%" -o"%nodeExtractFolder%" -aos > nul
)
FOR /f "delims=" %%A in ('dir "%nodeExtractFolder%\node*" /b') DO SET "nodePath=%nodeExtractFolder%\%%A"
	
:build
    ECHO Adding Node to path 
    REM SETLOCAL is on, so changes to the path not persist to the actual user's path
    PATH="%nodePath%";%PATH%

    ECHO Node version is: 
    call node -v
	
    ECHO npm version is:
    call npm -v

    SET buildFolder=%CD%

    ECHO Change directory to %CD%\..\src\Umbraco.Web.UI.Client\
    CD %CD%\..\src\Umbraco.Web.UI.Client\
    
    ECHO Clean npm cache 
    call npm cache clean
	
    ECHO Installing bower
    call npm install -g bower
	
    ECHO Installing gulp and gulp cli
    call npm install -g gulp
    call npm install -g gulp-cli
	
    ECHO Executing npm install
    call npm install
	
    ECHO Do gulp build of Belle
    call gulp build --buildversion=%release%

    ECHO Move back to the build folder
    CD "%buildFolder%"