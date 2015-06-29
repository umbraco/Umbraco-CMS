var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.httpbackend',
    'ngCookies',
    'ngMobile',
    'ngSanitize',
    /*'ui.sortable',*/
    'blueimp.fileupload',
    'tmh.dynamicLocale'
]);

/* For Angular 1.2: we need to load in Route, animate and touch seperately
	    'ngRoute',
	    'ngAnimate',
	    'ngTouch'
*/