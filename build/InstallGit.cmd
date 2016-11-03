@ECHO OFF
SETLOCAL
REM SETLOCAL is on, so changes to the path not persist to the actual user's path

git.exe --version
IF %ERRORLEVEL%==9009 GOTO :trydefaultpath
REM OK, DONE
GOTO :EOF

:trydefaultpath
PATH=C:\Program Files (x86)\Git\cmd;C:\Program Files\Git\cmd;%PATH%
git.exe --version
IF %ERRORLEVEL%==9009 GOTO :showerror
REM OK, DONE
GOTO :EOF

:showerror
ECHO Git is not in your path and could not be found in C:\Program Files (x86)\Git\cmd nor in C:\Program Files\Git\cmd
SET /p install=" Do you want to install Git through Chocolatey [y/n]? " %=%
IF %install%==y (
	:: Create a temporary batch file to execute either after elevating to admin or as-is when the user is already admin
	ECHO @ECHO OFF > "%temp%\ChocoInstallGit.cmd"
	ECHO SETLOCAL >> "%temp%\ChocoInstallGit.cmd"
	ECHO ECHO Installing Chocolatey first >> "%temp%\ChocoInstallGit.cmd"
	ECHO @powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" >> "%temp%\ChocoInstallGit.cmd"
	ECHO SET PATH=%%PATH%%;%%ALLUSERSPROFILE%%\chocolatey\bin >> "%temp%\ChocoInstallGit.cmd"
	ECHO choco install git -y >> "%temp%\ChocoInstallGit.cmd"
	
	GOTO :installgit
) ELSE (
	GOTO :cantcontinue
)

:cantcontinue
ECHO Can't complete the build without Git being in the path. Please add it to be able to continue.
GOTO :EOF

:installgit
pushd %~dp0
    :: Running prompt elevated

:: --> Check for permissions
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

:: --> If error flag set, we do not have admin.
IF '%errorlevel%' NEQ '0' (
    GOTO UACPrompt
) ELSE ( GOTO gotAdmin )

:UACPrompt
    ECHO You're not currently running this with admin privileges, we'll now try to execute the install of Git through Chocolatey after elevating to admin privileges
    ECHO Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    ECHO UAC.ShellExecute "%temp%\ChocoInstallGit.cmd", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
	
    "%temp%\getadmin.vbs"
    EXIT /B
	
:gotAdmin
    IF EXIST "%temp%\getadmin.vbs" ( DEL "%temp%\getadmin.vbs" )
    pushd "%CD%"
    CD /D "%~dp0"
	
    CALL "%temp%\ChocoInstallGit.cmd"