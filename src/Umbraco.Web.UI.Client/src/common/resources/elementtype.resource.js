/**
    * @ngdoc service
    * @name umbraco.resources.elementTypeResource
    * @description Loads in data for element types
    **/
function elementTypeResource($q, $http, umbRequestHelper) {

    return {

        getAll: function () {

            var url = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/UmbracoApi/ElementType/GetAll";
            return umbRequestHelper.resourcePromise(
                $http.get(url),
                'Failed to retrieve element types'
            );
            
            /*
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "elementTypeApiBaseUrl",
                        "GetAll")),
                "Failed to retrieve data");
            */
        }

    };
}

angular.module("umbraco.resources").factory("elementTypeResource", elementTypeResource);
