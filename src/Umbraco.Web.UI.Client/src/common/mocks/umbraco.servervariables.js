//create the namespace (NOTE: This loads before any dependencies so we don't have a namespace mgr so we just create it manually)
var Umbraco = {};
Umbraco.Sys = {};
//define a global static object
Umbraco.Sys.ServerVariables = {
    umbracoUrls: {
        "contentApiBaseUrl": "/umbraco/UmbracoApi/Content/",
        "mediaApiBaseUrl": "/umbraco/UmbracoApi/Media/",
        "sectionApiBaseUrl": "/umbraco/UmbracoApi/Section/",
        "treeApplicationApiBaseUrl": "/umbraco/UmbracoTrees/ApplicationTreeApi/",
        "contentTypeApiBaseUrl": "/umbraco/Api/ContentType/",
        "mediaTypeApiBaseUrl": "/umbraco/Api/MediaTypeApi/",
        "authenticationApiBaseUrl": "/umbraco/UmbracoApi/Authentication/"
    },
    umbracoSettings: {
        "umbracoPath": "/umbraco"    
    }
};