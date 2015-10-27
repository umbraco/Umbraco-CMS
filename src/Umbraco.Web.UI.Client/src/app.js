var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.packages',
    'ngCookies',
    'ngSanitize',
    'ngMobile',
    'blueimp.fileupload',
    'tmh.dynamicLocale'
]);
var packages = angular.module("umbraco.packages", []);

//Call a document callback if defined, this is sort of a dodgy hack to 
// be able to configure angular values in the Default.cshtml
// view which is much easier to do that configuring values by injecting them in  the back office controller
// to follow through to the js initialization stuff
if (angular.isFunction(document.angularReady)) {
    document.angularReady.apply(this, [app]);
}