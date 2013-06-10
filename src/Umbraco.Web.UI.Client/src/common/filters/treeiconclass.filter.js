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
}
angular.module('umbraco.filters').filter("umbTreeIconClass", treeIconClassFilter);