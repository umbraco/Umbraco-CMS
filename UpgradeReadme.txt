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

* Moved treeIcons.css to ~/umbraco_client/Tree/treeIcons.css

* Removed all IFormHandler dependencies and therefore removed the idea of FormHandlers all together:
- IFormHandler
- /umbraco/formhandler.cs
- StandardFormHandlers