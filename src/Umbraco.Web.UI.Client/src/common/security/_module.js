//TODO: This is silly and unecessary to have a separate module for this
angular.module('umbraco.security.retryQueue', []);
angular.module('umbraco.security.interceptor', ['umbraco.security.retryQueue']);
angular.module('umbraco.security', ['umbraco.security.retryQueue', 'umbraco.security.interceptor']);