@ECHO OFF
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build.proj"

if ERRORLEVEL 1 goto :showerror

goto :EOF

:showerror
pause