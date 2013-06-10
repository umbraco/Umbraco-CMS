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
}
angular.module('umbraco.filters').filter("umbTreeIconStyle", treeIconStyleFilter);