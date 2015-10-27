/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbNotifications
 */
function notificationDirective(notificationsService) {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-notifications.html',
        link: function (scope, element, attr, ctrl) {

            //subscribes to notifications in the notification service
            scope.notifications = notificationsService.current;
            scope.$watch('notificationsService.current', function (newVal, oldVal, scope) {
                if (newVal) {
                    scope.notifications = newVal;
                }
            });

        }
    };
}

angular.module('umbraco.directives').directive("umbNotifications", notificationDirective);