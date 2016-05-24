angular.module('umbraco.security.interceptor').factory("requestInterceptor",
    ['$q', 'requestInterceptorFilter', function ($q, requestInterceptorFilter) {
            var requestInterceptor = {
                request: function (config) {

                    var filtered = _.find(requestInterceptorFilter(), function (val) {
                        return config.url.indexOf(val) > 0;
                    });
                    if (filtered) {
                        return config;
                    }

                    config.headers["Time-Offset"] = (new Date().getTimezoneOffset());
                    return config;
                }
            };

            return requestInterceptor;
        }
    ])
    // We have to add the interceptor to the queue as a string because the interceptor depends upon service instances that are not available in the config block.
    .config([
        '$httpProvider', function($httpProvider) {
            $httpProvider.interceptors.push('requestInterceptor');
        }
    ]);