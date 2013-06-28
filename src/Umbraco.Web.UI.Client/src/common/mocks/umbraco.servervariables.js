//TODO: This would be nicer as an angular module so it can be injected into stuff... that'd be heaps nicer, but
// how to do that when this is not a regular JS file, it is a server side JS file and RequireJS seems to only want
// to force load JS files ?

//create the namespace (NOTE: This loads before any dependencies so we don't have a namespace mgr so we just create it manually)
var Umbraco = {};
Umbraco.Sys = {};
//define a global static object
Umbraco.Sys.ServerVariables = {
    "umbracoPath": "/umbraco",
    "contentApiBaseUrl": "/umbraco/UmbracoApi/Content/",
    "mediaApiBaseUrl": "/umbraco/UmbracoApi/Media/",
    "treeApplicationApiBaseUrl": "/umbraco/UmbracoTrees/ApplicationTreeApi/",
    "contentTypeApiBaseUrl": "/umbraco/Api/ContentType/",
    "mediaTypeApiBaseUrl": "/umbraco/Api/MediaTypeApi/",
    "authenticationApiBaseUrl": "/umbraco/UmbracoApi/Authentication/",
    "MyPackage": {
        "serverEnvironmentView": "/Belle/PropertyEditors/ServerSidePropertyEditors/ServerEnvironment"
    }
};