/**
* @ngdoc filter
* @name umbraco.filters.filter:umbTreeIconImage
* @description This will properly render the tree icon image based on the tree icon set on the server
**/
function treeIconStyleFilter(iconHelper) {
    return function (treeNode) {
        if (treeNode.iconIsClass) {
            var converted = iconHelper.convertFromLegacyTreeNodeIcon(treeNode);
            if (converted.startsWith('.')) {
                //its legacy so add some width/height
                return "height:16px;width:16px;";
            }
            return "";
        }
        return "background-image: url('" + treeNode.iconFilePath + "');height:16px; background-position:2px 0px; background-repeat: no-repeat";
    };
}
angular.module('umbraco.filters').filter("umbTreeIconStyle", treeIconStyleFilter);