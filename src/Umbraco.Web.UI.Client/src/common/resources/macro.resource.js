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
         * @param {Array} macroParamDictionary A dictionary of macro parameters
         *
         */
        getMacroResultAsHtmlForEditor: function (macroAlias, pageId, macroParamDictionary) {

            //need to format the query string for the custom dictionary
            var query = "macroAlias=" + macroAlias + "&pageId=" + pageId;
            if (macroParamDictionary) {
                var counter = 0;
                _.each(macroParamDictionary, function (val, key) {
                    //check for null
                    val = val ? val : "";
                    //need to detect if the val is a string or an object
                    if (!angular.isString(val)) {
                        //if it's not a string we'll send it through the json serializer
                        var json = angular.toJson(val);
                        //then we need to url encode it so that it's safe
                        val = encodeURIComponent(json);
                    }                    

                    query += "&macroParams[" + counter + "].key=" + key + "&macroParams[" + counter + "].value=" + val;
                    counter++;
                });
            }

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "macroApiBaseUrl",
                       "GetMacroResultAsHtmlForEditor",
                       query)),
               'Failed to retreive macro result for macro with alias  ' + macroAlias);
        }
            
    };
}

angular.module('umbraco.resources').factory('macroResource', macroResource);
