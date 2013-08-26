var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
    'ngCookies',
    'ngMobile',
    
    'ui.sortable',
    'blueimp.fileupload'
]);


/* For Angular 1.2: we need to load in Route, animate and touch seperately
	    'ngRoute',
	    'ngAnimate',
	    'ngTouch'
*/