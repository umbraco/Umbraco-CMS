var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.mocks',
	'umbraco.interceptors',

    'ngRoute',
    'ngCookies',
    'LocalStorageModule'
]);
