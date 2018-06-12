angular.module('umbraco.security.interceptor')
    // This http interceptor listens for authentication successes and failures
    .factory('securityInterceptor', ['$injector', 'securityRetryQueue', 'notificationsService', 'eventsService', 'requestInterceptorFilter', function ($injector, queue, notifications, eventsService, requestInterceptorFilter) {
        return {

            'request': function(config) {
                // do something on success
                // request for data - if we want to modify headers or url etc
                console.log('request config', config);
                return config;
            },

            'requestError': function(rejection) {
                // do something on error
               
                console.log('request error rejection', rejection);
                return $q.reject(rejection);
            },

            'response': function(response) {
                // do something on success
                console.log('response success', response);
                var headers = response.headers();
                console.log('response success headers', headers);

                /*
                // Intercept successful requests
                //Here we'll check if our custom header is in the response which indicates how many seconds the user's session has before it
                //expires. Then we'll update the user in the user service accordingly.
                var headers = originalResponse.headers();
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
                */

                return response;
            },
        
            'responseError': function(rejection) {

                // Intercept failed requests
                // Make sure we have the configuration of the request (don't we always?)
                var config = rejection.config ? rejection.config : {};

                // Make sure we have an object for the headers of the request
                var headers = config.headers ? config.headers : {};

                //Here we'll check if we should ignore the error (either based on the original header set or the request configuration)
                if (headers["x-umb-ignore-error"] === "ignore" || config.umbIgnoreErrors === true || (angular.isArray(config.umbIgnoreStatus) && config.umbIgnoreStatus.indexOf(rejection.status) !== -1)) {
                    //exit/ignore
                    return promise;
                }
                var filtered = _.find(requestInterceptorFilter(), function (val) {
                    return config.url.indexOf(val) > 0;
                });
                if (filtered) {
                    return promise;
                }

                //A 401 means that the user is not logged in
                if (rejection.status === 401 && !rejection.config.url.endsWith("umbraco/backoffice/UmbracoApi/Authentication/GetCurrentUser")) {

                    var userService = $injector.get('userService'); // see above

                    //Associate the user name with the retry to ensure we retry for the right user
                    promise = userService.getCurrentUser()
                        .then(function (user) {
                            var userName = user ? user.name : null;
                            //The request bounced because it was not authorized - add a new request to the retry queue
                            return queue.pushRetryFn('unauthorized-server', userName, function retryRequest() {
                                // We must use $injector to get the $http service to prevent circular dependency
                                return $injector.get('$http')(rejection.config);
                            });
                        });
                }
                else if (rejection.status === 404) {

                    //a 404 indicates that the request was not found - this could be due to a non existing url, or it could
                    //be due to accessing a url with a parameter that doesn't exist, either way we should notifiy the user about it

                    var errMsg = "The URL returned a 404 (not found): <br/><i>" + rejection.config.url.split('?')[0] + "</i>";
                    if (rejection.data && rejection.data.ExceptionMessage) {
                        errMsg += "<br/> with error: <br/><i>" + rejection.data.ExceptionMessage + "</i>";
                    }
                    if (rejection.config.data) {
                        errMsg += "<br/> with data: <br/><i>" + angular.toJson(rejection.config.data) + "</i><br/>Contact your administrator for information.";
                    }

                    notifications.error(
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

                    var msg = "Unauthorized access to URL: <br/><i>" + rejection.config.url.split('?')[0] + "</i>";
                    if (rejection.config.data) {
                        msg += "<br/> with data: <br/><i>" + angular.toJson(rejection.config.data) + "</i><br/>Contact your administrator for information.";
                    }

                    notifications.error("Authorization error", msg);
                }

                return $q.reject(rejection);
            }
        };
    }])
    //used to set headers on all requests where necessary
    .factory('umbracoRequestInterceptor', function ($q, urlHelper) {
        return {
            //dealing with requests:
            'request': function (config) {
                var queryStrings = urlHelper.getQueryStringParams();
                if (queryStrings.umbDebug === "true" || queryStrings.umbdebug === "true") {
                    config.headers["X-UMB-DEBUG"] = "true";
                }
                return config;
            }
        };
    })
    .value('requestInterceptorFilter', function () {
        return ["www.gravatar.com"];
    })

    // We have to add the interceptor to the queue as a string because the interceptor depends upon service instances that are not available in the config block.
    .config(['$httpProvider', function ($httpProvider) {
        $httpProvider.defaults.xsrfHeaderName = 'X-UMB-XSRF-TOKEN';
        $httpProvider.defaults.xsrfCookieName = 'UMB-XSRF-TOKEN';
        // Fixme: https://stackoverflow.com/questions/23804981/alternative-of-httpprovider-responseinterceptors
        $httpProvider.interceptors.push('securityInterceptor');
        $httpProvider.interceptors.push('umbracoRequestInterceptor');
    }]);
