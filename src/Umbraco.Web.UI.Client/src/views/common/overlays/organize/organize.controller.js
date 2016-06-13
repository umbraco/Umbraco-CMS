(function() {
    "use strict";

    function OrganizeController(scope, umbRequestHelper, http) {
        var allTemplatesUrl = umbRequestHelper.getApiUrl("templateApiBaseUrl", "GetAll");

        scope.model.addRenderBody = false;
        scope.model.mandatoryRenderSection = false;
        scope.masterPage = {};

        http.get(allTemplatesUrl)
            .then(function(result) {
                scope.masterPages = result.data;
                scope.masterPages.splice(0,
                    0,
                    {
                        alias: null,
                        name: "None"
                    });
                scope.model.masterPage = $.grep(scope.masterPages,
                    function(mp) {
                        return mp.alias === scope.model.template.masterPageAlias;
                    })[0];
            });

        scope.selectMasterPage = function(masterPage) {
            scope.model.masterPage = masterPage;
        }
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