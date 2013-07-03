angular.module("umbraco.directives")
.directive('sectionIcon', function ($compile) {
    return {
        restrict: 'E',
        replace: true,

        link: function (scope, element, attrs) {

            var icon = attrs.icon;
            
            if(icon.startsWith(".")) {
                element.html("<i class='" + icon.substr(1) + "'></i>");
            }else {
                element.html("<img src='images/tray/" + icon + "'>");
            }
        }
    };
});