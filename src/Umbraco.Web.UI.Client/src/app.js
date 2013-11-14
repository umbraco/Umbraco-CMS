var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.packages',
    'ngCookies',
    'ngSanitize',
    'ngMobile',
    'blueimp.fileupload'
]);
var packages = angular.module("umbraco.packages", []);