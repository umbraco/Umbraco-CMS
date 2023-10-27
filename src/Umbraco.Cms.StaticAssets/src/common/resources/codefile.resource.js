/**
    * @ngdoc service
    * @name umbraco.resources.codefileResource
    * @description Loads in data for files that contain code such as js scripts, partial views and partial view macros
    **/
function codefileResource($q, $http, umbDataFormatter, umbRequestHelper, localizationService) {

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
         * codefileResource.getByPath('scripts', 'oooh-la-la.js')
         *    .then(function(codefile) {
         *        alert('its here!');
         *    });
         * </pre>
         * 
         * <pre>
         * codefileResource.getByPath('partialView', 'Grid%2fEditors%2fBase.cshtml')
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
         * @name umbraco.resources.codefileResource#getByAlias
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Gets a template item with a given alias
         *
         * ##usage
         * <pre>
         * codefileResource.getByAlias("upload")
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
         * @name umbraco.resources.codefileResource#deleteByPath
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Deletes a codefile with a given type & path
         *
         * ##usage
         * <pre>
         * codefileResource.deleteByPath('scripts', 'oooh-la-la.js')
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre>
         * 
         * <pre>
         * codefileResource.deleteByPath('partialViews', 'Grid%2fEditors%2fBase.cshtml')
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre>
         *
         * @param {type} the type of script (partialViews, partialViewMacros, scripts)
         * @param {virtualpath} the virtual path of the script
         * @returns {Promise} resourcePromise object.
         *
         */
        deleteByPath: function (type, virtualpath) {

            var promise = localizationService.localize("codefile_deleteItemFailed", [virtualpath]);

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "codeFileApiBaseUrl",
                        "Delete",
                        [{ type: type }, { virtualPath: virtualpath}])),
                promise);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.codefileResource#save
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Saves or update a codeFile
         * 
         * ##usage
         * <pre>
         * codefileResource.save(codeFile)
         *    .then(function(codeFile) {
         *        alert('its saved!');
         *    });
         * </pre>
         *
         * @param {Object} template object to save
         * @returns {Promise} resourcePromise object.
         *
         */
        save: function (codeFile) {
            return umbRequestHelper.resourcePromise(
                 $http.post(
                     umbRequestHelper.getApiUrl(
                         "codeFileApiBaseUrl",
                         "PostSave"),
                         codeFile),
                "Failed to save data for code file " + codeFile.virtualPath);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.codefileResource#getSnippets
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Gets code snippets for a given file type
         * 
         * ##usage
         * <pre>
         * codefileResource.getSnippets("partialViews")
         *    .then(function(snippets) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {string} file type: (partialViews, partialViewMacros)
         * @returns {Promise} resourcePromise object.
         *
         */
        getSnippets: function (fileType) {
            return umbRequestHelper.resourcePromise(
                 $http.get(
                     umbRequestHelper.getApiUrl(
                         "codeFileApiBaseUrl",
                         "GetSnippets?type=" + fileType )),
                "Failed to get snippet for" + fileType);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.codefileResource#getScaffold
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Returns a scaffold of an empty codefile item.
         * 
         * The scaffold is used to build editors for code file editors that has not yet been populated with data.
         * 
         * ##usage
         * <pre>
         * codefileResource.getScaffold("partialViews", "Breadcrumb")
         *    .then(function(data) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {string} File type: (scripts, partialViews, partialViewMacros).
         * @param {string} Snippet name (Ex. Breadcrumb).
         * @returns {Promise} resourcePromise object.
         *
         */

        getScaffold: function (type, id, snippetName) {

            var queryString = "?type=" + type + "&id=" + id;
            if (snippetName) {
                queryString += "&snippetName=" + snippetName;
            }

            return umbRequestHelper.resourcePromise(
                 $http.get(
                     umbRequestHelper.getApiUrl(
                        "codeFileApiBaseUrl",
                        "GetScaffold" + queryString)),
                "Failed to get scaffold for" + type);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.codefileResource#createContainer
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Creates a container/folder
         * 
         * ##usage
         * <pre>
         * codefileResource.createContainer("partialViews", "folder%2ffolder", "folder")
         *    .then(function(data) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {string} File type: (scripts, partialViews, partialViewMacros).
         * @param {string} Parent Id: url encoded path
         * @param {string} Container name
         * @returns {Promise} resourcePromise object.
         *
         */

        createContainer: function (type, parentId, name) {

            // Is the parent ID numeric?
            var key = "codefile_createFolderFailedBy" + (isNaN(parseInt(parentId)) ? "Name" : "Id");

            var promise = localizationService.localize(key, [parentId]);

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl(
                    "codeFileApiBaseUrl", 
                    "PostCreateContainer", 
                    { type: type, parentId: parentId, name: encodeURIComponent(name) })),
                promise);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.codefileResource#interpolateStylesheetRules
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Takes all rich text editor styling rules and turns them into css
         * 
         * ##usage
         * <pre>
         * codefileResource.interpolateStylesheetRules(".box{background:purple;}", "[{name: "heading", selector: "h1", styles: "color: red"}]")
         *    .then(function(data) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {string} content The style sheet content.
         * @param {string} rules The rich text editor rules
         * @returns {Promise} resourcePromise object.
         *
         */
        interpolateStylesheetRules: function (content, rules) {
            var payload = {
                content: content,
                rules: rules
            };
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "codeFileApiBaseUrl",
                        "PostInterpolateStylesheetRules"),
                    payload),
                "Failed to interpolate sheet rules");
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.codefileResource#extractStylesheetRules
         * @methodOf umbraco.resources.codefileResource
         *
         * @description
         * Find all rich text editor styles in the style sheets and turns them into "rules"
         * 
         * ##usage
         * <pre>
         * 
         * var conntent
         * codefileResource.extractStylesheetRules(".box{background:purple;}")
         *    .then(function(data) {
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {string} content The style sheet content.
         * @returns {Promise} resourcePromise object.
         *
         */
        extractStylesheetRules: function(content) {
            var payload = {
                content: content,
                rules: null
            };
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "codeFileApiBaseUrl",
                        "PostExtractStylesheetRules"),
                    payload),
                "Failed to extract style sheet rules");
        }

    };
}

angular.module("umbraco.resources").factory("codefileResource", codefileResource);
