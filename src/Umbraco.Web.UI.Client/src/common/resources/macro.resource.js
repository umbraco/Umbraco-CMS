/**
    * @ngdoc service
    * @name umbraco.resources.macroResource
    * @description Deals with data for macros
    * 
    **/
function macroResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.macroResource#getMacroParameters
         * @methodOf umbraco.resources.macroResource
         *
         * @description
         * Gets the editable macro parameters for the specified macro alias
         *
         * @param {int} macroId The macro id to get parameters for
         *
         */
        getMacroParameters: function (macroId) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "macroApiBaseUrl",
                       "GetMacroParameters",
                       [{ macroId: macroId }])),
               'Failed to retreive macro parameters for macro with id  ' + macroId);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.macroResource#getMacroResult
         * @methodOf umbraco.resources.macroResource
         *
         * @description
         * Gets the result of a macro as html to display in the rich text editor
         *
         * @param {int} macroId The macro id to get parameters for
         * @param {int} pageId The current page id
         *
         */
        getMacroResultAsHtmlForEditor: function (macroAlias, pageId) {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "macroApiBaseUrl",
                       "GetMacroResultAsHtmlForEditor",
                       [{ macroAlias: macroAlias }, { pageId: pageId }])),
               'Failed to retreive macro result for macro with alias  ' + macroAlias);
        }
            
    };
}

angular.module('umbraco.resources').factory('macroResource', macroResource);
