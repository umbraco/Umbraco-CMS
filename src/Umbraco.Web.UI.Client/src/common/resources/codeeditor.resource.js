/**
    * @ngdoc service
    * @name umbraco.resources.codeEditorResource
    * @description Loads in data for Monaco code editor as auto completions for ModelsBuilder props etc
    **/
   function codeEditorResource($http, umbRequestHelper) {

    return {

        getModel: function (modelName) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "codeEditorApiBaseUrl",
                       "GetModel",
                       [{ modelName: modelName }])),
               `Failed to retrieve data for ${modelName}`);
        }

    };
}

angular.module("umbraco.resources").factory("codeEditorResource", codeEditorResource);
