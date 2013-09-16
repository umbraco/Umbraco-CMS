/**
    * @ngdoc service
    * @name umbraco.resources.entityResource
    * @description Loads in basic data for all entities
    * 
    * ##What is an entity?
    * An entity is a basic **read-only** representation of an Umbraco node. It contains only the most
    * basic properties used to display the item in trees, lists and navigation. 
    *
    * ##What is the difference between get entity and get content?
    * the entity only contains the basic node data, name, id and guid, whereas content
    * nodes fetched through the entity service also contains additional meta data such
    * as icon, document type, path and so on.
    *
    * ##Entity object types?
    * As an optional parameter, you can pass in the specific type name. So if you know you
    * are looking for a specific type, you should pass in the object name, to make lookup faster
    * and to return more data.
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
    **/
function entityResource($q, $http, umbRequestHelper) {

    //the factory object returned
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getEntityById
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an entity with a given id
         *
         * ##usage
         * <pre>
         * entityResource.getEntityById(1234)
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         *
         * //Only return users
         * entityResource.getEntityById(0, "User")
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of entity to return
         * @param {string} type optional Object type name        
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getById: function (id, type) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetById",
                       [{ id: id, type: type }])),
               'Failed to retreive entity data for id ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getEntitiesByIds
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of entities, given a collection of ids
         *
         * ##usage
         * <pre>
         * entityResource.getEntitiesByIds( [1234,2526,28262])
         *    .then(function(contentArray) {
         *        var myDoc = contentArray; 
         *        alert('they are here!');
         *    });
         * 
         * //Only return templates
         * entityResource.getEntitiesByIds( [1234,2526,28262], "Template")
         *    .then(function(templateArray) {
         *        var myDoc = contentArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {Array} ids ids of entities to return as an array
         * @param {string} type optional type name        
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        getByIds: function (ids) {
            
            var idQuery = "";
            _.each(ids, function(item) {
                idQuery += "ids=" + item + "&";
            });

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetByIds",
                       idQuery)),
               'Failed to retreive entity data for ids ' + idQuery);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getEntityById
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an entity with a given id
         *
         * ##usage
         * <pre>
         * //returns all entities, you should NEVER do that
         * entityResource.getAll()
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         *
         * //Only return users
         * entityResource.getAll("User")
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {string} type Object type name        
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getAll: function (type) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetAll",
                       [{type: type }])),
               'Failed to retreive entity data for type ' + type);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getEntityById
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an entity with a given id
         *
         * ##usage
         * <pre>
         * //returns all entities, you should NEVER do that
         * entityResource.getAll()
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         *
         * //Only return users
         * entityResource.getAll("User")
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {string} type Object type name        
         * @returns {Promise} resourcePromise object containing the entity.
         *
         */
        getAncestors: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetAncestors",
                       [{id: id}])),
               'Failed to retreive entity data for id ' + id);
        },


        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getDocumentById
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets a content entity with a given id
         *
         * ##usage
         * <pre>
         * entityResource.getDocumentById(1234)
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of document to return        
         * @returns {Promise} resourcePromise object containing the document.
         *
         */
        getDocumentById: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetDocumentById",
                       [{ id: id }])),
               'Failed to retreive entity data for id ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getDocumentsByIds
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of content entities, given a collection of ids
         *
         * ##usage
         * <pre>
         * entityResource.getDocumentsByIds( [1234,2526,28262])
         *    .then(function(contentArray) {
         *        var myDoc = contentArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {Array} ids ids of entities to return as an array        
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        getDocumentsByIds: function (ids) {
            
            var idQuery = "";
            _.each(ids, function(item) {
                idQuery += "ids=" + item + "&";
            });

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetDocumentsByIds",
                       idQuery)),
               'Failed to retreive document data for ids ' + idQuery);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#searchDocuments
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of content entities, given a query
         *
         * ##usage
         * <pre>
         * entityResource.searchDocuments("news")
         *    .then(function(contentArray) {
         *        var myDoc = contentArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {String} Query search query        
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        searchDocuments: function (query) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "SearchDocuments",
                       query)),
               'Failed to retreive document data for query ' + query);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getMediaById
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets a media entity with a given id
         *
         * ##usage
         * <pre>
         * entityResource.getMediaById(1234)
         *    .then(function(ent) {
         *        var myDoc = ent; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of media to return        
         * @returns {Promise} resourcePromise object containing the media.
         *
         */
        getMediaById: function (id) {            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetMediaById",
                       [{ id: id }])),
               'Failed to retreive media data for id ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#getMediaByIds
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of media entities, given a collection of ids
         *
         * ##usage
         * <pre>
         * entityResource.getMediaByIds( [1234,2526,28262])
         *    .then(function(mediaArray) {
         *        var myDoc = contentArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {Array} ids ids of entities to return as an array        
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        getMediaByIds: function (ids) {
            
            var idQuery = "";
            _.each(ids, function(item) {
                idQuery += "ids=" + item + "&";
            });

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "GetMediaByIds",
                       idQuery)),
               'Failed to retreive media data for ids ' + idQuery);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.entityResource#searchMedia
         * @methodOf umbraco.resources.entityResource
         *
         * @description
         * Gets an array of medoa entities, given a query
         *
         * ##usage
         * <pre>
         * entityResource.searchMedia("news")
         *    .then(function(mediaArray) {
         *        var myDoc = mediaArray; 
         *        alert('they are here!');
         *    });
         * </pre> 
         * 
         * @param {String} Query search query        
         * @returns {Promise} resourcePromise object containing the entity array.
         *
         */
        searchMedia: function (query) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "entityApiBaseUrl",
                       "SearchMedia",
                       query)),
               'Failed to retreive media data for query ' + query);
        }
            
    };
}

angular.module('umbraco.resources').factory('entityResource', entityResource);
