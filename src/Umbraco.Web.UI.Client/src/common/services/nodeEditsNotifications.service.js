/**
 @ngdoc service
 * @name umbraco.services.nodeEditsNotificationsService
 *
 * @description
 * 
 */
(function () {
    'use strict';

    function nodeEditsNotificationsService(notificationsService, localizationService) {

        var localizedLabels = {};

        function setCurrentEditNotification(edit) {
            var allOpenNotifications = notificationsService.getCurrent();
            //Make sure we dont get multiple of the same messages
            if (_.where(allOpenNotifications, { contentNotification: true, userId: edit.UserId }).length > 0) {
                return;
            }
            
            localizationService.localize("nodeEdits_userIsEditing", [edit.UserName]).then(function (value) {
                notificationsService
                    .add(
                    {
                        headline: localizedLabels.attention,
                        message: value,
                        type: 'warning',
                        sticky: true,
                        contentNotification: true,
                        userId: edit.UserId
                    });
            });

        };

        function setPublishedNotification(email, time) {
            localizationService.localize("nodeEdits_userPublished", [email, time]).then(function (value) {
                notificationsService
                    .add(
                    {
                        headline: localizedLabels.nodePublished,
                        message: value,
                        type: 'info',
                        sticky: false,
                        contentNotification: true
                    });
            });
        };

        function setReleasedNotification(edit) {
            this.removeByUserId(edit.UserId);

            localizationService.localize("nodeEdits_userStoppedEditing", [edit.UserName]).then(function (value) {
                notificationsService
                    .add(
                    {
                        headline: localizedLabels.released,
                        message: value,
                        type: 'info',
                        sticky: false
                    });
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

        localizationService.localizeMany([
            "nodeEdits_attention", "nodeEdits_released", "nodeEdits_nodePublished"
        ]).then(function (res) {
            localizedLabels.attention = res[0];
            localizedLabels.released = res[1];
            localizedLabels.nodePublished = res[2];
        });


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
