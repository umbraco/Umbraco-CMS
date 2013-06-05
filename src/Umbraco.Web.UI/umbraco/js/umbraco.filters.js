/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-03
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app', 'angular'], function (app, angular) {

    /**
    * @ngdoc filter 
    * @name umbraco.filters:umbTreeIconImage
    * @description This will properly render the tree icon image based on the tree icon set on the server
    **/
    function treeIconStyleFilter(treeIconHelper) {
        return function (treeNode) {
            if (treeNode.iconIsClass) {
                var converted = treeIconHelper.convertFromLegacy(treeNode);
                if (converted.startsWith('.')) {
                    //its legacy so add some width/height
                    return "height:16px;width:16px;";
                }
                return "";
            }
            return "background-image: url('" + treeNode.iconFilePath + "');height:16px;background-position:2px 0px";
        };
    };
    angular.module('umbraco').filter("umbTreeIconStyle", treeIconStyleFilter);

    /**
    * @ngdoc filter 
    * @name umbraco.filters:umbTreeIconClass
    * @description This will properly render the tree icon class based on the tree icon set on the server
    **/
    function treeIconClassFilter(treeIconHelper) {
        return function (treeNode, standardClasses) {
            if (treeNode.iconIsClass) {
                return standardClasses + " " + treeIconHelper.convertFromLegacy(treeNode);
            }
            //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
            return standardClasses + " icon-custom-file";
        };
    };
    angular.module('umbraco').filter("umbTreeIconClass", treeIconClassFilter);


    /**
    * @ngdoc filter 
    * @name umbraco.filters:propertyEditor
    * @description This will ensure that the view for the property editor is rendered correctly, meaning it will check for an absolute path, otherwise load it in the normal umbraco path
    **/
    function propertyEditorFilter($log) {
        return function (input) {
            //if its not defined then return undefined
            if (!input) return input;

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

    angular.module('umbraco.filters', ["umbraco.services.tree"])
        .filter('interpolate', ['version', function(version) {
            return function(text) {
                return String(text).replace(/\%VERSION\%/mg, version);
            };
        }])
        .filter('propertyEditor', propertyEditorFilter);

return app;
});