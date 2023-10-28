angular.module('umbraco.interceptors', [])
    // We have to add the interceptor to the queue as a string because the interceptor depends upon service instances that are not available in the config block.
    .config(['$httpProvider', function ($httpProvider) {
        $httpProvider.defaults.xsrfHeaderName = 'X-UMB-XSRF-TOKEN';
        $httpProvider.defaults.xsrfCookieName = 'UMB-XSRF-TOKEN';

        $httpProvider.interceptors.push('securityInterceptor');

        $httpProvider.interceptors.push('requiredHeadersInterceptor');
        $httpProvider.interceptors.push('doNotPostDollarVariablesOnPostRequestInterceptor');
        $httpProvider.interceptors.push('cultureRequestInterceptor');

    }]);
