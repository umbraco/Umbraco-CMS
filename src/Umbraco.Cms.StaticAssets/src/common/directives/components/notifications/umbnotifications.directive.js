/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbNotifications
 */

(function() {
   'use strict';

   function NotificationDirective(notificationsService) {

      function link(scope, el, attr, ctrl) {

         //subscribes to notifications in the notification service
         scope.notifications = notificationsService.current;
         scope.$watch('notificationsService.current', function (newVal, oldVal, scope) {
             if (newVal) {
                 scope.notifications = newVal;
             }
         });

      }

      var directive = {
         restrict: "E",
         replace: true,
         templateUrl: 'views/components/notifications/umb-notifications.html',
         link: link
      };

      return directive;

   }

   angular.module('umbraco.directives').directive('umbNotifications', NotificationDirective);

})();
