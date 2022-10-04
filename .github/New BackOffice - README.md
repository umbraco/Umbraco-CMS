# New backoffice

> **Warning**:
> This is an early WIP and is set not to be packable since we don't want to release this yet. There will be breaking changes in these projects.

This solution folder contains the projects for the new backoffice. If you're looking to fix or improve the existing CMS, this is not the place to do it, although we do very much appreciate your efforts.

### Project structure

Since the new backoffice API is still very much a work in progress, we've created new projects for the new backoffice API:

* Umbrao.Cms.ManagementApi - The "presentation layer" for the management API
* "New" versions of existing projects, should be merged with the existing projects when the new API is released:
    * Umbraco.New.Cms.Core
    * Umbraco.New.Cms.Infrastructure
    * Umbraco.New.Cms.Web.Common

This also means that we have to use "InternalsVisibleTo" for the new projects since these should be able to access the internal classes since they will when they get merged.
