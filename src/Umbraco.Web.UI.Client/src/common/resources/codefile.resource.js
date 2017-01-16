/**
    * @ngdoc service
    * @name umbraco.resources.codefileResource
    * @description Loads in data for files that contain code such as js scripts, partial views and partial view macros
    **/
function codefileResource($q, $http, umbDataFormatter, umbRequestHelper) {

    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.codefileResource#getByPath
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Gets a codefile item with a given path
         *
         * ##usage
         * <pre>
         * codefileResource.getByPath('partialView', 'oooh-la-la')
         *    .then(function(codefile) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {type} the type of script (partialView, partialViewMacro, script)
         * @param {virtualpath} the virtual path of the script
         * @returns {Promise} resourcePromise object.
         *
         */
        getByPath: function (type, virtualpath) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "codeFileApiBaseUrl",
                       "GetByPath",
                       [{ type: type }, {virtualPath: virtualpath }])),
               "Failed to retrieve data for " + type + " from virtual path " + virtualpath);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.templateResource#getByAlias
         * @methodOf umbraco.resources.templateResource
         *
         * @description
         * Gets a template item with a given alias
         *
         * ##usage
         * <pre>
         * templateResource.getByAlias("upload")
         *    .then(function(template) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {String} alias Alias of template to retrieve
         * @returns {Promise} resourcePromise object.
         *
         */
        getByAlias: function (alias) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "templateApiBaseUrl",
                       "GetByAlias",
                       [{ alias: alias }])),
               "Failed to retrieve data for template with alias: " + alias);
        },


        /**
         * @ngdoc method
         * @name umbraco.resources.templateResource#getScaffold
         * @methodOf umbraco.resources.templateResource
         *
         * @description
         * Returns a scaffold of an empty template item
         *
         * The scaffold is used to build editors for templates that has not yet been populated with data.
         *
         * ##usage
         * <pre>
         * templateResource.getScaffold()
         *    .then(function(template) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @returns {Promise} resourcePromise object containing the template scaffold.
         *
         */
        getScaffold: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "templateApiBaseUrl",
                       "GetScaffold",
                        [{ id: id }])),
               "Failed to retrieve data for empty template");
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.templateResource#deleteById
         * @methodOf umbraco.resources.templateResource
         *
         * @description
         * Deletes a template with a given id
         *
         * ##usage
         * <pre>
         * templateResource.deleteById(1234)
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre>
         *
         * @param {Int} id id of template to delete
         * @returns {Promise} resourcePromise object.
         *
         */
        deleteByPath: function (type, virtualpath) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "codeFileApiBaseUrl",
                        "Delete",
                        [{ type: type }, { virtualPath: virtualpath}])),
                "Failed to delete item " + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.templateResource#save
         * @methodOf umbraco.resources.templateResource
         *
         * @description
         * Saves or update a template
         * 
         * ##usage
         * <pre>
         * templateResource.save(template)
         *    .then(function(template) {
         *        alert('its saved!');
         *    });
         * </pre>
         *
         * @param {Object} template object to save
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (partialView) {
            return umbRequestHelper.resourcePromise(
                 $http.post(
                     umbRequestHelper.getApiUrl(
                         "codeFileApiBaseUrl",
                         "PostSave"),
                         partialView),
                "Failed to save data for partialView " + partialView.virtualPath);
        }
    };
}

angular.module("umbraco.resources").factory("codefileResource", codefileResource);
