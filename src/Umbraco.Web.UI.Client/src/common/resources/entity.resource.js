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
         * entityResource.getPath(id)
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
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetById",
                       [{ id: id}, {type: type }])),
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
            
            var query = "";
            _.each(ids, function(item) {
                query += "ids=" + item + "&";
            });

            // if ids array is empty we need a empty variable in the querystring otherwise the service returns a error
            if (ids.length === 0) {
                query += "ids=&";
            }

            query += "type=" + type;

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetByIds",
                       query)),
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
         * @name umbraco.resources.entityResource#getAncestors
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets children entities for a given item
         *        
         * 
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
