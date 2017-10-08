@ECHO OFF
powershell .\build\build.ps1

IF ERRORLEVEL 1 (
	GOTO :error 
) ELSE (
	GOTO :EOF 
)

:error
ECHO.
ECHO Can not run build\build.ps1.
ECHO If this is due to a SecurityError then make sure to run the following command from an administrator command prompt:
ECHO.
ECHO powershell Set-ExecutionPolicy -ExecutionPolicy RemoteSigned