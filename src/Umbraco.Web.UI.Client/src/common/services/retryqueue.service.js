(function () {
    'use strict';

    /**
     * A service normally used to recover from session expiry
     * @param {any} $q
     * @param {any} $log
     */
    function requestRetryQueue($q, $log) {

        var retryQueue = [];
        var retryUser = null;

        var service = {
            // The security service puts its own handler in here!
            onItemAddedCallbacks: [],

            hasMore: function () {
                return retryQueue.length > 0;
            },
            push: function (retryItem) {
                retryQueue.push(retryItem);
                // Call all the onItemAdded callbacks
                Utilities.forEach(service.onItemAddedCallbacks, cb => {
                    try {
                        cb(retryItem);
                    } catch (e) {
                        $log.error('requestRetryQueue.push(retryItem): callback threw an error' + e);
                    }
                });
            },
            pushRetryFn: function (reason, userName, retryFn) {
                // The reason parameter is optional
                if (arguments.length === 2) {
                    retryFn = userName;
                    userName = reason;
                    reason = undefined;
                }

                if ((retryUser && retryUser !== userName) || userName === null) {
                    throw new Error('invalid user');
                }

                retryUser = userName;

                // The deferred object that will be resolved or rejected by calling retry or cancel
                var deferred = $q.defer();
                var retryItem = {
                    reason: reason,
                    retry: function () {
                        // Wrap the result of the retryFn into a promise if it is not already
                        $q.when(retryFn()).then(function (value) {
                            // If it was successful then resolve our deferred
                            deferred.resolve(value);
                        }, function (value) {
                            // Othewise reject it
                            deferred.reject(value);
                        });
                    },
                    cancel: function () {
                        // Give up on retrying and reject our deferred
                        deferred.reject();
                    }
                };
                service.push(retryItem);
                return deferred.promise;
            },
            retryReason: function () {
                return service.hasMore() && retryQueue[0].reason;
            },
            cancelAll: function () {
                while (service.hasMore()) {
                    retryQueue.shift().cancel();
                }
                retryUser = null;
            },
            retryAll: function (userName) {

                if (retryUser == null) {
                    return;
                }

                if (retryUser !== userName) {
                    service.cancelAll();
                    return;
                }

                while (service.hasMore()) {
                    retryQueue.shift().retry();
                }
            }
        };
        return service;

    }
    angular.module('umbraco.services').factory('requestRetryQueue', requestRetryQueue);
})();
