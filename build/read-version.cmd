SET VERSION_RELEASE=
SET VERSION_COMMENT=

:: Try to get the version and comment from Version.txt lines 2 and 3
IF EXIST Version.txt (
    FOR /F "skip=1 delims=" %%i IN (Version.txt) DO IF NOT DEFINED VERSION_RELEASE SET VERSION_RELEASE=%%i
    FOR /F "skip=2 delims=" %%i IN (Version.txt) DO IF NOT DEFINED VERSION_COMMENT SET VERSION_COMMENT=%%i
)
