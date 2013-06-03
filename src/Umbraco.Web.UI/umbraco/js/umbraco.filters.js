/*! umbraco - v0.0.1-SNAPSHOT - 2013-05-28
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app', 'angular'], function (app, angular) {
    angular.module('umbraco.filters', [])
            .filter('interpolate', ['version', function (version) {
                return function (text) {
                    return String(text).replace(/\%VERSION\%/mg, version);
                };
            }])
            .filter('propertyEditor', function () {
                return function (input) {
                    return "views/propertyeditors/" + String(input).replace('.', '/') + "/editor.html";
                };
            });

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
    };
    angular.module('umbraco.filters').filter("umbTreeIconImage", treeIconImageFilter);

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
    };
    angular.module('umbraco.filters').filter("umbTreeIconClass", treeIconClassFilter);


    return app;
});