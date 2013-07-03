/**
    * @ngdoc filter 
    * @name umbraco.filters:sectionIcon
    * @description This will properly render the tree icon image based on the tree icon set on the server
    **/
function sectionIconFilter(iconHelper) {
    return function (sectionIconClass) {        
        if (iconHelper.isLegacyIcon(sectionIconClass)) {
            return iconHelper.convertFromLegacyIcon(sectionIconClass);
        }
        else {
            return sectionIconClass;
        }
    };
}
angular.module('umbraco.filters').filter("sectionIcon", sectionIconFilter);