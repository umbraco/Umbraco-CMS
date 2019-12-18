LazyLoad.js("##JsInitialize##", function () {
    //we need to set the legacy UmbClientMgr path
    if ((typeof UmbClientMgr) !== "undefined") {
        UmbClientMgr.setUmbracoPath('"##UmbracoPath##"');
    }

    jQuery(document).ready(function () {

        angular.bootstrap(document, ['"##AngularModule##"']);

    });
});
