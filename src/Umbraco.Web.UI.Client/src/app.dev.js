var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.httpbackend',
    'ngCookies',
    'ngMobile',
    'ngSanitize',
    'tmh.dynamicLocale'
]);

/* For Angular 1.4: we need to load in Route, animate and touch seperately
	    'ngRoute',
	    'ngAnimate',
	    'ngTouch'
*/