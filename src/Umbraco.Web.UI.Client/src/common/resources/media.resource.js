/**
 * @ngdoc service
 * @name umbraco.resources.mediaResource
 * @description Loads in data for media
 **/
function mediaResource($q, $http, umbDataFormatter, umbRequestHelper, trackedReferencesResource) {

    /** internal method process the saving of data and post processing the result */
    function saveMediaItem(content, action, files) {
        return umbRequestHelper.postSaveContent({
            restApiUrl: umbRequestHelper.getApiUrl(
                "mediaApiBaseUrl",
                "PostSave"),
            content: content,
            action: action,
            files: files,
            dataFormatter: function (c, a) {
                return umbDataFormatter.formatMediaPostData(c, a);
            }
        });
    }

    return {

        getRecycleBin: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "GetRecycleBin")),
                'Failed to retrieve data for media recycle bin');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#sort
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Sorts all children below a given parent node id, based on a collection of node-ids
         *
         * ##usage
         * <pre>
         * var ids = [123,34533,2334,23434];
         * mediaResource.sort({ sortedIds: ids })
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
                $http.post(umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostSort"),
                    {
                        parentId: args.parentId,
                        idSortOrder: args.sortedIds
                    }),
                'Failed to sort media');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#move
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Moves a node underneath a new parentId
         *
         * ##usage
         * <pre>
         * mediaResource.move({ parentId: 1244, id: 123 })
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
                $http.post(umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostMove"),
                    {
                        parentId: args.parentId,
                        id: args.id
                         }, {responseType: 'text'}),
                {
                    error: function(data){
                        var errorMsg = 'Failed to move media';
                        if (data.id !== undefined && data.parentId !== undefined) {
                            if (data.id === data.parentId) {
                                errorMsg = 'Media can\'t be moved into itself';
                            }
                        }
                        else if (data.notifications !== undefined) {
                            if (data.notifications.length > 0) {
                                if (data.notifications[0].header.length > 0) {
                                    errorMsg = data.notifications[0].header;
                                }
                                if (data.notifications[0].message.length > 0) {
                                    errorMsg = errorMsg + ": " + data.notifications[0].message;
                                }
                            }
                        }

                        return {
                            errorMsg: errorMsg
                        };
                    }
                });
        },


        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#getById
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Gets a media item with a given id
         *
         * ##usage
         * <pre>
         * mediaResource.getById(1234)
         *    .then(function(media) {
         *        var myMedia = media;
         *        alert('its here!');
         *    });
         * </pre>
         *
         * @param {Int} id id of media item to return
         * @returns {Promise} resourcePromise object containing the media item.
         *
         */
        getById: function (id) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "GetById",
                        [{ id: id }])),
                'Failed to retrieve data for media id ' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#deleteById
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Deletes a media item with a given id
         *
         * ##usage
         * <pre>
         * mediaResource.deleteById(1234)
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre>
         *
         * @param {Int} id id of media item to delete
         * @returns {Promise} resourcePromise object.
         *
         */
        deleteById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                'Failed to delete item ' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#getByIds
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Gets an array of media items, given a collection of ids
         *
         * ##usage
         * <pre>
         * mediaResource.getByIds( [1234,2526,28262])
         *    .then(function(mediaArray) {
         *        var myDoc = contentArray;
         *        alert('they are here!');
         *    });
         * </pre>
         *
         * @param {Array} ids ids of media items to return as an array
         * @returns {Promise} resourcePromise object containing the media items array.
         *
         */
        getByIds: function (ids) {

            var idQuery = "";
            ids.forEach(id => idQuery += `ids=${id}&`);

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "GetByIds",
                        idQuery)),
                'Failed to retrieve data for media ids ' + ids);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#getScaffold
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Returns a scaffold of an empty media item, given the id of the media item to place it underneath and the media type alias.
         *
         * - Parent Id must be provided so umbraco knows where to store the media
         * - Media Type alias must be provided so umbraco knows which properties to put on the media scaffold
         *
         * The scaffold is used to build editors for media that has not yet been populated with data.
         *
         * ##usage
         * <pre>
         * mediaResource.getScaffold(1234, 'folder')
         *    .then(function(scaffold) {
         *        var myDoc = scaffold;
         *        myDoc.name = "My new media item";
         *
         *        mediaResource.save(myDoc, true)
         *            .then(function(media){
         *                alert("Retrieved, updated and saved again");
         *            });
         *    });
         * </pre>
         *
         * @param {Int} parentId id of media item to return
         * @param {String} alias mediatype alias to base the scaffold on
         * @returns {Promise} resourcePromise object containing the media scaffold.
         *
         */
        getScaffold: function (parentId, alias) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "GetEmpty",
                        [{ contentTypeAlias: alias }, { parentId: parentId }])),
                'Failed to retrieve data for empty media item type ' + alias);

        },

        rootMedia: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "GetRootMedia")),
                'Failed to retrieve data for root media');

        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#getChildren
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Gets children of a media item with a given id
         *
         * ##usage
         * <pre>
         * mediaResource.getChildren(1234, {pageSize: 10, pageNumber: 2})
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
         * @returns {Promise} resourcePromise object containing an array of content items.
         *
         */
        getChildren: function (parentId, options) {

            var defaults = {
                pageSize: 0,
                pageNumber: 0,
                filter: '',
                orderDirection: "Ascending",
                orderBy: "SortOrder",
                orderBySystemField: true
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
                        "mediaApiBaseUrl",
                        "GetChildren",
                        [
                            { id: parentId },
                            { pageNumber: options.pageNumber },
                            { pageSize: options.pageSize },
                            { orderBy: options.orderBy },
                            { orderDirection: options.orderDirection },
                            { orderBySystemField: toBool(options.orderBySystemField) },
                            { filter: options.filter }
                        ])),
                'Failed to retrieve children for media item ' + parentId);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#save
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Saves changes made to a media item, if the media item is new, the isNew paramater must be passed to force creation
         * if the media item needs to have files attached, they must be provided as the files param and passed separately
         *
         *
         * ##usage
         * <pre>
         * mediaResource.getById(1234)
         *    .then(function(media) {
         *          media.name = "I want a new name!";
         *          mediaResource.save(media, false)
         *            .then(function(media){
         *                alert("Retrieved, updated and saved again");
         *            });
         *    });
         * </pre>
         *
         * @param {Object} media The media item object with changes applied
         * @param {Bool} isNew set to true to create a new item or to update an existing
         * @param {Array} files collection of files for the media item
         * @returns {Promise} resourcePromise object containing the saved media item.
         *
         */
        save: function (media, isNew, files) {
            return saveMediaItem(media, "save" + (isNew ? "New" : ""), files);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#addFolder
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Shorthand for adding a media item of the type "Folder" under a given parent ID
         *
         * ##usage
         * <pre>
         * mediaResource.addFolder("My gallery", 1234)
         *    .then(function(folder) {
         *        alert('New folder');
         *    });
         * </pre>
         *
         * @param {string} name Name of the folder to create
         * @param {int} parentId Id of the media item to create the folder underneath
         * @returns {Promise} resourcePromise object.
         *
         */
        addFolder: function (name, parentId) {
            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper
                        .getApiUrl("mediaApiBaseUrl", "PostAddFolder"),
                    {
                        name: name,
                        parentId: parentId
                    }),
                'Failed to add folder');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#getChildFolders
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Retrieves all media children with types used as folders.
         * Uses the convention of looking for media items with mediaTypes ending in
         * *Folder so will match "Folder", "bannerFolder", "secureFolder" etc,
         *
          * NOTE: This will return a page of max 500 folders, if more is required it needs to be paged
          *       and then folders are in the .items property of the returned promise data
         *
         * ##usage
         * <pre>
         * mediaResource.getChildFolders(1234)
          *    .then(function(page) {
         *        alert('folders');
         *    });
         * </pre>
         *
         * @param {int} parentId Id of the media item to query for child folders
         * @returns {Promise} resourcePromise object.
         *
         */
        getChildFolders: function (parentId) {
            if (!parentId) {
                parentId = -1;
            }

            //NOTE: This will return a max of 500 folders, if more is required it needs to be paged
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "GetChildFolders",
                        {
                                id: parentId,
                                pageNumber: 1,
                                pageSize: 500
                        })),
                        'Failed to retrieve child folders for media item ' + parentId);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#emptyRecycleBin
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Empties the media recycle bin
         *
         * ##usage
         * <pre>
         * mediaResource.emptyRecycleBin()
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
                        "mediaApiBaseUrl",
                        "EmptyRecycleBin")),
                'Failed to empty the recycle bin');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#search
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Paginated search for media items starting on the supplied nodeId
         *
         * ##usage
         * <pre>
         * mediaResource.search("my search", 1, 100, -1)
         *    .then(function(searchResult) {
         *        alert('it's here!');
         *    });
         * </pre>
         *
         * @param {string} query The search query
         * @param {int} pageNumber The page number
         * @param {int} pageSize The number of media items on a page
         * @param {int} searchFrom NodeId to search from (-1 for root)
         * @returns {Promise} resourcePromise object.
         *
         */
        search: function (query, pageNumber, pageSize, searchFrom) {

            var args = [
                { "query": query },
                { "pageNumber": pageNumber },
                { "pageSize": pageSize },
                { "searchFrom": searchFrom }
            ];

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "Search",
                        args)),
                'Failed to retrieve media items for search: ' + query);
        },

         /**
         * @ngdoc method
         * @name umbraco.resources.mediaResource#getPagedReferences
         * @methodOf umbraco.resources.mediaResource
         *
         * @description
         * Gets a page list of tracked references for the current item, so you can see where an item is being used
         *
         * @deprecated
         * Use umbraco.resources.trackedReferencesResource#getPagedReferences instead
         *
         * ##usage
         * <pre>
         * var options = {
         *      pageSize : 25,
         *      pageNumber : 1,
         *      entityType : 'DOCUMENT'
         *  };
         * trackedReferencesResource.getPagedReferences(1, options)
         *    .then(function(data) {
         *        console.log(data);
         *    });
         * </pre>
         *
         * @param {int} id Id of the item to query for tracked references
         * @param {Object} args optional args object
         * @param {Int} args.pageSize the pagesize of the returned list (default 25)
         * @param {Int} args.pageNumber the current page index (default 1)
         * @param {Int} args.entityType the type of tracked entity (default : DOCUMENT). Possible values DOCUMENT, MEDIA
         * @returns {Promise} resourcePromise object.
         *
         */
        getPagedReferences: function (id, options) {
            return trackedReferencesResource.getPagedReferences(id, options);
        }
    };
}

angular.module('umbraco.resources').factory('mediaResource', mediaResource);
