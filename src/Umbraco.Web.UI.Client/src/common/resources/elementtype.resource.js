/**
    * @ngdoc service
    * @name umbraco.resources.elementTypeResource
    * @description Loads in data for element types
    **/
function elementTypeResource($q, $http, umbRequestHelper) {

    return {

        getAll: function () {

            // TODO: Change this into a real api (ElementTypeApi). This is a temporary fix to get data.
                var url = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/UmbracoApi/NestedContent/GetContentTypes";
                return umbRequestHelper.resourcePromise(
                    $http.get(url),
                    'Failed to retrieve content types'
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
