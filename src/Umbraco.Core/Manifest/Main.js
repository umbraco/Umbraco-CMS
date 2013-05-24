require.config("##RequireJsConfig##");

require("##RequireJsInitialize##", function (angular, myApp) {

    //This function will be called when all the dependencies
    //listed above are loaded. Note that this function could
    //be called before the page is loaded.
    //This callback is optional.

    jQuery(document).ready(function() {
        angular.bootstrap(document, ['myApp']);
    });
});