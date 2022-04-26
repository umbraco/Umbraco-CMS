/**
 * @ngdoc service
 * @name umbraco.resources.contentResource
 * @description Handles all transactions of content data
 * from the angular application to the Umbraco database, using the Content WebApi controller
 *
 * all methods returns a resource promise async, so all operations won't complete untill .then() is completed.
 *
 * @requires $q
 * @requires $http
 * @requires umbDataFormatter
 * @requires umbRequestHelper
 *
 * ##usage
 * To use, simply inject the contentResource into any controller or service that needs it, and make
 * sure the umbraco.resources module is accesible - which it should be by default.
 *
 * <pre>
 *    contentResource.getById(1234)
 *          .then(function(data) {
 *              $scope.content = data;
  *          });
  * </pre>
 **/

function contentResource($q, $http, umbDataFormatter, umbRequestHelper) {

    /** internal method process the saving of data and post processing the result */
    function saveContentItem(content, action, files, restApiUrl, showNotifications) {

        return umbRequestHelper.postSaveContent({
            restApiUrl: restApiUrl,
            content: content,
            action: action,
            files: files,
            showNotifications: showNotifications,
            dataFormatter: function (c, a) {
                return umbDataFormatter.formatContentPostData(c, a);
            }
        });
    }

    return {

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#allowsCultureVariation
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Check whether any content types have culture variant enabled
        *
        * ##usage
        * <pre>
        * contentResource.allowsCultureVariation()
        *    .then(function() {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @returns {Promise} resourcePromise object.
        *
        */
        allowsCultureVariation: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "AllowsCultureVariation")),
                'Failed to retrieve variant content types');
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#savePermissions
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Save user group permissions for the content
        *
        * ##usage
        * <pre>
        * contentResource.savePermissions(saveModel)
        *    .then(function() {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {object} The object which contains the user group permissions for the content
        * @returns {Promise} resourcePromise object.
        *
        */
        savePermissions: function (saveModel) {
            if (!saveModel) {
                throw "saveModel cannot be null";
            }
            if (!saveModel.contentId) {
                throw "saveModel.contentId cannot be null";
            }
            if (!saveModel.permissions) {
                throw "saveModel.permissions cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentApiBaseUrl", "PostSaveUserGroupPermissions"),
                    saveModel),
                'Failed to save permissions');
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#getRecycleBin
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Get the recycle bin
        *
        * ##usage
        * <pre>
        * contentResource.getRecycleBin()
        *    .then(function() {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @returns {Promise} resourcePromise object.
        *
        */
        getRecycleBin: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetRecycleBin")),
                'Failed to retrieve data for content recycle bin');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#sort
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Sorts all children below a given parent node id, based on a collection of node-ids
         *
         * ##usage
         * <pre>
         * var ids = [123,34533,2334,23434];
         * contentResource.sort({ parentId: 1244, sortedIds: ids })
         *    .then(function() {
         *        $scope.complete = true;
         *    });
          * </pre>
         * @param {Object} args arguments object
         * @param {Int} args.parentId the ID of the parent node
         * @param {Array} options.sortedIds array of node IDs as they should be sorted
         * @returns {Promise} resourcePromise object.
         *
         */
        sort: function (args) {
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.parentId) {
                throw "args.parentId cannot be null";
            }
            if (!args.sortedIds) {
                throw "args.sortedIds cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentApiBaseUrl", "PostSort"),
                    {
                        parentId: args.parentId,
                        idSortOrder: args.sortedIds
                    }),
                'Failed to sort content');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#move
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Moves a node underneath a new parentId
         *
         * ##usage
         * <pre>
         * contentResource.move({ parentId: 1244, id: 123 })
         *    .then(function() {
         *        alert("node was moved");
         *    }, function(err){
          *      alert("node didnt move:" + err.data.Message);
         *    });
          * </pre>
         * @param {Object} args arguments object
         * @param {Int} args.idd the ID of the node to move
         * @param {Int} args.parentId the ID of the parent node to move to
         * @returns {Promise} resourcePromise object.
         *
         */
        move: function (args) {
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.parentId) {
                throw "args.parentId cannot be null";
            }
            if (!args.id) {
                throw "args.id cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentApiBaseUrl", "PostMove"),
                    {
                        parentId: args.parentId,
                        id: args.id
                    }, { responseType: 'text' }),
                'Failed to move content');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#copy
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Copies a node underneath a new parentId
         *
         * ##usage
         * <pre>
         * contentResource.copy({ parentId: 1244, id: 123 })
         *    .then(function() {
         *        alert("node was copied");
         *    }, function(err){
          *      alert("node wasnt copy:" + err.data.Message);
         *    });
          * </pre>
         * @param {Object} args arguments object
         * @param {Int} args.id the ID of the node to copy
         * @param {Int} args.parentId the ID of the parent node to copy to
         * @param {Boolean} args.relateToOriginal if true, relates the copy to the original through the relation api
         * @returns {Promise} resourcePromise object.
         *
         */
        copy: function (args) {
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.parentId) {
                throw "args.parentId cannot be null";
            }
            if (!args.id) {
                throw "args.id cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("contentApiBaseUrl", "PostCopy"),
                    args, { responseType: 'text' }),
                'Failed to copy content');
        },

        /**
         * @ngdoc method
          * @name umbraco.resources.contentResource#unpublish
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Unpublishes a content item with a given Id
         *
         * ##usage
         * <pre>
          * contentResource.unpublish(1234)
         *    .then(function() {
         *        alert("node was unpulished");
         *    }, function(err){
          *      alert("node wasnt unpublished:" + err.data.Message);
         *    });
          * </pre>
         * @param {Int} id the ID of the node to unpublish
         * @returns {Promise} resourcePromise object.
         *
         */
        unpublish: function (id, cultures) {
            if (!id) {
                throw "id cannot be null";
            }

            if (!cultures) {
                cultures = [];
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "PostUnpublish"), { id: id, cultures: cultures }),
                'Failed to publish content with id ' + id);
        },
        /**
         * @ngdoc method
          * @name umbraco.resources.contentResource#getCultureAndDomains
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Gets the culture and hostnames for a content item with the given Id
          *
          * ##usage
          * <pre>
          * contentResource.getCultureAndDomains(1234)
          *    .then(function(data) {
          *        alert(data.Domains, data.Language);
          *    });
          * </pre>
          * @param {Int} id the ID of the node to get the culture and domains for.
          * @returns {Promise} resourcePromise object.
          *
          */
        getCultureAndDomains: function (id) {
            if (!id) {
                throw "id cannot be null";
            }
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetCultureAndDomains", { id: id })),
                'Failed to retreive culture and hostnames for ' + id);
        },
        saveLanguageAndDomains: function (model) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "PostSaveLanguageAndDomains"),
                        model));
        },
        /**
          * @ngdoc method
         * @name umbraco.resources.contentResource#emptyRecycleBin
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Empties the content recycle bin
         *
         * ##usage
         * <pre>
         * contentResource.emptyRecycleBin()
         *    .then(function() {
         *        alert('its empty!');
         *    });
          * </pre>
          *
         * @returns {Promise} resourcePromise object.
         *
         */
        emptyRecycleBin: function () {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "EmptyRecycleBin")),
                'Failed to empty the recycle bin');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#deleteById
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Deletes a content item with a given id
         *
         * ##usage
         * <pre>
         * contentResource.deleteById(1234)
         *    .then(function() {
         *        alert('its gone!');
         *    });
          * </pre>
          *
          * @param {Int} id id of content item to delete
         * @returns {Promise} resourcePromise object.
         *
         */
        deleteById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                'Failed to delete item ' + id);
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#deleteBlueprint
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Deletes a content blueprint item with a given id
        *
        * ##usage
        * <pre>
        * contentResource.deleteBlueprint(1234)
        *    .then(function() {
        *        alert('its gone!');
        *    });
        * </pre>
        *
        * @param {Int} id id of content blueprint item to delete
        * @returns {Promise} resourcePromise object.
        *
        */
        deleteBlueprint: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "DeleteBlueprint",
                        [{ id: id }])),
                'Failed to delete blueprint ' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#getById
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Gets a content item with a given id
         *
         * ##usage
         * <pre>
         * contentResource.getById(1234)
         *    .then(function(content) {
          *        var myDoc = content;
         *        alert('its here!');
         *    });
          * </pre>
          *
          * @param {Int} id id of content item to return
          * @param {Int} culture optional culture to retrieve the item in
         * @returns {Promise} resourcePromise object containing the content item.
         *
         */
        getById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetById",
                        { id: id })),
                'Failed to retrieve data for content id ' + id)
                .then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#getBlueprintById
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Gets a content blueprint item with a given id
        *
        * ##usage
        * <pre>
        * contentResource.getBlueprintById(1234)
        *    .then(function() {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {Int} id id of content blueprint item to retrieve
        * @returns {Promise} resourcePromise object.
        *
        */
        getBlueprintById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetBlueprintById",
                        { id: id })),
                'Failed to retrieve data for content id ' + id)
                .then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#getNotifySettingsById
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Gets notification options for a content item with a given id for the current user
        *
        * ##usage
        * <pre>
        * contentResource.getNotifySettingsById(1234)
        *    .then(function() {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {Int} id id of content item
        * @returns {Promise} resourcePromise object.
        *
        */
        getNotifySettingsById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetNotificationOptions",
                        { contentId: id })),
                'Failed to retrieve data for content id ' + id);
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#getNotifySettingsById
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Sets notification settings for a content item with a given id for the current user
        *
        * ##usage
        * <pre>
        * contentResource.setNotifySettingsById(1234,["D", "F", "H"])
        *    .then(function() {
        *       Do stuff...
        *    });
        * </pre>
        *
        * @param {Int} id id of content item
        * @param {Array} options the notification options to set for the content item
        * @returns {Promise} resourcePromise object.
        *
        */
        setNotifySettingsById: function (id, options) {
            if (!id) {
                throw "contentId cannot be null";
            }
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "PostNotificationOptions",
                        { contentId: id, notifyOptions: options })),
                'Failed to set notify settings for content id ' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#getByIds
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Gets an array of content items, given a collection of ids
         *
         * ##usage
         * <pre>
         * contentResource.getByIds( [1234,2526,28262])
         *    .then(function(contentArray) {
          *        var myDoc = contentArray;
         *        alert('they are here!');
         *    });
          * </pre>
          *
          * @param {Array} ids ids of content items to return as an array
         * @returns {Promise} resourcePromise object containing the content items array.
         *
         */
        getByIds: function (ids) {

            var idQuery = "";
            ids.forEach(id => idQuery += `ids=${id}&`);

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetByIds",
                        idQuery)),
                'Failed to retrieve data for content with multiple ids')
                .then(function (result) {
                    //each item needs to be re-formatted
                    result.forEach(r => umbDataFormatter.formatContentGetData(r));
                    return $q.when(result);
                });
        },


        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#getScaffold
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Returns a scaffold of an empty content item, given the id of the content item to place it underneath and the content type alias.
          *
         * - Parent Id must be provided so umbraco knows where to store the content
          * - Content Type alias must be provided so umbraco knows which properties to put on the content scaffold
          *
         * The scaffold is used to build editors for content that has not yet been populated with data.
          *
         * ##usage
         * <pre>
         * contentResource.getScaffold(1234, 'homepage')
         *    .then(function(scaffold) {
         *        var myDoc = scaffold;
          *        myDoc.name = "My new document";
         *
         *        contentResource.publish(myDoc, true)
         *            .then(function(content){
         *                alert("Retrieved, updated and published again");
         *            });
         *    });
          * </pre>
          *
         * @param {Int} parentId id of content item to return
          * @param {String} alias contenttype alias to base the scaffold on
         * @returns {Promise} resourcePromise object containing the content scaffold.
         *
         */
        getScaffold: function (parentId, alias) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetEmpty",
                        { contentTypeAlias: alias, parentId: parentId })),
                'Failed to retrieve data for empty content item type ' + alias)
                .then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
        },

        getScaffolds: function(parentId, aliases){
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetEmptyByAliases"),
                        { parentId: parentId, contentTypeAliases: aliases }
                    ),
                    'Failed to retrieve data for empty content item aliases ' + aliases.join(", ")
                ).then(function(result) {
                    Object.keys(result).map(function(key){
                        result[key] = umbDataFormatter.formatContentGetData(result[key]);
                    });

                    return $q.when(result);
                });
        },
        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#getScaffoldByKey
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Returns a scaffold of an empty content item, given the id of the content item to place it underneath and the content type alias.
          *
         * - Parent Id must be provided so umbraco knows where to store the content
          * - Content Type Id must be provided so umbraco knows which properties to put on the content scaffold
          *
         * The scaffold is used to build editors for content that has not yet been populated with data.
          *
         * ##usage
         * <pre>
         * contentResource.getScaffoldByKey(1234, '...')
         *    .then(function(scaffold) {
         *        var myDoc = scaffold;
          *        myDoc.name = "My new document";
         *
         *        contentResource.publish(myDoc, true)
         *            .then(function(content){
         *                alert("Retrieved, updated and published again");
         *            });
         *    });
          * </pre>
          *
         * @param {Int} parentId id of content item to return
          * @param {String} contentTypeGuid contenttype guid to base the scaffold on
         * @returns {Promise} resourcePromise object containing the content scaffold.
         *
         */
        getScaffoldByKey: function (parentId, contentTypeKey) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetEmptyByKey",
                        { contentTypeKey: contentTypeKey, parentId: parentId })),
                'Failed to retrieve data for empty content item id ' + contentTypeKey)
                .then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
        },

        getScaffoldByKeys: function (parentId, scaffoldKeys) {

            return umbRequestHelper.resourcePromise(
                    $http.post(
                        umbRequestHelper.getApiUrl(
                            "contentApiBaseUrl",
                            "GetEmptyByKeys"),
                        { contentTypeKeys: scaffoldKeys, parentId: parentId }
                    ),
                    'Failed to retrieve data for empty content items ids' + scaffoldKeys.join(", "))
                .then(function (result) {
                    Object.keys(result).map(function(key) {
                        result[key] = umbDataFormatter.formatContentGetData(result[key]);
                    });

                    return $q.when(result);
                });
        },

        getBlueprintScaffold: function (parentId, blueprintId) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetEmptyBlueprint",
                        { blueprintId: blueprintId, parentId: parentId })),
                'Failed to retrieve blueprint for id ' + blueprintId)
                .then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#getNiceUrl
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Returns a url, given a node ID
         *
         * ##usage
         * <pre>
         * contentResource.getNiceUrl(id)
         *    .then(function(url) {
         *        alert('its here!');
         *    });
          * </pre>
          *
         * @param {Int} id Id of node to return the public url to
         * @returns {Promise} resourcePromise object containing the url.
         *
         */
        getNiceUrl: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetNiceUrl", { id: id }),
                    { responseType: 'text' }),
                'Failed to retrieve url for id:' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#getChildren
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Gets children of a content item with a given id
         *
         * ##usage
         * <pre>
         * contentResource.getChildren(1234, {pageSize: 10, pageNumber: 2})
         *    .then(function(contentArray) {
          *        var children = contentArray;
         *        alert('they are here!');
         *    });
          * </pre>
          *
         * @param {Int} parentid id of content item to return children of
         * @param {Object} options optional options object
         * @param {Int} options.pageSize if paging data, number of nodes per page, default = 0
         * @param {Int} options.pageNumber if paging data, current page index, default = 0
         * @param {String} options.filter if provided, query will only return those with names matching the filter
         * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
         * @param {String} options.orderBy property to order items by, default: `SortOrder`
          * @param {String} options.cultureName if provided, the results will be for this specific culture/variant
         * @returns {Promise} resourcePromise object containing an array of content items.
         *
         */
        getChildren: function (parentId, options) {

            var defaults = {
                includeProperties: [],
                pageSize: 0,
                pageNumber: 0,
                filter: "",
                orderDirection: "Ascending",
                orderBy: "SortOrder",
                orderBySystemField: true,
                cultureName: ""
            };
            if (options === undefined) {
                options = {};
            }
            //overwrite the defaults if there are any specified
            Utilities.extend(defaults, options);
            //now copy back to the options we will use
            options = defaults;
            //change asc/desct
            if (options.orderDirection === "asc") {
                options.orderDirection = "Ascending";
            }
            else if (options.orderDirection === "desc") {
                options.orderDirection = "Descending";
            }

            //converts the value to a js bool
            function toBool(v) {
                if (Utilities.isNumber(v)) {
                    return v > 0;
                }
                if (Utilities.isString(v)) {
                    return v === "true";
                }
                if (typeof v === "boolean") {
                    return v;
                }
                return false;
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetChildren",
                        {
                            id: parentId,
                            includeProperties: _.pluck(options.includeProperties, 'alias').join(","),
                            pageNumber: options.pageNumber,
                            pageSize: options.pageSize,
                            orderBy: options.orderBy,
                            orderDirection: options.orderDirection,
                            orderBySystemField: toBool(options.orderBySystemField),
                            filter: options.filter,
                            cultureName: options.cultureName
                        })),
                'Failed to retrieve children for content item ' + parentId);
        },

        getDetailedPermissions: function (contentId) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetDetailedPermissions", { contentId: contentId })),
                'Failed to retrieve permissions for content item ' + contentId);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#save
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Saves changes made to a content item to its current version, if the content item is new, the isNew parameter must be passed to force creation
          * if the content item needs to have files attached, they must be provided as the files param and passed separately
          *
          *
         * ##usage
         * <pre>
         * contentResource.getById(1234)
         *    .then(function(content) {
         *          content.name = "I want a new name!";
         *          contentResource.save(content, false)
         *            .then(function(content){
         *                alert("Retrieved, updated and saved again");
         *            });
         *    });
          * </pre>
          *
         * @param {Object} content The content item object with changes applied
          * @param {Bool} isNew set to true to create a new item or to update an existing
          * @param {Array} files collection of files for the document
          * @param {Bool} showNotifications an option to disable/show notifications (default is true)
         * @returns {Promise} resourcePromise object containing the saved content item.
         *
         */
        save: function (content, isNew, files, showNotifications) {
            var endpoint = umbRequestHelper.getApiUrl(
                "contentApiBaseUrl",
                "PostSave");
            return saveContentItem(content, "save" + (isNew ? "New" : ""), files, endpoint, showNotifications);
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#saveBlueprint
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Saves changes made to a content blueprint item to its current version, if the content blueprint item is new, the isNew parameter must be passed to force creation
        * if the content item needs to have files attached, they must be provided as the files param and passed separately
        *
        * ##usage
        * <pre>
        * contentResource.getById(1234)
        *    .then(function(content) {
        *          content.name = "I want a new name!";
        *          contentResource.saveBlueprint(content, false)
        *            .then(function(content){
        *                alert("Retrieved, updated and saved again");
        *            });
        *    });
        * </pre>
        *
        * @param {Object} content The content blueprint item object with changes applied
        * @param {Bool} isNew set to true to create a new item or to update an existing
        * @param {Array} files collection of files for the document
        * @param {Bool} showNotifications an option to disable/show notifications (default is true)
        * @returns {Promise} resourcePromise object containing the saved content item.
        *
        */
        saveBlueprint: function (content, isNew, files, showNotifications) {
            var endpoint = umbRequestHelper.getApiUrl(
                "contentApiBaseUrl",
                "PostSaveBlueprint");
            return saveContentItem(content, "save" + (isNew ? "New" : ""), files, endpoint, showNotifications);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#publish
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Saves and publishes changes made to a content item to a new version, if the content item is new, the isNew parameter must be passed to force creation
          * if the content item needs to have files attached, they must be provided as the files param and passed separately
          *
          *
         * ##usage
         * <pre>
         * contentResource.getById(1234)
         *    .then(function(content) {
         *          content.name = "I want a new name, and be published!";
         *          contentResource.publish(content, false)
         *            .then(function(content){
         *                alert("Retrieved, updated and published again");
         *            });
         *    });
          * </pre>
          *
         * @param {Object} content The content item object with changes applied
          * @param {Bool} isNew set to true to create a new item or to update an existing
          * @param {Array} files collection of files for the document
          * @param {Bool} showNotifications an option to disable/show notifications (default is true)
         * @returns {Promise} resourcePromise object containing the saved content item.
         *
         */
        publish: function (content, isNew, files, showNotifications) {
            var endpoint = umbRequestHelper.getApiUrl(
                "contentApiBaseUrl",
                "PostSave");
            return saveContentItem(content, "publish" + (isNew ? "New" : ""), files, endpoint, showNotifications);
        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#publish
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Saves and publishes changes made to a content item and its descendants to a new version, if the content item is new, the isNew parameter must be passed to force creation
        * if the content items needs to have files attached, they must be provided as the files param and passed separately
        *
        *
        * ##usage
        * <pre>
        * contentResource.getById(1234)
        *    .then(function(content) {
        *          content.name = "I want a new name, and be published!";
        *          contentResource.publishWithDescendants(content, false)
        *            .then(function(content){
        *                alert("Retrieved, updated and published again");
        *            });
        *    });
        * </pre>
        *
        * @param {Object} content The content item object with changes applied
        * @param {Bool} isNew set to true to create a new item or to update an existing
        * @param {Array} files collection of files for the document
        * @param {Bool} showNotifications an option to disable/show notifications (default is true)
        * @returns {Promise} resourcePromise object containing the saved content item.
        *
        */
        publishWithDescendants: function (content, isNew, force, files, showNotifications) {
            var endpoint = umbRequestHelper.getApiUrl(
                "contentApiBaseUrl",
                "PostSave");

            var action = "publishWithDescendants";
            if (force === true) {
                action += "Force";
            }

            return saveContentItem(content, action + (isNew ? "New" : ""), files, endpoint, showNotifications);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#sendToPublish
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Saves changes made to a content item, and notifies any subscribers about a pending publication
          *
         * ##usage
         * <pre>
         * contentResource.getById(1234)
         *    .then(function(content) {
         *          content.name = "I want a new name, and be published!";
         *          contentResource.sendToPublish(content, false)
         *            .then(function(content){
         *                alert("Retrieved, updated and notication send off");
         *            });
         *    });
          * </pre>
          *
         * @param {Object} content The content item object with changes applied
          * @param {Bool} isNew set to true to create a new item or to update an existing
          * @param {Array} files collection of files for the document
         * @returns {Promise} resourcePromise object containing the saved content item.
         *
         */
        sendToPublish: function (content, isNew, files, showNotifications) {
            var endpoint = umbRequestHelper.getApiUrl(
                "contentApiBaseUrl",
                "PostSave");
            return saveContentItem(content, "sendPublish" + (isNew ? "New" : ""), files, endpoint, showNotifications);
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#saveSchedule
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Saves changes made to a content item, and saves the publishing schedule
          *
          * @param {Object} content The content item object with changes applied
          * @param {Bool} isNew set to true to create a new item or to update an existing
          * @param {Array} files collection of files for the document
          * @returns {Promise} resourcePromise object containing the saved content item.
          *
          */
        saveSchedule: function (content, isNew, files, showNotifications) {
            var endpoint = umbRequestHelper.getApiUrl(
                "contentApiBaseUrl",
                "PostSave");
            return saveContentItem(content, "schedule" + (isNew ? "New" : ""), files, endpoint, showNotifications);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.contentResource#publishByid
         * @methodOf umbraco.resources.contentResource
         *
         * @description
         * Publishes a content item with a given ID
          *
         * ##usage
         * <pre>
         * contentResource.publishById(1234)
         *    .then(function(content) {
         *        alert("published");
         *    });
          * </pre>
          *
         * @param {Int} id The ID of the conten to publish
         * @returns {Promise} resourcePromise object containing the published content item.
         *
         */
        publishById: function (id) {

            if (!id) {
                throw "id cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "PostPublishById",
                        [{ id: id }])),
                'Failed to publish content with id ' + id);

        },

        /**
        * @ngdoc method
        * @name umbraco.resources.contentResource#createBlueprintFromContent
        * @methodOf umbraco.resources.contentResource
        *
        * @description
        * Creates a content blueprint with a given name from a given content id
        *
        * ##usage
        * <pre>
        * contentResource.createBlueprintFromContent(1234,"name")
        *    .then(function(content) {
        *        alert("created");
        *    });
            * </pre>
            *
        * @param {Int} id The ID of the content to create the content blueprint from
        * @param {string} id The name of the content blueprint
        * @returns {Promise} resourcePromise object
        *
        */
        createBlueprintFromContent: function (contentId, name) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl("contentApiBaseUrl", "CreateBlueprintFromContent", {
                        contentId: contentId, name: name
                    })
                ),
                "Failed to create blueprint from content with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#getRollbackVersions
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Returns an array of previous version id's, given a node id and a culture
          *
          * ##usage
          * <pre>
          * contentResource.getRollbackVersions(id, culture)
          *    .then(function(versions) {
          *        alert('its here!');
          *    });
          * </pre>
          *
          * @param {Int} id Id of node
          * @param {Int} culture if provided, the results will be for this specific culture/variant
          * @returns {Promise} resourcePromise object containing the versions
          *
          */
        getRollbackVersions: function (contentId, culture) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl("contentApiBaseUrl", "GetRollbackVersions", {
                        contentId: contentId,
                        culture: culture
                    })
                ),
                "Failed to get rollback versions for content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#getRollbackVersion
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Returns a previous version of a content item
          *
          * ##usage
          * <pre>
          * contentResource.getRollbackVersion(versionId, culture)
          *    .then(function(version) {
          *        alert('its here!');
          *    });
          * </pre>
          *
          * @param {Int} versionId The version Id
          * @param {Int} culture if provided, the results will be for this specific culture/variant
          * @returns {Promise} resourcePromise object containing the version
          *
          */
        getRollbackVersion: function (versionId, culture) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl("contentApiBaseUrl", "GetRollbackVersion", {
                        versionId: versionId,
                        culture: culture
                    })
                ),
                "Failed to get version for content item with id " + versionId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#rollback
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Roll backs a content item to a previous version
          *
          * ##usage
          * <pre>
          * contentResource.rollback(contentId, versionId, culture)
          *    .then(function() {
          *        alert('its here!');
          *    });
          * </pre>
          *
          * @param {Int} id Id of node
          * @param {Int} versionId The version Id
          * @param {Int} culture if provided, the results will be for this specific culture/variant
          * @returns {Promise} resourcePromise object
          *
          */
        rollback: function (contentId, versionId, culture) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl("contentApiBaseUrl", "PostRollbackContent", {
                        contentId: contentId, versionId:versionId, culture:culture
                    })
                ),
                "Failed to roll back content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#getPublicAccess
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Returns the public access protection for a content item
          *
          * ##usage
          * <pre>
          * contentResource.getPublicAccess(contentId)
          *    .then(function(publicAccess) {
          *        // do your thing
          *    });
          * </pre>
          *
          * @param {Int} contentId The content Id
          * @returns {Promise} resourcePromise object containing the public access protection
          *
          */
        getPublicAccess: function (contentId) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl("publicAccessApiBaseUrl", "GetPublicAccess", {
                        contentId: contentId
                    })
                ),
                "Failed to get public access for content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#updatePublicAccess
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Sets or updates the public access protection for a content item
          *
          * ##usage
          * <pre>
          * contentResource.updatePublicAccess(contentId, userName, password, roles, loginPageId, errorPageId)
          *    .then(function() {
          *        // do your thing
          *    });
          * </pre>
          *
          * @param {Int} contentId The content Id
          * @param {Array} groups The names of the groups that should have access (if using group based protection)
          * @param {Array} usernames The usernames of the members that should have access (if using member based protection)
          * @param {Int} loginPageId The Id of the login page
          * @param {Int} errorPageId The Id of the error page
          * @returns {Promise} resourcePromise object containing the public access protection
          *
          */
        updatePublicAccess: function (contentId, groups, usernames, loginPageId, errorPageId) {
            var publicAccess = {
                contentId: contentId,
                loginPageId: loginPageId,
                errorPageId: errorPageId
            };
            if (Utilities.isArray(groups) && groups.length) {
                publicAccess.groups = groups;
            }
            else if (Utilities.isArray(usernames) && usernames.length) {
                publicAccess.usernames = usernames;
            }
            else {
                throw "must supply either userName/password or roles";
            }
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl("publicAccessApiBaseUrl", "PostPublicAccess", publicAccess)
                ),
                "Failed to update public access for content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#removePublicAccess
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Removes the public access protection for a content item
          *
          * ##usage
          * <pre>
          * contentResource.removePublicAccess(contentId)
          *    .then(function() {
          *        // do your thing
          *    });
          * </pre>
          *
          * @param {Int} contentId The content Id
          * @returns {Promise} resourcePromise object that's resolved once the public access has been removed
          *
          */
        removePublicAccess: function (contentId) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl("contentApiBaseUrl", "RemovePublicAccess", {
                        contentId: contentId
                    })
                ),
                "Failed to remove public access for content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#getPagedContentVersions
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Returns a paged array of previous version id's, given a node id, pageNumber, pageSize and a culture
          *
          * ##usage
          * <pre>
          * contentResource.getPagedContentVersions(id, pageNumber, pageSize, culture)
          *    .then(function(versions) {
          *        alert('its here!');
          *    });
          * </pre>
          *
          * @param {Int} id Id of node
          * @param {Int} pageNumber page number
          * @param {Int} pageSize page size
          * @param {Int} culture if provided, the results will be for this specific culture/variant
          * @returns {Promise} resourcePromise object containing the versions
          *
          */
        getPagedContentVersions: function (contentId, pageNumber, pageSize, culture) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl("contentApiBaseUrl", "GetPagedContentVersions", {
                        contentId: contentId,
                        pageNumber: pageNumber,
                        pageSize: pageSize,
                        culture: culture
                    })
                ),
                "Failed to get versions for content item with id " + contentId
            );
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#contentVersionPreventCleanup
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Enables or disabled clean up of a version
          *
          * ##usage
          * <pre>
          * contentResource.contentVersionPreventCleanup(contentId, versionId, preventCleanup)
          *    .then(function() {
          *        // do your thing
          *    });
          * </pre>
          *
          * @param {Int} contentId Id of node
          * @param {Int} versionId Id of version
          * @param {Int} preventCleanup Boolean to toggle clean up prevention
          *
          */
        contentVersionPreventCleanup: function (contentId, versionId, preventCleanup) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl("contentApiBaseUrl", "PostSetContentVersionPreventCleanup", {
                        contentId: contentId,
                        versionId: versionId,
                        preventCleanup: preventCleanup
                    })
                ),
                "Failed to toggle prevent cleanup of version with id " + versionId
            );
        }
    };
}

angular.module('umbraco.resources').factory('contentResource', contentResource);
