"##JsNoCache##"
yepnope({
    load: [
         'lib/jquery/jquery-2.0.3.min.js',
         'lib/angular/1.1.5/angular.min.js',
         'lib/underscore/underscore.js',
    ],
    complete: function () {
        yepnope({
            load: "##JsInitialize##",
            complete: function () {

                //we need to set the legacy UmbClientMgr path
                UmbClientMgr.setUmbracoPath('"##UmbracoPath##"');

                jQuery(document).ready(function () {
                    angular.bootstrap(document, ['umbraco']);
                });
            }
        });
    }
});