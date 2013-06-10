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