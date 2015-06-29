var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.mocks',
	'umbraco.security',
    'ngCookies'
]);

/* For Angular 1.2: we need to load in Routing separately
	    'ngRoute'
*/