@ECHO OFF
set version=6.0.0.5
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build.proj" /p:BUILD_NUMBER=%version%
..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.Core.nuspec -Version %version%
..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.nuspec -Version %version%


if ERRORLEVEL 1 goto :showerror

goto :EOF

:showerror
pause