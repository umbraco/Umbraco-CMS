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
ECHO If this is due to a SecurityError then please refer to BUILD.md for help!
ECHO.
