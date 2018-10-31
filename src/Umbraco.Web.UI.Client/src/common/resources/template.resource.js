/**
    * @ngdoc service
    * @name umbraco.resources.templateResource
    * @description Loads in data for templates
    **/
function templateResource($q, $http, umbDataFormatter, umbRequestHelper, localizationService) {

    return {

        /**
         * @ngdoc method
         * @name umbraco.resources.templateResource#getById
         * @methodOf umbraco.resources.templateResource
         *
         * @description
         * Gets a template item with a given id
         *
         * ##usage
         * <pre>
         * templateResource.getById(1234)
         *    .then(function(template) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {Int} id id of template to retrieve
         * @returns {Promise} resourcePromise object.
         *
         */
        getById: function (id) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "templateApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               "Failed to retrieve data for template id " + id);
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
         * @name umbraco.resources.templateResource#getAll
         * @methodOf umbraco.resources.templateResource
         *
         * @description
         * Gets all templates
         *
         * ##usage
         * <pre>
         * templateResource.getAll()
         *    .then(function(templates) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @returns {Promise} resourcePromise object.
         *
         */
        getAll: function () {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "templateApiBaseUrl",
                       "GetAll")),
               "Failed to retrieve data");
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
                        [{ id: id }] )),
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
        deleteById: function(id) {

            var promise = localizationService.localize("template_deleteByIdFailed", [id]);

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "templateApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                promise);
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
        save: function (template) {
            return umbRequestHelper.resourcePromise(
                 $http.post(
                     umbRequestHelper.getApiUrl(
                         "templateApiBaseUrl", 
                         "PostSave"), 
                         template),
                "Failed to save data for template id " + template.id);
        }
    };
}

angular.module("umbraco.resources").factory("templateResource", templateResource);
