Umbraco Cms Build
--
----

# Quick!

To build Umbraco, fire PowerShell and move to Umbraco's repository root (the directory that contains `src`, `build`, `README.md`...). There, trigger the build with the following command:

    build\build.ps1
    
By default, this builds the current version. It is possible to specify a different version as a parameter to the build script: 

    build\build.ps1 7.6.44

Valid version strings are defined in the `Set-UmbracoVersion` documentation below.

## Notes

Git might have issues dealing with long file paths during build. You may want/need to enable `core.longpaths` support (see [this page](https://github.com/msysgit/msysgit/wiki/Git-cannot-create-a-file-or-directory-with-a-long-path) for details).

# Build

The Umbraco Build solution relies on a PowerShell module. The module needs to be imported into PowerShell. From within Umbraco's repository root:

    build\build.ps1 -ModuleOnly

Or the abbreviated form:

    build\build.ps1 -mo

Once the module has been imported, a set of commands are added to PowerShell.

## Get-UmbracoBuildEnv

Gets the Umbraco build environment ie NuGet, Semver, Visual Studio, etc. Downloads things that can be downloaded such as NuGet. Examples:

    $uenv = Get-UmbracoBuildEnv
    Write-Host $uenv.SolutionRoot
    &$uenv.NuGet help
    
The object exposes the following properties:

* `SolutionRoot`: the absolute path to the solution root
* `VisualStudio`: a Visual Studio object (see below)
* `NuGet`: the absolute path to the NuGet executable
* `Zip`: the absolute path to the 7Zip executable
* `VsWhere`: the absolute path to the VsWhere executable
* `NodePath`: the absolute path to the Node install
* `NpmPath`: the absolute path to the Npm install

The Visual Studio object is `null` when Visual Studio has not been detected (eg on VSTS). When not null, the object exposes the following properties:

* `Path`: Visual Studio installation path (eg some place under `Program Files`)
* `Major`: Visual Studio major version (eg `15` for VS 2017)
* `Minor`: Visual Studio minor version
* `MsBUild`: the absolute path to the MsBuild executable

## Get-UmbracoVersion

Gets an object representing the current Umbraco version. Example:

    $v = Get-UmbracoVersion
    Write-Host $v.Semver

The object exposes the following properties:

* `Semver`: the semver object representing the version
* `Release`: the main part of the version (eg `7.6.33`)
* `Comment`: the pre release part of the version (eg `alpha02`)
* `Build`: the build number part of the version (eg `1234`)

## Set-UmbracoVersion

Modifies Umbraco files with the new version.

>This entirely replaces the legacy `UmbracoVersion.txt` file.

The version must be a valid semver version. It can include a *pre release* part (eg `alpha02`) and/or a *build number* (eg `1234`). Examples:

    Set-UmbracoVersion 7.6.33
    Set-UmbracoVersion 7.6.33-alpha02
    Set-UmbracoVersion 7.6.33+1234
    Set-UmbracoVersion 7.6.33-beta05+5678

Note that `Set-UmbracoVersion` enforces a slightly more restrictive naming scheme than what semver would tolerate. The pre release part can only be composed of a-z and 0-9, therefore `alpha033` is considered valid but not `alpha.033` nor `alpha033-preview` nor `RC2` (would need to be lowercased `rc2`). 

>It is considered best to add trailing zeroes to pre releases, else NuGet gets the order of versions wrong. So if you plan to have more than 10, but no more that 100 alpha versions, number the versions `alpha00`, `alpha01`, etc. 

## Build-Umbraco

Builds Umbraco. Temporary files are generated in `build.tmp` while the actual artifacts (zip files, NuGet packages...) are produced in `build.out`. Example:

    Build-Umbraco

Some log files, such as MsBuild logs, are produced in `build.tmp` too. The `build` directory should remain clean during a build.

### web.config

Building Umbraco requires a clean `web.config` file in the `Umbraco.Web.UI` project. If a `web.config` file already exists, the `pre-build` task (see below) will save it as `web.config.temp-build` and replace it with a clean copy of `web.Template.config`. The original file is replaced once it is safe to do so, by the `pre-packages` task.

## Build-UmbracoDocs

Builds umbraco documentation. Temporary files are generated in `build.tmp` while the actual artifacts (docs...) are produced in `build.out`. Example:

    Build-UmbracoDocs

Some log files, such as MsBuild logs, are produced in `build.tmp` too. The `build` directory should remain clean during a build.

## Verify-NuGet

Verifies that projects all require the same version of their dependencies, and that NuSpec files require versions that are consistent with projects. Example:

    Verify-NuGet

# VSTS

Continuous integration, nightly builds and release builds run on VSTS.

VSTS uses the `Build-Umbraco` command several times, each time passing a different *target* parameter. The supported targets are:

* `pre-build`: prepares the build
* `compile-belle`: compiles Belle
* `compile-umbraco`: compiles Umbraco
* `pre-tests`: prepares the tests
* `compile-tests`: compiles the tests
* `pre-packages`: prepares the packages
* `pkg-zip`: creates the zip files
* `pre-nuget`: prepares NuGet packages
* `pkg-nuget`: creates NuGet packages

All these targets are executed when `Build-Umbraco` is invoked without a parameter (or with the `all` parameter). On VSTS, compilations (of Umbraco and tests) are performed by dedicated VSTS tasks. Similarly, creating the NuGet packages is also performed by dedicated VSTS tasks.

Finally, the produced artifacts are published in two containers that can be downloaded from VSTS: `zips` contains the zip files while `nuget` contains the NuGet packages.

>During a VSTS build, some environment `UMBRACO_*` variables are exported by the `pre-build` target and can be reused in other targets *and* in VSTS tasks. The `UMBRACO_TMP` environment variable is used in `Umbraco.Tests` to disable some tests that have issues with VSTS at the moment.

# Notes

*This part needs to be cleaned up*

Nightlies should use some sort of build number.

We should increment versions as soon as a version is released. Ie, as soon as `7.6.33` is released, we should `Set-UmbracoVersion 7.6.34-alpha` and push.

NuGet / NuSpec consistency checks are performed in tests. We should move it so it is done as part of the PowerShell script even before we try to compile and run the tests.

There are still a few commands in `build` (to build docs, install Git or cleanup the install) that will need to be migrated to PowerShell.

/eof