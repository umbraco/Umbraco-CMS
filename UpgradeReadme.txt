* //TODO: All removed files will be in the umbraco.Legacy project unless otherwise noted

* DataType: 'editor' removed (editor.cs)
* removed WysiwygDataType.cs

* Many files have been removed but a zip file containing all removed files has been supplied

* Remove umbraco.controls.helper.cs?

* Any packages that have pages containing Umbraco Controls should now have a 
DependencyLoader registered on them, otherwise the controls will not down register client files
to be downlaoded (perhaps theres a better way to implement this... (i.e. check if a DependencyLoader
exists in the current context, and if it doesn't it registers the scripts?)

* Umbraco_Client folder path needs to be specified in AppSettings

* The old TinyMCE was not upgraded to use ClientDependency but it's paths have been changed
to use the UmbracoClientPath setting

* removed CheckBoxTree.cs [permanently!, not in legacy package]
* removed windowCloser.cs

* All references to the old Client Dependency libraries have been removed completely.

* Moved treeIcons.css to ~/[UmbracoClientFolder]/Tree/treeIcons.css

* Removed all IFormHandler dependencies and therefore removed the idea of FormHandlers all together:
- IFormHandler
- /umbraco/formhandler.cs
- StandardFormHandlers

* So far only minor database upgrade (not structure just data)

* Moved all old TinyMCE supporting files to legacy project
* Moved all old TinyMCE code files (that are not used to legacy project)
* Moved all old TinyMCE plugins to legacy project

* Moved jquery-fieldselection.js to umbraco_client/Application/jQuery

* Removed old internal indexer/searcher/SearchItem
* Removed OnBeforeIndexing, OnAfterIndexing, AddToIndexEventArgs, IndexEventHandler, BeforeAddToIndex, 
FireBeforeAddToIndex, AfterAddToIndex, FireAfterAddToIndex, Document.Index

* Removed /umbraco/dashboard/webService.cs as it wasn't doing anything
* Removed /umbraco/dashboard/search.aspx as this is an old handler used by quickEdit.ascx
* Removed /umbraco/dashboard/quickEdit.ascx and replaced with /umbraco/Search/QuickSearch.ascx
* Removed /umbraco/dashboard/quickEdit.js and repalced with /umbraco/Search/quickEdit.js
* Moved jquery autocomplete to /umbraco_client/Application/Jquery
* Removed /umbraco/webservices/Search.asmx as the SearchItem object has been removed
* Removed /umbraco/reindex.aspx

* Removed /umbraco/dialogs/editImage.aspx since it didn't do anything at all