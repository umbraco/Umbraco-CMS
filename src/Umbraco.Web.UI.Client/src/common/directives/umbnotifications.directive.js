/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbNotifications
 */
function notificationDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-notifications.html'
    };
}

angular.module('umbraco.directives').directive("umbNotifications", notificationDirective);