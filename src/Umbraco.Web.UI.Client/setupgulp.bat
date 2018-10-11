@ECHO OFF

ECHO.
ECHO.
ECHO This will only work when you have NPM available on the command line
ECHO Works great with NodeJS Portable - https://gareth.flowers/nodejs-portable/
ECHO.
ECHO.
set /P c=Are you sure you want to continue [Y/N]?
if /I "%c%" EQU "Y" goto :setupgulp
if /I "%c%" EQU "N" goto :eof

:setupgulp
call npm install
call npm -g install gulp
call npm -g install gulp-cli

ECHO.
ECHO.
ECHO You should now be able to run: gulp build or gulp dev
ECHO.
ECHO.