/**
    * @ngdoc service
    * @name umbraco.resources.entityResource
    * @description Loads in basic data for all entities
    * 
    * ##What is an entity?
    * An entity is a basic **read-only** representation of an Umbraco node. It contains only the most
    * basic properties used to display the item in trees, lists and navigation. 
    *
    * ##What is the difference between entity and content/media/etc...?
    * the entity only contains the basic node data, name, id and guid, whereas content
    * nodes fetched through the content service also contains additional all of the content property data, etc..
    * This is the same principal for all entity types. Any user that is logged in to the back office will have access
    * to view the basic entity information for all entities since the basic entity information does not contain sensitive information.
    *
    * ##Entity object types?
    * You need to specify the type of object you want returned.
    * 
    * The core object types are:
    *
    * - Document
    * - Media
    * - Member
    * - Template
    * - DocumentType
    * - MediaType
    * - MemberType
    * - Macro
    * - User
    * - Language
    * - Domain
    * - DataType
    **/
function entityResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
        
        getSafeAlias: function (value, camelCase) {

            if (!value) {
                return "";
            }
            value = value.replace("#", "");
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetSafeAlias", { value: value, camelCase: camelCase })),
               'Failed to retrieve content type scaffold');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getPath
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Returns a path, given a node ID and type
         *
         * ##usage
         * <pre>
         * entityResource.getPath(id, type)
         *    .then(function(pathArray) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id Id of node to return the public url to
         * @param {string} type Object type name     
         * @returns {Promise} resourcePromise object containing the url.
         *
         */
        getPath: function (id, type) {

            if (id === -1 || id === "-1") {
                return "-1";
            }

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetPath",
                       [{ id: id }, {type: type }])),
               'Failed to retrieve path for id:' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getUrl
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Returns a url, given a node ID and type
         *
         * ##usage
         * <pre>
         * entityResource.getUrl(id, type)
         *    .then(function(url) {
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id Id of node to return the public url to
         * @param {string} type Object type name
         * @returns {Promise} resourcePromise object containing the url.
         *
         */
        getUrl: function (id, type) {

            if (id === -1 || id === "-1") {
                return "";
            }

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetUrl",
                       [{ id: id }, {type: type }])),
               'Failed to retrieve url for id:' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getById
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an entity with a given id
         *
         * ##usage
         * <pre>
         * //get media by id
         * entityResource.getEntityById(0, "Media")
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of entity to return
         * @param {string} type Object type name        
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getById: function (id, type) {      

            if (id === -1 || id === "-1") {
                return null;
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "entityApiBaseUrl",
                        "GetById",
                        [{ id: id }, { type: type }])),
                'Failed to retrieve entity data for id ' + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getByIds
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of entities, given a collection of ids
         *
         * ##usage
         * <pre>
         * //Get templates for ids
         * entityResource.getEntitiesByIds( [1234,2526,28262], "Template")
         *    .then(function(templateArray) {
         *        var myDoc = contentArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {Array} ids ids of entities to return as an array
         * @param {string} type type name        
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        getByIds: function (ids, type) {
            
            var query = "type=" + type;

            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetByIds",
                       query),
                   {
                       ids: ids
                   }),
               'Failed to retrieve entity data for ids ' + ids);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getByQuery
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an entity from a given xpath
         *
         * ##usage
         * <pre>
         * //get content by xpath
         * entityResource.getByQuery("$current", -1, "Document")
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {string} query xpath to use in query
         * @param {Int} nodeContextId id id to start from
         * @param {string} type Object type name        
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getByQuery: function (query, nodeContextId, type) {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetByQuery",
                       [{ query: query }, { nodeContextId: nodeContextId }, { type: type }])),
               'Failed to retrieve entity data for query ' + query);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getAll
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an entity with a given id
         *
         * ##usage
         * <pre>
         *
         * //Only return media
         * entityResource.getAll("Media")
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {string} type Object type name        
         * @param {string} postFilter optional filter expression which will execute a dynamic where clause on the server
         * @param {string} postFilterParams optional parameters for the postFilter expression
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getAll: function (type, postFilter, postFilterParams) {            

            //need to build the query string manually
            var query = "type=" + type + "&postFilter=" + (postFilter ? postFilter : "");
            if (postFilter && postFilterParams) {
                var counter = 0;
                _.each(postFilterParams, function(val, key) {
                    query += "&postFilterParams[" + counter + "].key=" + key + "&postFilterParams[" + counter + "].value=" + val;
                    counter++;
                });
            } 

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetAll",
                       query)),
               'Failed to retrieve entity data for type ' + type);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getAncestors
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets ancestor entities for a given item
         *        
         * 
         * @param {string} type Object type name        
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getAncestors: function (id, type) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetAncestors",
                       [{id: id}, {type: type}])),
               'Failed to retrieve ancestor data for id ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getChildren
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets children entities for a given item
         *        
         * @param {Int} parentid id of content item to return children of
         * @param {string} type Object type name        
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getChildren: function (id, type) {

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetChildren",
                       [{ id: id }, { type: type }])),
               'Failed to retrieve child data for id ' + id);
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.entityResource#getPagedChildren
          * @methodOf umbraco.resources.entityResource
          *
          * @description
          * Gets paged children of a content item with a given id
          *
          * ##usage
          * <pre>
          * entityResource.getPagedChildren(1234, "Content", {pageSize: 10, pageNumber: 2})
          *    .then(function(contentArray) {
          *        var children = contentArray; 
          *        alert('they are here!');
          *    });
          * </pre> 
          * 
          * @param {Int} parentid id of content item to return children of
          * @param {string} type Object type name
          * @param {Object} options optional options object
          * @param {Int} options.pageSize if paging data, number of nodes per page, default = 1
          * @param {Int} options.pageNumber if paging data, current page index, default = 100
          * @param {String} options.filter if provided, query will only return those with names matching the filter
          * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
          * @param {String} options.orderBy property to order items by, default: `SortOrder`
          * @returns {Promise} resourcePromise object containing an array of content items.
          *
          */
        getPagedChildren: function (parentId, type, options) {

            var defaults = {
                pageSize: 1,
                pageNumber: 100,
                filter: '',
                orderDirection: "Ascending",
                orderBy: "SortOrder"
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

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "entityApiBaseUrl",
                        "GetPagedChildren",
                        {
                            id: parentId,
                            type: type,
                            pageNumber: options.pageNumber,
                            pageSize: options.pageSize,
                            orderBy: options.orderBy,
                            orderDirection: options.orderDirection,
                            filter: encodeURIComponent(options.filter)
                        }
                    )),
                'Failed to retrieve child data for id ' + parentId);
        },

        /**
          * @ngdoc method
          * @name umbraco.resources.entityResource#getPagedDescendants
          * @methodOf umbraco.resources.entityResource
          *
          * @description
          * Gets paged descendants of a content item with a given id
          *
          * ##usage
          * <pre>
          * entityResource.getPagedDescendants(1234, "Content", {pageSize: 10, pageNumber: 2})
          *    .then(function(contentArray) {
          *        var children = contentArray; 
          *        alert('they are here!');
          *    });
          * </pre> 
          * 
          * @param {Int} parentid id of content item to return descendants of
          * @param {string} type Object type name
          * @param {Object} options optional options object
          * @param {Int} options.pageSize if paging data, number of nodes per page, default = 1
          * @param {Int} options.pageNumber if paging data, current page index, default = 100
          * @param {String} options.filter if provided, query will only return those with names matching the filter
          * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
          * @param {String} options.orderBy property to order items by, default: `SortOrder`
          * @returns {Promise} resourcePromise object containing an array of content items.
          *
          */
        getPagedDescendants: function (parentId, type, options) {

            var defaults = {
                pageSize: 1,
                pageNumber: 100,
                filter: '',
                orderDirection: "Ascending",
                orderBy: "SortOrder"
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

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "entityApiBaseUrl",
                        "GetPagedDescendants",
                        {
                            id: parentId,
                            type: type,
                            pageNumber: options.pageNumber,
                            pageSize: options.pageSize,
                            orderBy: options.orderBy,
                            orderDirection: options.orderDirection,
                            filter: encodeURIComponent(options.filter)
                        }
                    )),
                'Failed to retrieve child data for id ' + parentId);
        },
     
        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#search
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of entities, given a lucene query and a type
         *
         * ##usage
         * <pre>
         * entityResource.search("news", "Media")
         *    .then(function(mediaArray) {
         *        var myDoc = mediaArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {String} Query search query 
         * @param {String} Type type of conten to search        
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        search: function (query, type, searchFrom, canceler) {

            var args = [{ query: query }, { type: type }];
            if (searchFrom) {
                args.push({ searchFrom: searchFrom });
            }

            var httpConfig = {};
            if (canceler) {
                httpConfig["timeout"] = canceler;
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "entityApiBaseUrl",
                        "Search",
                        args),
                    httpConfig),
                'Failed to retrieve entity data for query ' + query);
        },
        

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#searchAll
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of entities from all available search indexes, given a lucene query
         *
         * ##usage
         * <pre>
         * entityResource.searchAll("bob")
         *    .then(function(array) {
         *        var myDoc = array; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {String} Query search query 
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        searchAll: function (query, canceler) {

            var httpConfig = {};
            if (canceler) {
                httpConfig["timeout"] = canceler;
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "entityApiBaseUrl",
                        "SearchAll",
                        [{ query: query }]),
                    httpConfig),
                'Failed to retrieve entity data for query ' + query);
        }
            
    };
}

angular.module('umbraco.resources').factory('entityResource', entityResource);
