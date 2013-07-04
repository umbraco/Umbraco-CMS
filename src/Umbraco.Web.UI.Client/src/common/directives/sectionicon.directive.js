angular.module("umbraco.directives")
.directive('sectionIcon', function ($compile, iconHelper) {
    return {
        restrict: 'E',
        replace: true,

        link: function (scope, element, attrs) {

            var icon = attrs.icon;

            if (iconHelper.isLegacyIcon(icon)) {
                //its a known legacy icon, convert to a new one
                element.html("<i class='" + iconHelper.convertFromLegacyIcon(icon) + "'></i>");
            }
            else if (iconHelper.isFileBasedIcon(icon)) {
                //it's a file, normally legacy so look in the icon tray images
                element.html("<img src='images/tray/" + icon + "'>");
            }
            else {
                //it's normal
                element.html("<i class='" + icon + "'></i>");
            }
        }
    };
});