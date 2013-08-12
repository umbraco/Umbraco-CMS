// Based loosely around work by Witold Szczerba - https://github.com/witoldsz/angular-http-auth
angular.module('umbraco.security', [
  'umbraco.security.retryQueue',
  'umbraco.security.interceptor']);