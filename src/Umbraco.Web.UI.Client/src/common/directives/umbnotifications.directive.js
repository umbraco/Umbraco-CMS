/**
* @ngdoc object 
* @name umbraco.directive:notifications 
* @restrict E
**/
function notificationDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-notifications.html'
    };
}

angular.module('umbraco.directives').directive("umbNotifications", notificationDirective);