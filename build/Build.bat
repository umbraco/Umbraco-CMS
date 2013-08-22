@ECHO OFF
SET release=6.1.4
SET comment=
SET version=%release%

IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %release%

%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment%

echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\App_Code\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\App_Data\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\App_Plugins\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\css\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\macroScripts\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\masterpages\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\media\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\scripts\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\usercontrols\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\Views\Partials\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\Views\MacroPartials\dummy.txt
echo This file is only here so that the containing folder will be included in the NuGet package, it is safe to delete. > .\_BuildOutput\WebApp\xslt\dummy.txt

..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.Core.nuspec -Version %version%
..\src\.nuget\NuGet.exe pack NuSpecs\UmbracoCms.nuspec -Version %version%

IF ERRORLEVEL 1 GOTO :showerror

GOTO :EOF

:showerror
PAUSE