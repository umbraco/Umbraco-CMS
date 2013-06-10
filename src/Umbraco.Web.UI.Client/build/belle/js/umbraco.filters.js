/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-10
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define([ 'app','angular'], function (app,angular) {
angular.module('umbraco.filters', []);
/**
* @ngdoc filter 
* @name umbraco.filters:propertyEditor
* @description This will ensure that the view for the property editor is rendered correctly, meaning it will check for an absolute path, otherwise load it in the normal umbraco path
**/
function propertyEditorFilter($log) {
    return function (input) {
        //if its not defined then return undefined
        if (!input){
            return input;
        }

        //Added logging here because this fires a ton of times and not sure that it should be!
        //$log.info("Filtering property editor view: " + input);
        var path = String(input);
        if (path.startsWith('/')) {
            return path;
        }
        else {
            return "views/propertyeditors/" + path.replace('.', '/') + "/editor.html";
        }       
    };
}

angular.module("umbraco.filters").filter('propertyEditor', propertyEditorFilter);
/**
* @ngdoc filter 
* @name umbraco.filters:umbTreeIconClass
* @restrict E
* @description This will properly render the tree icon class based on the tree icon set on the server
**/
function treeIconClassFilter() {
    return function (treeNode, standardClasses) {
        if (treeNode.iconIsClass) {
            return standardClasses + " " + treeNode.icon;
        }
        
        return standardClasses;
    };
}
angular.module('umbraco.filters').filter("umbTreeIconClass", treeIconClassFilter);
/**
* @ngdoc filter 
* @name umbraco.filters:umbTreeIconImage
* @restrict E
* @description This will properly render the tree icon image based on the tree icon set on the server
**/
function treeIconImageFilter() {
    return function (treeNode) {
        if (treeNode.iconIsClass) {
            return "";
        }

        return "<img src='" + treeNode.iconFilePath + "'></img>";
    };
}

//angular.module('umbraco.filters').filter("umbTreeIconImage", treeIconImageFilter);

return app;
});