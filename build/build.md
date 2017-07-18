Umbraco Core Build
--


# Environment

## Bootstrap

From within the solution directory,

    $env:PSModulePath = "$pwd\build\Modules\;$env:PSModulePath"
    Import-Module Umbraco.Build -Verbose -Force

## Get-UmbracoBuildEnv

Gets the Umbraco build environment ie NuGet, Semver, Visual Studio, etc. Can download things that can be downloaded such as NuGet. Examples:

    $uenv = Get-UmbracoBuildEnv
    write-host $uenv.SolutionRoot
    &$uenv.NuGet help

# Versions

## Get-UmbracoVersion

Gets an object representing the current Umbraco version. Example:

    $v = Get-UmbracoVersion
    write $v.Semver

## Set-UmbracoVersion

Modifies Umbraco files with the new version. Must be a valid semver version. Can have a *pre release* part (eg `alpha02`) and/or a *build number* (eg `1234`). Examples:

    Set-UmbracoVersion 7.6.33
    Set-UmbracoVersion 7.6.33-alpha02
    Set-UmbracoVersion 7.6.33+1234
    Set-UmbracoVersion 7.6.33-beta05+5678

>It is considered best to add trailing zeroes to pre releases, else NuGet gets the order of versions wrong. So if you plan to have more than 10, but no more that 100 alpha versions, number the versions `alpha00`, `alpha01`, etc. 

**[EVERYTHING BELOW IS OBSOLETE]**

Core can be entirely built, including running the grunt/gulp UI build and creating the NuGet packages, from the `build.bat` script.

## Script Usage 

### Parameters

This script accepts the following parameters:

`release:<release>` - the release number, e.g. "6.0.0" or "1.20.4".

`comment:<comment>` - the release comment, e.g. "alpha002" or "beta001".

`build:<number>` - the build number, e.g. "6689". This number should only be specified on the integration server when continuously building Core and/or releasing nightlies.

`nugetfolder:<folder>` - the location where NuGet packages should be restored and read from. The default value is `src/packages`.

`integration` - the script should not pause on errors.

`nugetpkg` - the script should create the NuGet packages.

`tests` - the script should build the tests projects.

### Usage

The script tries to read the release number and comment from the `Version.txt` file, then it overrides these values with those specified as arguments, if any, then it updates the `Version.txt` file again. That file does therefore not need to be edited manually.
 
The release number, comment and build number are used to produce the final SemVer version of the build, e.g. "6.0.0", "1.20.4-beta001", "7.5.12+6689", "8.0.0-alpha045+6689".
 
## Continuous Integration
 
Whenever commits are pushed to the GitHub repository, AppVeyor triggers a test build and runs the commands from the `appveyor.yml` file, which ends up running: 
 
```
build -build:%APPVEYOR_BUILD_NUMBER% -tests -integration -nugetfolder:%PACKAGES%
```
 
Thus reusing the release number and comment from `Version.txt` to compile everything, including tests. These tests are then executed.

## Development Releases

In order to bump the version number, do e.g.: 
```
build -release:7.6.0 -comment:beta001 -nugetpkg
```

This will rebuild the version on your local environment, just to make sure it all works. Git should then report that `Version.txt`, `??` and `??` have been updated. Commit these files with a message reading "Version 7.6.0-alpha075" and tag this commit with "dev-7.6-alpha075".

So you end up with one commit looking like:
```
* 6992b03 (tag: dev-7.6-alpha074) Version 7.6-alpha074  Stephan, 2 days ago
```

In addition, the command above creates a few `*.nupkg` files in the `build` folder, which you have to upload to MyGet so that others can use them.
 
## Public Releases

In order to bump the version number, do:
```
build -release:7.6.0
```

This will rebuild the version on your local environment, just to make sure it all works. Git should then report that `Version.txt`, `??` and `??` have been updated. Commit these files with a message reading "Version 7.6.0" and tag this commit with "release-7.6.0".

So you end up with one commit looking like:
```
* 6992b03 (tag: release-7.6.0) Version 7.6.0  Stephan, 2 days ago
```

Then, the development branch needs to be merged into `master-v7` and pushed. Then, an AppVeyor release build needs to be manually triggered. This build automatically creates the NuGet packages, and uploads them to NuGet.

AppVeyor uses a copy of the `appveyor-release-script.cmd`. Keep them in sync. This scripts ends up running:
```
build -integration -nugetfolder:%PACKAGES% -nugetpkg
```

## Dependencies

Before you build, it is important to check the nuspec files for dependencies, and compare with packages.config, so that they are sync. Could we automate this?

## Notes

With this flow in place, every continuous integration build that runs *after* 7.5.11 has been released, is still version 7.5.11+8896. Technically this is wrong, because 7.5.11 has been released, and what we are building really is 7.5.12.

I *think* that the flow should be slightly changed so that
- we tag `release-7.5.11`
- we merge for public release
- we *immediately* bump the version to `7.5.12` or `7.5.12-alpha` or...

(to be discussed)