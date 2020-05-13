ECHO OFF
set cypressCommand=%1
for /f "tokens=*" %%a in ('Powershell.exe "((./../../../build/build.ps1 -g true).GetUmbracoVersion().Semver.ToString())"') do set Port=%%a

REM Remove .'s
set Port=%Port:.=%

REM pad with 0's so the length is 4
set "Port=%Port%0"
set "Port=%Port:~-4%"

set baseUrl=http://localhost:%Port%
ECHO ON
npx cypress %cypressCommand% --config baseUrl="%baseUrl%"
