angular.module('umbraco.resources').factory('Umbraco.PropertyEditors.NestedContent.Resources',
    function ($q, $http, umbRequestHelper) {
        return {
            getContentTypes: function () {
                var url = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/UmbracoApi/NestedContent/GetContentTypes";
                return umbRequestHelper.resourcePromise(
                    $http.get(url),
                    'Failed to retrieve content types'
                );
            }
        };
    });