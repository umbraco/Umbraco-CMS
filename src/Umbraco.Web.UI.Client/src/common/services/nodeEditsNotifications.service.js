/**
 @ngdoc service
 * @name umbraco.services.nodeEditsNotificationsService
 *
 * @description
 * 
 */
(function () {
    'use strict';

    function nodeEditsNotificationsService(notificationsService) {

        function setCurrentEditNotification(edit) {
            var allOpenNotifications = notificationsService.getCurrent();
            //Make sure we dont get multiple of the same messages
            if (_.where(allOpenNotifications, { contentNotification: true, userId: edit.UserId }).length > 0) {
                return;
            }
            notificationsService
                .add(
                {
                    headline: 'Attention',
                    message: edit.UserName + ' is currently editing this content.',
                    type: 'warning',
                    sticky: true,
                    contentNotification: true,
                    userId: edit.UserId
                });
        };

        function setPublishedNotification(email, time) {
            notificationsService
                .add(
                {
                    headline: 'This node was just published at ' + time + ' by ',
                    message: email,
                    type: 'warning',
                    sticky: false,
                    contentNotification: true
                });
        };

        function setReleasedNotification(edit) {
            this.removeByUserId(edit.UserId);

            notificationsService
                .add(
                {
                    headline: 'Released',
                    message: edit.UserName + ' stopped working on this item',
                    type: 'success',
                    sticky: false
                });
        };

        function removeByUserId(userId) {
            var allOpenNotifications = notificationsService.getCurrent();
            _.each(_.where(allOpenNotifications, { contentNotification: true, userId: userId }), function (el, index) {
                notificationsService.remove(index);
            });
        };

        function removeAll() {
            //Remove all old 'contentNotification' notifications
            var allOpenNotifications = notificationsService.getCurrent();
            _.each(_.where(allOpenNotifications, { contentNotification: true }), function (el, index) {
                notificationsService.remove(index);
            });
        }

        var service = {
            setCurrentEditNotification: setCurrentEditNotification,
            setPublishedNotification: setPublishedNotification,
            setReleasedNotification: setReleasedNotification,
            removeByUserId: removeByUserId,
            removeAll: removeAll
        };

        return service;
    };

    angular.module('umbraco.services').factory('nodeEditsNotificationsService', nodeEditsNotificationsService);

})();
