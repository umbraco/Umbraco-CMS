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
        getMacroParameters: function(macroId) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "macroRenderingApiBaseUrl",
                        "GetMacroParameters",
                        [{ macroId: macroId }])),
                'Failed to retrieve macro parameters for macro with id  ' + macroId);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.macroResource#getMacroResult
         * @methodOf umbraco.resources.macroResource
         *
         * @description
         * Gets the result of a macro as html to display in the rich text editor or in the Grid
         *
         * @param {int} macroId The macro id to get parameters for
         * @param {int} pageId The current page id
         * @param {Array} macroParamDictionary A dictionary of macro parameters
         *
         */
        getMacroResultAsHtmlForEditor: function(macroAlias, pageId, macroParamDictionary) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "macroRenderingApiBaseUrl",
                        "GetMacroResultAsHtmlForEditor"),
                    {
                        macroAlias: macroAlias,
                        pageId: pageId,
                        macroParams: macroParamDictionary
                    }),
                'Failed to retrieve macro result for macro with alias  ' + macroAlias);
        },

        /**
         *
         * @param {} filename
         * @returns {}
         */
        createPartialViewMacroWithFile: function(virtualPath, filename) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "macroRenderingApiBaseUrl",
                        "CreatePartialViewMacroWithFile"),
                    {
                        virtualPath: virtualPath,
                        filename: filename
                    }
                ),
                'Failed to create macro "' + filename + '"'
            );

        },

        createMacro: function(name) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "macroApiBaseUrl",
                        "Create?name=" + name)
                ),
                'Failed to create macro "' + name + '"'
            );
        },

        getPartialViews: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("macroApiBaseUrl", "GetPartialViews"),
                    "Failed to get partial views")
            );
        },

        getParameterEditors: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("macroApiBaseUrl", "GetParameterEditors"),
                    "Failed to get parameter editors")
            );
        },

        getGroupedParameterEditors: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("macroApiBaseUrl", "GetGroupedParameterEditors"),
                    "Failed to get parameter editors")
            );
        },

        getParameterEditorByAlias: function(alias) {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("macroApiBaseUrl", "GetParameterEditorByAlias", { "alias": alias }),
                    "Failed to get parameter editor")
            );
        },

        getById: function(id) {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("macroApiBaseUrl", "GetById", { "id": id }), "Failed to get macro")
            );
        },

        saveMacro: function(macro) {
            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("macroApiBaseUrl", "Save"), macro)
            );
        },

        deleteById: function(id) {
            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("macroApiBaseUrl", "deleteById", { "id": id }))
            );
        }
};
}

angular.module('umbraco.resources').factory('macroResource', macroResource);
