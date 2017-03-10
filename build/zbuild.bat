@ECHO OFF

REM We do not read from UmbracoVersion.txt
REM Instead we hard-code our versions here
REM And then we write UmbracoVersion.txt

SET release=7.6.0
SET comment=alpha074

REM build.bat does it now
REM ECHO # Usage: on line 2 put the release version, on line 3 put the version comment (example: beta)> UmbracoVersion.txt
REM ECHO %release%>> UmbracoVersion.txt
REM ECHO %comment%>> UmbracoVersion.txt

:: /skipnuget
CALL build.bat /release:%release% /comment:%comment%
IF ERRORLEVEL 1 GOTO error

SET version=%release%
IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)

ECHO.
ECHO Publish NuGet release files to our own local NuGet repo
copy UmbracoCms.%version%.nupkg d:\d\NuGet\Packages 
IF ERRORLEVEL 1 GOTO error
copy UmbracoCms.Core.%version%.nupkg d:\d\NuGet\Packages 
IF ERRORLEVEL 1 GOTO error

ECHO.
ECHO Clear our local NuGet repo cache
curl http://nuget.local/nugetserver/api/clear-cache
IF ERRORLEVEL 1 GOTO error

ECHO.
ECHO.
GOTO :eof

:error
ECHO.
ECHO Error!
ECHO.
