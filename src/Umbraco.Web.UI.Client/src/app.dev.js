var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.httpbackend',
    'ngCookies',
    'ngSanitize',
    'tmh.dynamicLocale',
    'hmTouchEvents'
]);

/* For Angular 1.4: we need to load in Route, animate and touch seperately
	    'ngRoute',
	    'ngAnimate',
	    'ngTouch'
*/
