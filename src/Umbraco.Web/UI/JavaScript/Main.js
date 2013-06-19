yepnope({

    load: "##JsInitialize##",

    complete: function () {
        jQuery(document).ready(function () {
            angular.bootstrap(document, ['umbraco']);
        });

    }
});