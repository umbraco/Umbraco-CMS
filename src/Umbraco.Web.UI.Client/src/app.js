var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.packages',
    'ngCookies',
    'ngMobile',
    'ngSanitize',
    'blueimp.fileupload'
]);

//seperate core and packages? 
var packages = angular.module("umbraco.packages", []);


/* For Angular 1.2: we need to load in Route, animate and touch seperately
	    'ngRoute',
	    'ngAnimate',
	    'ngTouch'
*/