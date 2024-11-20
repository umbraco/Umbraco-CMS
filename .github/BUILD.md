# Umbraco CMS Build

## Are you sure?

In order to use Umbraco as a CMS and build your website with it, you should not build it yourself. If you're reading this then you're trying to contribute to Umbraco or you're debugging a complex issue.

-   Are you about to [create a pull request for Umbraco][contribution guidelines]?
-   Are you trying to get to the bottom of a problem in your existing Umbraco installation?

If the answer is yes, please read on. Otherwise, make sure to head on over [to the download page](https://our.umbraco.com/download) and start using Umbraco CMS as intended.

## Table of contents

↖️ You can jump to any section by using the "table of contents" button ( ![Table of contents icon](img/tableofcontentsicon.svg) ) above.

## Debugging source locally

Did you read ["Are you sure"](#are-you-sure)?

[More details about contributing to Umbraco and how to use the GitHub tooling can be found in our guide to contributing.][contribution guidelines]

If you want to run a build without debugging, see [Building from source](#building-from-source) below. This runs the build in the same way it is run on our build servers.

> [!NOTE]
> The caching for the back office has been described as 'aggressive' so we often find it's best when making back office changes to [disable caching in the browser (check "Disable cache" on the "Network" tab of developer tools)][disable browser caching] to help you to see the changes you're making.

#### Debugging with VS Code

In order to build the Umbraco source code locally with Visual Studio Code, first make sure you have the following installed.

-   [Visual Studio Code](https://code.visualstudio.com/)
-   [dotnet SDK v9+](https://dotnet.microsoft.com/en-us/download)
-   [Node.js v20+](https://nodejs.org/en/download/)
-   npm v10+ (installed with Node.js)
-   [Git command line](https://git-scm.com/download/)

Open the root folder of the repository in Visual Studio Code.

To build the front end you'll need to open the command pallet (<kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>P</kbd>) and run `>Tasks: Run Task` followed by `Client Build`.

You can also run the tasks manually on the command line:

```
cd src\Umbraco.Web.UI.Client
npm i
npm run build:for:cms
```

If you just want to watch the UI Client to `Umbraco.Web.UI` then instead of running `build:for:cms`, you can do: `npm run dev:mock`. This will launch the Vite dev server on http://localhost:5173 and watch for changes with mocked API responses.

You can also run `npm run dev:server` to run the Vite server against a local Umbraco instance. In this case, you need to have the .NET project running and accept connections from the Vite server. Please see the Umbraco.Web.UI.Client README.md [Run against a local Umbraco instance](../src/Umbraco.Web.UI.Client/.github/README.md#run-against-a-local-umbraco-instance) for more information.

The login screen is a different frontend build, for that one you can run it as follows:

```
cd src\Umbraco.Web.UI.Login
npm i
npm run dev
```

If you just want to build the Login screen to `Umbraco.Web.UI` then instead of running `dev`, you can do: `npm run build`.

To run the C# portion of the project, either hit <kbd>F5</kbd> to begin debugging, or manually using the command line:

```
dotnet watch --project .\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj
```

**The initial C# build might take a _really_ long time (seriously, go and make a cup of coffee!) - but don't worry, this will be faster on subsequent runs.**

When the page eventually loads in your web browser, you can follow the installer to set up a database for debugging. You may also wish to install a [starter kit][starter kits] to ease your debugging.

#### Debugging with Visual Studio

In order to build the Umbraco source code locally with Visual Studio, first make sure you have the following installed.

-   [Visual Studio 2022 v17+ with .NET 7+](https://visualstudio.microsoft.com/vs/) ([the community edition is free](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=Community&rel=15) for you to use to contribute to Open Source projects)
-   [Node.js v20+](https://nodejs.org/en/download/)
-   npm v10+ (installed with Node.js)
-   [Git command line](https://git-scm.com/download/)

The easiest way to get started is to open `umbraco.sln` in Visual Studio.

Umbraco is a C# based codebase, which is mostly ASP.NET Core MVC based. You can make changes, build them in Visual Studio, and hit <kbd>F5</kbd> to see the result. There are two web projects in the solution, `Umbraco.Web.UI.Client` and `Umbraco.Web.UI.Login`. They each have their own build process and can be run separately. The `Umbraco.Web.UI` project is the main project that hosts the back office and login screen. This is the project you will want to run to see your changes. It will automatically restore and build the client projects when you run it.

If you want to watch the UI Client to `Umbraco.Web.UI` then you can open a terminal in `src/Umbraco.Web.UI.Client` where you can run `npm run dev:mock` manually. This will launch the Vite dev server on http://localhost:5173 and watch for changes with mocked API responses.

You can also run `npm run dev:server` to run the Vite server against a local Umbraco instance. In this case, you need to have the .NET project running and accept connections from the Vite server. Please see the Umbraco.Web.UI.Client README.md [Run against a local Umbraco instance](../src/Umbraco.Web.UI.Client/.github/README.md#run-against-a-local-umbraco-instance) for more information.

**The initial C# build might take a _really_ long time (seriously, go and make a cup of coffee!) - but don't worry, this will be faster on subsequent runs.**

When the page eventually loads in your web browser, you can follow the installer to set up a database for debugging. You may also wish to install a [starter kit][starter kits] to ease your debugging.

## Building from source

Did you read ["Are you sure"](#are-you-sure)?

Do note that this is only required if you want to test out your custom changes in a separate site (not the one in the Umbraco.Web.UI), if you just want to test your changes you can run the included test site using: `dotnet run` from `src/Umbraco.Web.UI/`

You may want to build a set of NuGet packages with your changes, this can be done using the dotnet pack command.

First enter the root of the project in a command line environment, and then use the following command to build the NuGet packages:

`dotnet pack -c Release -o Build.Out`

This will restore and build the project using the release configuration, and put all the outputted files in a folder called `Build.Out`

You can then add these as a local NuGet feed using the following command:

`dotnet nuget add source <Path to Build.Out folder> -n MyLocalFeed`

This will add a local nuget feed with the name "MyLocalFeed" and you'll now be able to use your custom built NuGet packages.

### Cleaning up

Once the solution has been used to run a site, one may want to "reset" the solution in order to run a fresh new site again.

The easiest way to do this by deleting the following files and folders:

-   src/Umbraco.Web.UI/appsettings.json
-   src/Umbraco.Web.UI/umbraco/Data

You only have to remove the connection strings from the appsettings, but removing the data folder ensures that the sqlite database gets deleted too.

Next time you run a build the `appsettings.json` file will be re-created in its default state.

This will leave media files and views around, but in most cases, it will be enough.

To perform a more complete clear, you will want to also delete the content of the media, views, scripts... directories.

The following command will force remove all untracked files and directories, whether they are ignored by Git or not. Combined with `git reset` it can recreate a pristine working directory.

    git clean -xdf .

For git documentation see:

-   git [clean](https://git-scm.com/docs/git-clean)
-   git [reset](https://git-scm.com/docs/git-reset)

## Azure DevOps

Umbraco uses Azure DevOps for continuous integration, nightly builds and release builds. The Umbraco CMS project on DevOps [is available for anonymous users](https://umbraco.visualstudio.com/Umbraco%20Cms)..

The produced artifacts are published in a container that can be downloaded from DevOps called "nupkg" which contains all the NuGet packages that got built.

## Quirks

### Git Quirks

Git might have issues dealing with long file paths during build. You may want/need to enable `core.longpaths` support (see [this page](https://github.com/msysgit/msysgit/wiki/Git-cannot-create-a-file-or-directory-with-a-long-path) for details).

[ contribution guidelines]: CONTRIBUTING.md "Read the guide to contributing for more details on contributing to Umbraco"
[ starter kits ]: https://our.umbraco.com/packages/?category=Starter%20Kits&version=9 "Browse starter kits available for v9 on Our "
[ disable browser caching ]: https://techwiser.com/disable-cache-google-chrome-firefox "Instructions on how to disable browser caching in Chrome and Firefox"
