(function() {
    "use strict";

    function OrganizeController(scope, umbRequestHelper, http) {
        var allTemplatesUrl = umbRequestHelper.getApiUrl("templateApiBaseUrl", "GetAll");

        http.get(allTemplatesUrl)
            .then(function(result) {
                scope.masterPages = result.data;
            });
    }

    angular.module("umbraco")
        .controller("Umbraco.Dialogs.Template.OrganizeController",
        [
            "$scope",
            "umbRequestHelper",
            "$http",
            OrganizeController
        ]);

}());