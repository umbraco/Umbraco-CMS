angular.module('umbraco.security.interceptor', ['umbraco.security.retryQueue'])
    // This http interceptor listens for authentication successes and failures
    .factory('securityInterceptor', ['$injector', 'securityRetryQueue', 'notificationsService', 'requestInterceptorFilter', function ($injector, queue, notifications, requestInterceptorFilter) {
        return function(promise) {

            return promise.then(
                function(originalResponse) {
                    // Intercept successful requests
                    
                    //Here we'll check if our custom header is in the response which indicates how many seconds the user's session has before it
                    //expires. Then we'll update the user in the user service accordingly.
                    var headers = originalResponse.headers();
                    if (headers["x-umb-user-seconds"]) {
                        // We must use $injector to get the $http service to prevent circular dependency
                        var userService = $injector.get('userService');
                        userService.setUserTimeout(headers["x-umb-user-seconds"]);
                    }
                    
                    return promise;
                }, function(originalResponse) {
                    // Intercept failed requests
                    
                    //Here we'll check if we should ignore the error, this will be based on an original header set
                    var headers = originalResponse.config ? originalResponse.config.headers : {};
                    if (headers["x-umb-ignore-error"] === "ignore") {
                        //exit/ignore
                        return promise;
                    }
                    var filtered = _.find(requestInterceptorFilter(), function(val) {
                        return originalResponse.config.url.indexOf(val) > 0;
                    });
                    if (filtered) {
                        return promise;
                    }

                    //A 401 means that the user is not logged in
                    if (originalResponse.status === 401) {

                        // The request bounced because it was not authorized - add a new request to the retry queue
                        promise = queue.pushRetryFn('unauthorized-server', function retryRequest() {
                            // We must use $injector to get the $http service to prevent circular dependency
                            return $injector.get('$http')(originalResponse.config);
                        });
                    }
                    else if (originalResponse.status === 404) {

                        //a 404 indicates that the request was not found - this could be due to a non existing url, or it could
                        //be due to accessing a url with a parameter that doesn't exist, either way we should notifiy the user about it
                        
                        var errMsg = "The URL returned a 404 (not found): <br/><i>" + originalResponse.config.url.split('?')[0] + "</i>";
                        if (originalResponse.data && originalResponse.data.ExceptionMessage) {
                            errMsg += "<br/> with error: <br/><i>" + originalResponse.data.ExceptionMessage + "</i>";
                        }
                        if (originalResponse.config.data) {
                            errMsg += "<br/> with data: <br/><i>" + angular.toJson(originalResponse.config.data) + "</i><br/>Contact your administrator for information.";
                        }

                        notifications.error(
                            "Request error",
                            errMsg);
                        
                    }
                    else if (originalResponse.status === 403) {
                        //if the status was a 403 it means the user didn't have permission to do what the request was trying to do.
                        //How do we deal with this now, need to tell the user somehow that they don't have permission to do the thing that was 
                        //requested. We can either deal with this globally here, or we can deal with it globally for individual requests on the umbRequestHelper,
                        // or completely custom for services calling resources.

                        //http://issues.umbraco.org/issue/U4-2749

                        //It was decided to just put these messages into the normal status messages. 

                        var msg = "Unauthorized access to URL: <br/><i>" + originalResponse.config.url.split('?')[0] + "</i>";
                        if (originalResponse.config.data) {
                            msg += "<br/> with data: <br/><i>" + angular.toJson(originalResponse.config.data) + "</i><br/>Contact your administrator for information.";
                        }

                        notifications.error(
                            "Authorization error",
                            msg);
                    }

                    return promise;
                });
        };
    }])

    .value('requestInterceptorFilter', function() {
        return ["www.gravatar.com"];
    })

    // We have to add the interceptor to the queue as a string because the interceptor depends upon service instances that are not available in the config block.
    .config(['$httpProvider', function ($httpProvider) {
        $httpProvider.responseInterceptors.push('securityInterceptor');
    }]);