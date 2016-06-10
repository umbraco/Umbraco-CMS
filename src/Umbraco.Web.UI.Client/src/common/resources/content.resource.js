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
    function saveContentItem(content, action, files) {
        return umbRequestHelper.postSaveContent({
            restApiUrl: umbRequestHelper.getApiUrl(
                   "contentApiBaseUrl",
                   "PostSave"),
            content: content,
            action: action,
            files: files,
            dataFormatter: function (c, a) {
                return umbDataFormatter.formatContentPostData(c, a);
            }
        });
    }

    return {

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
                         }),
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
                         args),
                   'Failed to copy content');
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#unPublish
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Unpublishes a content item with a given Id
          *
          * ##usage
          * <pre>
          * contentResource.unPublish(1234)
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
        unPublish: function (id) {
            if (!id) {
                throw "id cannot be null";
            }

            return umbRequestHelper.resourcePromise(
                                    $http.post(
                                          umbRequestHelper.getApiUrl(
                                                "contentApiBaseUrl",
                                                "PostUnPublish",
                                                [{ id: id }])),
                                    'Failed to publish content with id ' + id);
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
          * @returns {Promise} resourcePromise object containing the content item.
          *
          */
        getById: function (id) {
            return umbRequestHelper.resourcePromise(
                  $http.get(
                        umbRequestHelper.getApiUrl(
                              "contentApiBaseUrl",
                              "GetById",
                              [{ id: id }])),
                  'Failed to retrieve data for content id ' + id);
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
            _.each(ids, function (item) {
                idQuery += "ids=" + item + "&";
            });

            return umbRequestHelper.resourcePromise(
                  $http.get(
                        umbRequestHelper.getApiUrl(
                              "contentApiBaseUrl",
                              "GetByIds",
                              idQuery)),
                  'Failed to retrieve data for content with multiple ids');
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
                              [{ contentTypeAlias: alias }, { parentId: parentId }])),
                  'Failed to retrieve data for empty content item type ' + alias);
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
                              "GetNiceUrl", [{ id: id }])),
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
            angular.extend(defaults, options);
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
                if (angular.isNumber(v)) {
                    return v > 0;
                }
                if (angular.isString(v)) {
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
                              [
                                    { id: parentId },
                                    { pageNumber: options.pageNumber },
                                    { pageSize: options.pageSize },
                                    { orderBy: options.orderBy },
                                    { orderDirection: options.orderDirection },
                                    { orderBySystemField: toBool(options.orderBySystemField) },
                                    { filter: options.filter }
                              ])),
                  'Failed to retrieve children for content item ' + parentId);
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#hasPermission
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Returns true/false given a permission char to check against a nodeID
          * for the current user
          *
          * ##usage
          * <pre>
          * contentResource.hasPermission('p',1234)
          *    .then(function() {
          *        alert('You are allowed to publish this item');
          *    });
          * </pre> 
          *
          * @param {String} permission char representing the permission to check
          * @param {Int} id id of content item to delete        
          * @returns {Promise} resourcePromise object.
          *
          */
        checkPermission: function (permission, id) {
            return umbRequestHelper.resourcePromise(
                   $http.get(
                         umbRequestHelper.getApiUrl(
                               "contentApiBaseUrl",
                               "HasPermission",
                               [{ permissionToCheck: permission }, { nodeId: id }])),
                   'Failed to check permission for item ' + id);
        },

        getPermissions: function (nodeIds) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "contentApiBaseUrl",
                        "GetPermissions"),
                    nodeIds),
                'Failed to get permissions');
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#save
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Saves changes made to a content item to its current version, if the content item is new, the isNew paramater must be passed to force creation
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
          * @returns {Promise} resourcePromise object containing the saved content item.
          *
          */
        save: function (content, isNew, files) {
            return saveContentItem(content, "save" + (isNew ? "New" : ""), files);
        },


        /**
          * @ngdoc method
          * @name umbraco.resources.contentResource#publish
          * @methodOf umbraco.resources.contentResource
          *
          * @description
          * Saves and publishes changes made to a content item to a new version, if the content item is new, the isNew paramater must be passed to force creation
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
          * @returns {Promise} resourcePromise object containing the saved content item.
          *
          */
        publish: function (content, isNew, files) {
            return saveContentItem(content, "publish" + (isNew ? "New" : ""), files);
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
        sendToPublish: function (content, isNew, files) {
            return saveContentItem(content, "sendPublish" + (isNew ? "New" : ""), files);
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

        }


    };
}

angular.module('umbraco.resources').factory('contentResource', contentResource);
