@ECHO OFF
set version=6.0.0.4
..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.Core.nuspec -Version %version%
..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.nuspec -Version %version%

if ERRORLEVEL 1 goto :showerror

goto :EOF

:showerror
pause