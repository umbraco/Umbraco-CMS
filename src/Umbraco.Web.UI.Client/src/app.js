var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.packages',
	'umbraco.views',

    'ngCookies',
    'ngSanitize',
    'ngMobile',
    'tmh.dynamicLocale',
    'ngFileUpload',
	'LocalStorageModule'
]);

var packages = angular.module("umbraco.packages", []);

//this ensures we can inject our own views into templateCache and clear
//the entire cache before the app runs, due to the module
//order, clearing will always happen before umbraco.views and umbraco
//module is initilized.
angular.module("umbraco.views", ["umbraco.viewcache"]);
angular.module("umbraco.viewcache", [])
    .run(function($rootScope, $templateCache) {
        /** For debug mode, always clear template cache to cut down on
            dev frustration and chrome cache on templates */
        if (Umbraco.Sys.ServerVariables.isDebuggingEnabled) {
            $templateCache.removeAll();
        }
    })
    .config([
        //This ensures that all of our angular views are cache busted, if the path starts with views/ and ends with .html, then
        // we will append the cache busting value to it. This way all upgraded sites will not have to worry about browser cache.
        "$provide", function($provide) {
            return $provide.decorator("$http", [
                "$delegate", function($delegate) {
                    var get = $delegate.get;
                    $delegate.get = function (url, config) {

                        if (Umbraco.Sys.ServerVariables.application && url.startsWith("views/") && url.endsWith(".html")) {
                            var rnd = Umbraco.Sys.ServerVariables.application.cacheBuster;
                            var _op = (url.indexOf("?") > 0) ? "&" : "?";
                            url += _op + "umb__rnd=" + rnd;
                        }
                        
                        return get(url, config);
                    };
                    return $delegate;
                }
            ]);
        }
    ]);

//Call a document callback if defined, this is sort of a dodgy hack to
// be able to configure angular values in the Default.cshtml
// view which is much easier to do that configuring values by injecting them in  the back office controller
// to follow through to the js initialization stuff
if (angular.isFunction(document.angularReady)) {
    document.angularReady.apply(this, [app]);
}
