
# to the whole build.proj dance
# then do the build.bat thing too
# or - use VSO to build the nuget packages?

# fixme
#  first thing, we need to restore NuGet!
#  then, we need to ./get-version

import-module ".\build-module.psm1"

# constants
# (unless we decide to make them params)
$buildConfiguration = "Release"

# get version
$version = $env:UMBRACO_VERSION
if ($version -eq "" -or $version -eq $null) {
  write-error "Could not determine version."
  exit 1
}
$release = $env:UMBRACO_RELEASE
$comment = $env:UMBRACO_COMMENT
if ($comment -eq $null) { $comment = "" }
$build = $env:UMBRACO_BUILD
if ($build -eq $null) { $build = "" }
write "Version $version"

# clear
write "Clear folders and files"
rmrf $(fullpath "..\src\Umbraco.Web.UI.Client\build")
rmrf $(fullpath "..\src\Umbraco.Web.UI.Client\bower_components")
rmrf $(fullpath "_BuildOutput")
rmf $(fullpath "UmbracoCms.*.zip")
rmf $(fullpath "UmbracoExamine.*.zip")
rmf $(fullpath "UmbracoCms.*.nupkg")
rmf $(fullpath "webpihash.txt")

# prepare
write "Making sure we have a web.config"
$webConfig = $(fullPath "..\src\Umbraco.Web.UI\web.config")
if (-not (test-path $webConfig)) {
  cp $(fullPath "..\src\Umbraco.Web.UI\web.Template.config") $webConfig
}

# fixme
#  at that point build.bat triggers build.proj
#  vso should build and run tests
#
#  rename _BuildOutput to _build.temp
#  are we building there?

# cleanup presentation
write "Cleanup presentation"
rmrf $(fullpath "_BuildOutput\WebApp\umbraco.presentation")

# copy various files
write "Copy xml documentation"
cp -force $(fullpath "_BuildOutput\bin\*.xml") $(fullpath "_BuildOutput\WebApp\bin")
write "Copy transformed configs and langs"
cp -force $(fullpath "_BuildOutput\WebApp\config\*.config")
cp -force $(fullpath "_BuildOutput\WebApp\config\*.js") $(fullpath "_BuildOutput\Configs")
cp -force $(fullpath "_BuildOutput\WebApp\config\lang\*.xml") $(fullpath "_BuildOutput\Configs\Lang")
cp -force $(fullpath "_BuildOutput\WebApp\web.config") $(fullpath "_BuildOutput\Configs\web.config.transform")
write "Copy transformed web.config"
#FIXME buildConfiguration!
cp -force $(fullpath "..\src\Umbraco.Web.UI\web.$buildConfiguration.Config.transformed") $(fullpath "_BuildOutput\WebApp\web.config")

# offset the modified timestamps on all umbraco dlls, as WebResources
# break if date is in the future, which, due to timezone offsets can happen.
write "Offset dlls timestamps"
ls $(fullpath "_BuildOutput\**\*.dll") | foreach {
  $_.CreationTime = $_.CreationTime.AddHours(-11)
  $_.LastWriteTime = $_.LastWriteTime.AddHours(-11)
}

# copy libs
write "Copy SqlCE libraries"
$base = $(fullpath "..\src\packages\SqlServerCE.4.0.0.1\")
ls $(fullpath "..\src\packages\SqlServerCE.4.0.0.1\**\*.*") |
  where { -not $_.Extension.StartsWith(".nu") -and -not $_.FullName.SubString($base.Length).StartsWith("lib\") } | 
  foreach {
    cp $_ -destination "_BuildOutput\bin\$($_.FullName.SubString($base.Length))"
    cp $_ -destination "_BuildOutput\WebApp\$($_.FullName.SubString($base.Length))"
  }
        
# copy Belle
write "Copy Belle"
$base = $(fullpath "..\src\Umbraco.Web.UI.Client\build\belle\")
ls $(fullpath "..\src\Umbraco.Web.UI.Client\build\belle\**\*.*") |
  where { -not $_.FullName.SubString($base.Length) -eq "index.html" } |
  foreach {
    cp $_ -destination "_BuildOutput\WebApp\umbraco\$($_.FullName.SubString($base.Length))"
  }

# fixme - keep going!
        
# zip the webpi app
write "Zip WebPI file"

# clear
write "Delete build folder"

exit 0

# hash the webpi file
write "Hash the WebPI file"
$webpifile = $(fullpath "UmbracoCms.WebPI.$version.zip")
$hashfile = $(fullpath "webpihash.txt")
$hash = genHash $webpifile
write $hash | out-file $hashfile -encoding ascii

# fixme
#  back to build.bat

# setting node_modules folder to hidden to prevent VS13 from
# crashing on it while loading the websites project (still needed?)
write "Set hidden attribute on node_modules"
$dir = get-item $(fullpath "..\src\Umbraco.Web.UI.Client\node_modules")
$dir.Attributes = $dir.Attributes -bor ([System.IO.FileAttributes]::Hidden)

# add Web.config transform files to the NuGet package
write "Add web.config transforms to NuGet package"
mv $(fullpath "_BuildOutput\WebApp\Views\Web.config") $(fullpath "_BuildOutput\WebApp\Views\Web.config.transform")
mv $(fullpath "_BuildOutput\WebApp\Xslt\Web.config") $(fullpath "_BuildOutput\WebApp\Xslt\Web.config.transform")

# fixme
#   and then we just nuget package
#   and we are done
#   vso should use it's own vso task

#"%NUGET%" Pack NuSpecs\UmbracoCms.Core.nuspec -Version %VERSION% -Symbols -Verbosity quiet
#"%NUGET%" Pack NuSpecs\UmbracoCms.nuspec -Version %VERSION% -Verbosity quiet

# eof