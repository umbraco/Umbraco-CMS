/**
* @ngdoc directive 
* @name umbraco.directive:notifications 
* @restrict E
**/
function notificationDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        template: '<div ng-include="notificationViewFile"></div>',
        link: function (scope, el, attrs) {
            //set the notificationViewFile
            scope.notificationViewFile = "views/directives/umb-notifications.html";
        }
    };
}

angular.module('umbraco.directives').directive("umbNotifications", notificationDirective);