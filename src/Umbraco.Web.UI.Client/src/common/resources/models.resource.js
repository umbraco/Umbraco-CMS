function modelsResource($q, $http, umbRequestHelper) {

    // TODO - should use BackOfficeController to register urls? How can we extend it?
    // TODO - this shouldn't exist in core!!

    return {
        getModelsOutOfDateStatus: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    /*umbRequestHelper.getApiUrl(
                        "modelsApiBaseUrl",
                        "GetModelsOutOfDateStatus")*/ "/Umbraco/BackOffice/ModelsBuilder/ModelsBuilder/GetModelsOutOfDateStatus"),
                "Failed to get models out-of-date status");
        },

        buildModels: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    /*umbRequestHelper.getApiUrl(
                        "modelsApiBaseUrl",
                        "BuildModels")*/ "/Umbraco/BackOffice/ModelsBuilder/ModelsBuilder/BuildModels"),
                "Failed to build models");
        }
    };
}
angular.module("umbraco.resources").factory("modelsResource", modelsResource);
