var app = angular.module('umbraco', [
	'umbraco.filters',
	'umbraco.directives',
	'umbraco.resources',
	'umbraco.services',
	'umbraco.packages',
	'umbraco.views',

    'ngRoute',
	'ngAnimate',
    'ngCookies',
    'ngSanitize',
    'ngTouch',
    'ngMessages',
    'ngAria',
    'tmh.dynamicLocale',
    'ngFileUpload',
    'LocalStorageModule',
    'chart.js'
]);

app.config(['$compileProvider', function ($compileProvider) {
    // when not in debug mode remove all angularjs debug css classes and  HTML comments from the dom
    $compileProvider.debugInfoEnabled(Umbraco.Sys.ServerVariables.isDebuggingEnabled);
    // don't execute directives inside comments
    $compileProvider.commentDirectivesEnabled(false);
    // don't execute directives inside css classes
    $compileProvider.cssClassDirectivesEnabled(false);
}]);

// I configure the $animate service during bootstrap.
angular.module("umbraco").config(
    function configureAnimate( $animateProvider ) {
        // By default, the $animate service will check for animation styling
        // on every structural change. This requires a lot of animateFrame-based
        // DOM-inspection. However, we can tell $animate to only check for
        // animations on elements that have a specific class name RegExp pattern
        // present. In this case, we are requiring the "umb-animated" class.
        $animateProvider.classNameFilter( /\bumb-animated\b/ );
    }
);

var packages = angular.module("umbraco.packages", []);

//this ensures we can inject our own views into templateCache and clear
//the entire cache before the app runs, due to the module
//order, clearing will always happen before umbraco.views and umbraco
//module is initilized.
angular.module("umbraco.views", ["umbraco.viewcache"]);
angular.module("umbraco.viewcache", [])
    .run(function ($rootScope, $templateCache, localStorageService) {
        /** For debug mode, always clear template cache to cut down on
            dev frustration and chrome cache on templates */
        if (Umbraco.Sys.ServerVariables.isDebuggingEnabled) {
            $templateCache.removeAll();
        }
        else {
            var storedVersion = localStorageService.get("umbVersion");
            if (!storedVersion || storedVersion !== Umbraco.Sys.ServerVariables.application.cacheBuster) {
                //if the stored version doesn't match our cache bust version, clear the template cache
                $templateCache.removeAll();
                //store the current version
                localStorageService.set("umbVersion", Umbraco.Sys.ServerVariables.application.cacheBuster);
            }
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
if (_.isFunction(document.angularReady)) {
    document.angularReady.apply(this, [app]);
}
