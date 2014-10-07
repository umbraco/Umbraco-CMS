//create the namespace (NOTE: This loads before any dependencies so we don't have a namespace mgr so we just create it manually)
var Umbraco = {};
Umbraco.Sys = {};
//define a global static object
Umbraco.Sys.ServerVariables = {
    umbracoUrls: {
        "contentApiBaseUrl": "/umbraco/UmbracoApi/Content/",
        "mediaApiBaseUrl": "/umbraco/UmbracoApi/Media/",
        "dataTypeApiBaseUrl": "/umbraco/UmbracoApi/DataType/",
        "sectionApiBaseUrl": "/umbraco/UmbracoApi/Section/",
        "treeApplicationApiBaseUrl": "/umbraco/UmbracoTrees/ApplicationTreeApi/",
        "contentTypeApiBaseUrl": "/umbraco/Api/ContentType/",
        "mediaTypeApiBaseUrl": "/umbraco/Api/MediaType/",
        "macroApiBaseUrl": "/umbraco/Api/Macro/",
        "authenticationApiBaseUrl": "/umbraco/UmbracoApi/Authentication/",
        //For this we'll just provide a file that exists during the mock session since we don't really have legay js tree stuff
        "legacyTreeJs": "/belle/lib/lazyload/empty.js",
        "serverVarsJs": "/belle/lib/lazyload/empty.js",
        "imagesApiBaseUrl": "/umbraco/UmbracoApi/Images/",
        "entityApiBaseUrl": "/umbraco/UmbracoApi/Entity/",
        "dashboardApiBaseUrl": "/umbraco/UmbracoApi/Dashboard/",
        "updateCheckApiBaseUrl": "/umbraco/Api/UpdateCheck/",
        "rteApiBaseUrl": "/umbraco/UmbracoApi/RichTextPreValue/"
    },
    umbracoSettings: {
        "umbracoPath": "/umbraco",
        "appPluginsPath" : "/App_Plugins",
        "imageFileTypes": "jpeg,jpg,gif,bmp,png,tiff,tif",
        "keepUserLoggedIn": true
    },
    umbracoPlugins: {
        trees: [
            { alias: "myTree", packageFolder: "MyPackage" }
        ]
    },
    isDebuggingEnabled: true,
    application: {
        assemblyVersion: "1",
        version: "7"
    }
};