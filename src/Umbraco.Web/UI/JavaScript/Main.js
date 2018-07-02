LazyLoad.js("##JsInitialize##", function () {
    //we need to set the legacy UmbClientMgr path
    UmbClientMgr.setUmbracoPath('"##UmbracoPath##"');

    jQuery(document).ready(function () {

        angular.bootstrap(document, ['umbraco']);

    });
});