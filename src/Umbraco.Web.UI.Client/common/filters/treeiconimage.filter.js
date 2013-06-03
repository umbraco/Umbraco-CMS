'use strict';

define(['angular'], function (angular) {

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

});