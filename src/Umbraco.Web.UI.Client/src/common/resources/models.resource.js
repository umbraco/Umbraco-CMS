function modelsResource($q, $http, umbRequestHelper) {

    // fixme - should use BackOfficeController to register urls? How can we extend it?

    return {
        getModelsOutOfDateStatus: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    /*umbRequestHelper.getApiUrl(
                        "modelsApiBaseUrl",
                        "GetModelsOutOfDateStatus")*/ "/Umbraco/BackOffice/Zbu/ModelsBuilderApi/GetModelsOutOfDateStatus"),
                "Failed to get models out-of-date status");
        },

        buildModels: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    /*umbRequestHelper.getApiUrl(
                        "modelsApiBaseUrl",
                        "BuildModels")*/ "/Umbraco/BackOffice/Zbu/ModelsBuilderApi/BuildModels"),
                "Failed to build models");
        }
    };
}
angular.module("umbraco.resources").factory("modelsResource", modelsResource);
