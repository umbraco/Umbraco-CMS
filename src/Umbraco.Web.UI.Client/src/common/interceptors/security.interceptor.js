(function () {
    'use strict';

    /**
     * This http interceptor listens for authentication successes and failures
     * @param {any} $q
     * @param {any} $injector
     * @param {any} requestRetryQueue
     * @param {any} notificationsService
     * @param {any} eventsService
     * @param {any} requestInterceptorFilter
     */
    function securityInterceptor($q, $injector, requestRetryQueue, notificationsService, eventsService, requestInterceptorFilter) {
        return {

            'response': function (response) {
                // Intercept successful requests
                //Here we'll check if our custom header is in the response which indicates how many seconds the user's session has before it
                //expires. Then we'll update the user in the user service accordingly.
                var headers = response.headers();

                if (headers["x-umb-user-seconds"]) {
                    // We must use $injector to get the $http service to prevent circular dependency
                    var userService = $injector.get('userService');
                    userService.setUserTimeout(headers["x-umb-user-seconds"]);
                }

                //this checks if the user's values have changed, in which case we need to update the user details throughout
                //the back office similar to how we do when a user logs in
                if (headers["x-umb-user-modified"]) {
                    eventsService.emit("app.userRefresh");
                }

                return response;
            },

            'responseError': function (rejection) {

                // Intercept failed requests
                // Make sure we have the configuration of the request (don't we always?)
                var config = rejection.config ? rejection.config : {};

                // Make sure we have an object for the headers of the request
                var headers = config.headers ? config.headers : {};

                //Here we'll check if we should ignore the error (either based on the original header set or the request configuration)
                if (headers["x-umb-ignore-error"] === "ignore" || config.umbIgnoreErrors === true || (Utilities.isArray(config.umbIgnoreStatus) && config.umbIgnoreStatus.indexOf(rejection.status) !== -1)) {
                    //exit/ignore
                    return $q.reject(rejection);
                }

                if (config.url) {
                    var filtered = _.find(requestInterceptorFilter(), function (val) {
                        return config.url.indexOf(val) > 0;
                    });
                    if (filtered) {
                        return $q.reject(rejection);
                    }
                }

                //A 401 means that the user is not logged in
                if (rejection.status === 401) {
                    //avoid an infinite loop
                    var umbRequestHelper = $injector.get('umbRequestHelper');
                    var getCurrentUserPath = umbRequestHelper.getApiUrl("authenticationApiBaseUrl", "GetCurrentUser");
                    if (!rejection.config.url.endsWith(getCurrentUserPath)) {

                        var userService = $injector.get('userService'); // see above

                        //Associate the user name with the retry to ensure we retry for the right user
                        return userService.getCurrentUser()
                            .then(function(user) {
                                var userName = user ? user.name : null;
                                //The request bounced because it was not authorized - add a new request to the retry queue
                                return requestRetryQueue.pushRetryFn('unauthorized-server',
                                    userName,
                                    function retryRequest() {
                                        // We must use $injector to get the $http service to prevent circular dependency
                                        return $injector.get('$http')(rejection.config);
                                    });
                            });
                    }
                }
                else if (rejection.status === 404) {

                    //a 404 indicates that the request was not found - this could be due to a non existing url, or it could
                    //be due to accessing a url with a parameter that doesn't exist, either way we should notifiy the user about it

                    var errMsg = "The URL returned a 404 (not found): <br/><i>" + rejection.config.url.split('?')[0] + "</i>";
                    if (rejection.data && rejection.data.ExceptionMessage) {
                        errMsg += "<br/> with error: <br/><i>" + rejection.data.ExceptionMessage + "</i>";
                    }
                    
                    notificationsService.error(
                        "Request error",
                        errMsg);

                }
                else if (rejection.status === 403) {
                    //if the status was a 403 it means the user didn't have permission to do what the request was trying to do.
                    //How do we deal with this now, need to tell the user somehow that they don't have permission to do the thing that was
                    //requested. We can either deal with this globally here, or we can deal with it globally for individual requests on the umbRequestHelper,
                    // or completely custom for services calling resources.

                    //http://issues.umbraco.org/issue/U4-2749

                    //It was decided to just put these messages into the normal status messages.

                    var msg = "Unauthorized access to URL: <br/><i>" + rejection.config.url.split('?')[0] + "</i><br/>Contact your administrator for information.";
                    
                    notificationsService.error("Authorization error", msg);
                }

                return $q.reject(rejection);
            }
        };
    }

    angular.module('umbraco.interceptors').factory('securityInterceptor', securityInterceptor);


})();
