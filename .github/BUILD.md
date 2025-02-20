# Umbraco CMS Build

## Are you sure?

In order to use Umbraco as a CMS and build your website with it, you should not build it yourself. If you're reading this then you're trying to contribute to Umbraco or you're debugging a complex issue.

-   Are you about to [create a pull request for Umbraco][contribution guidelines]?
-   Are you trying to get to the bottom of a problem in your existing Umbraco installation?

If the answer is yes, please read on. Otherwise, make sure to head on over [to the download page](https://our.umbraco.com/download) and start using Umbraco CMS as intended.

## Table of contents

↖️ You can jump to any section by using the "table of contents" button ( ![Table of contents icon](img/tableofcontentsicon.svg) ) above.

## Working with the Umbraco source code

Did you read ["Are you sure"](#are-you-sure)?

[More details about contributing to Umbraco and how to use the GitHub tooling can be found in our guide to contributing.][contribution guidelines]

If you want to run a build without debugging, see [Building from source](#building-from-source) below. This runs the build in the same way it is run on our build servers.

If you've got this far and are keen to get stuck in helping us fix a bug or implement a feature, great! Please read on...

### Prerequisites

In order to work with the Umbraco source code locally, first make sure you have the following installed.

-   Your favourite IDE: [Visual Studio 2022 v17+ with .NET 7+](https://visualstudio.microsoft.com/vs/), [Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/)
-   [dotnet SDK v9+](https://dotnet.microsoft.com/en-us/download)
-   [Node.js v20+](https://nodejs.org/en/download/)
-   npm v10+ (installed with Node.js)
-   [Git command line](https://git-scm.com/download/)

### Familiarizing yourself with the code

Umbraco is a .NET application using C#. The solution is broken down into multiple projects.  There are several class libraries. The `Umbraco.Web.UI` project is the main project that hosts the back office and login screen. This is the project you will want to run to see your changes.

There are two web projects in the solution with client-side assets based on TypeScript, `Umbraco.Web.UI.Client` and `Umbraco.Web.UI.Login`.

There are a few different ways to work locally when implementing features or fixing issues with the Umbraco CMS. Depending on whether you are working solely on the front-end, solely on the back-end, or somewhere in between, you may find different workflows work best for you.

Here are some suggestions based on how we work on developing Umbraco at HQ.

### First checkout

When you first clone the source code, build the whole solution via your IDE. You can then start the `Umbraco.Web.UI` project via the IDE or the command line and should find everything across front and back-end is built and running.

```
cd <solution root>\src\Umbraco.Web.UI
dotnet run --no-build
```

When the page loads in your web browser, you can follow the installer to set up a database for debugging. When complete, you will have an empty Umbraco installation to begin working with. You may also wish to install a [starter kit][https://marketplace.umbraco.com/category/themes-&-starter-kits] to ease your debugging.

### Back-end only changes

If you are working on back-end only features, when switching branches or pulling down the latest from GitHub, you will find the front-end getting rebuilt periodically when you look to build the back-end changes. This can take a while and slow you down. So if for a period of time you don't care about changes in the front-end, you can disable this build step.

Go to `Umbraco.Cms.StaticAssets.csproj` and comment out the following lines of MsBuild by adding a REM statement in front:

```
REM npm ci --no-fund --no-audit --prefer-offline
REM npm run build:for:cms
```

Just be careful not to include this change in your PR.

### Front-end only changes

Conversely, if you are working on front-end only, you want to build the back-end once and then run it. Before you do so, update the configuration in `appSettings.json` to add the following under `Umbraco:Cms:Security`:

```
"BackOfficeHost": "http://localhost:5173",
"AuthorizeCallbackPathName": "/oauth_complete",
"AuthorizeCallbackLogoutPathName": "/logout",
"AuthorizeCallbackErrorPathName": "/error"
```

Then run Umbraco from the command line.

```
cd <solution root>\src\Umbraco.Web.UI
dotnet run --no-build
```

In another terminal window, run the following to watch the front-end changes and launch Umbraco using the URL indicated from this task.

```
cd <solution root>\src\Umbraco.Web.UI.Client
npm run dev:server
```

You'll find as you make changes to the front-end files, the updates will be picked up and your browser refreshed automatically.

> [!NOTE]
> The caching for the back office has been described as 'aggressive' so we often find it's best when making back office changes to [disable caching in the browser (check "Disable cache" on the "Network" tab of developer tools)][disable browser caching] to help you to see the changes you're making.

Whilst most of the backoffice code lives in `Umbraco.Web.UI.Client`, the login screen is in a separate project. If you do any work with that you can build with:

```
cd <solution root>\src\Umbraco.Web.UI.Login
npm run build
```

In both front-end projects, if you've refreshed your branch from the latest on GitHub you may need to update front-end dependencies.

To do that, run:

```
npm ci --no-fund --no-audit --prefer-offline
```

### Full-stack changes

If working across both front and back-end, follow both methods and use `dotnet watch`, or re-run `dotnet run` (or `dotnet build` followed by `dotnet run --no-build`) whenever you need to update the back-end code.

Request and response models used by the management APIs are made available client-side as generated code. If you make changes to the management API, you can re-generate the typed client code with:

```
cd <solution root>\src\Umbraco.Web.UI.Client
npm run generate:server-api-dev
```

Please also update the `OpenApi.json` file held in the solution by copying and pasting the output from `/umbraco/swagger/management/swagger.json`.

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
[ disable browser caching ]: https://techwiser.com/disable-cache-google-chrome-firefox "Instructions on how to disable browser caching in Chrome and Firefox"
