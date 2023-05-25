# New backoffice

> **Warning**:
> This is an early WIP and is set not to be packable since we don't want to release this yet. There will be breaking changes in these projects.

This solution folder contains the projects for the new backoffice. If you're looking to fix or improve the existing CMS, this is not the place to do it, although we do very much appreciate your efforts.

### Getting Started: Management API

This branch contains the project for the new Management API as well as the new Backoffice. To get started with the Management API you should:
* Run any of the executables (Umbraco.Web.UI or Umbraco.Web.UI.New)
* Access "/umbraco/swagger" in the browser to see the Swagger interface
* The API lives in the Umbraco.Cms.Api.Management project

### Getting Started: Backoffice
To run the new Backoffice:
* Execute `git submodule init` and then `git submodule update` to get the files into Umbraco.Web.UI.New.Client project
* Run Umbraco.Web.UI.New project
    * If you get a white page delete Umbraco.Cms.StaticAssets\wwwroot\umbraco folder and run `npm ci && npm run build:for:cms` inside Umbraco.Web.UI.New.Client folder to clear out any leftover files from older versions. 
* If you are going to work on the Backoffice, you can either go to the Umbraco.Web.UI.New.Client folder and check out a new branch or set it up in your IDE, which will allow you to commit to each repository simultaneously:
  * **Rider**: Preferences -> Version Control -> Directory Mappings -> Click the '+' sign

### Latest version
* If you want to get the latest changes from the Backoffice repository, run `git submodule update` again which will pull the latest main branch.

### Project structure

Since the new backoffice API is still very much a work in progress, we've created new projects for the new backoffice API:

* Umbrao.Cms.ManagementApi - The "presentation layer" for the management API
* "New" versions of existing projects, should be merged with the existing projects when the new API is released:
    * Umbraco.Cms.Core
    * Umbraco.Cms.Infrastructure
    * Umbraco.Cms.Web.Common
    * Umbraco.Web.UI.New
    * Umbraco.Web.UI.New.Client

This also means that we have to use "InternalsVisibleTo" for the new projects since these should be able to access the internal classes since they will when they get merged.
