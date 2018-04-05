/**
 @ngdoc service
 * @name umbraco.services.nodeEditsService
 *
 * @description
 * Application-wide service for handling node edits.
 */
(function () {
    'use strict';

    function nodeEditsService($rootScope, $q) {
        var proxy = null;

        var start = function (user) {
            var deferred = $q.defer();

            //Creating proxy
            if (proxy == null) {
                proxy = $.connection.NodeEditsHub;

                proxy.client.broadcastPublished = function (nodeId, userId, email, time) {
                    var data = {};
                    data.NodeId = nodeId;
                    data.UserId = userId;
                    data.UserName = email;
                    data.Time = time;
                    $rootScope.$emit('broadcastPublished', data);
                };

                proxy.client.userDisconnected = function (userId, userName) {
                    var data = {};
                    data.userId = userId;
                    data.userName = userName;
                    $rootScope.$emit('broadcastUserDisconnected', data);
                };

                proxy.client.editsChanged = function (allEdits) {
                    $rootScope.$emit('broadcastEditsChanged', allEdits);
                };

                var userId = user.id,
                    userName = user.name;

                //Starting connection
                $.connection.hub.logging = true;
                $.connection.hub.start().done(function (connection) {
                    proxy.server.subscribe(userId, userName).then(function () {
                        deferred.resolve(connection.id);
                    }, function () {
                        deferred.reject();
                    });
                });

                $.connection.hub.reconnected(function () {
                    $rootScope.$emit('reconnected');
                });

                $.connection.hub.reconnecting(function () {
                    $rootScope.$emit('reconnecting');
                });

            }
            else {
                //We allready have the proxy, resolve directly
                deferred.resolve(proxy.connection.id);
            }
            return deferred.promise;
        };

        var tryClaimSingleNode = function (userId, userName, nodeId) {
            var deferred = $q.defer();

            if (proxy && proxy.server && proxy.connection.state === 1) {
                var data = {
                    userId: userId,
                    nodeId: nodeId,
                    userName: userName
                };
                proxy.server.claimSingleNode(data).then(function (data) {
                    deferred.resolve(data);
                });
            } else {
                deferred.resolve(null);
            }

            return deferred.promise;
        }

        var disconnect = function () {
            $.connection.hub.stop();
            proxy = null;
        }

        return {
            start: start,
            tryClaimSingleNode: tryClaimSingleNode,
            disconnect: disconnect
        };
    };
    angular.module("umbraco.services").factory("nodeEditsService", nodeEditsService);

})();
