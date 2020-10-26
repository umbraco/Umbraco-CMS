/**
 * @ngdoc service
 * @name umbraco.resources.trackedReferencesResource
 * @description Loads in data for tracked references
 **/
function trackedReferencesResource($q, $http, umbRequestHelper) {


    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.trackedReferencesResource#getPagedReferences
        * @methodOf umbraco.resources.trackedReferencesResource
         *
         * @description
         * Get's a page list of tracked references for the current item, so you can see where a item is being used
         *
         * ##usage
         * <pre>
         * var options = {
         *      pageSize : 25,
         *      pageNumber : 1,
         *      entityType : 'DOCUMENT'
         *  };
         * trackedReferencesResource.getPagedReferences(1,options)
         *    .then(function(data) {
         *        console.log(data);
         *    });
         * </pre>
         *
         * @param {int} id Id of the  item to query for tracked references
         * @param {Object} args optional args object
         * @param {Int} args.pageSize the pagesize of the returned list (default 25)
         * @param {Int} args.pageNumber the current page index (default 1)
         * @param {Int} args.entityType the type of tracked entity (default : DOCUMENT). Possible values DOCUMENT, MEDIA      
         * @returns {Promise} resourcePromise object.
         *
         */
        getPagedReferences: function (id, args) {

            var defaults = {
                pageSize: 25,
                pageNumber: 1,
                entityType: "DOCUMENT"
            };
            if (args === undefined) {
                args = {};
            }

            //overwrite the defaults if there are any specified
            var options = Utilities.extend(defaults, args);
          
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "trackedReferencesApiBaseUrl",
                        "GetPagedReferences",
                        {
                            id: id,
                            entityType: options.entityType,
                            pageNumber: options.pageNumber,
                            pageSize: options.pageSize
                        }
                    )),
                "Failed to retrieve usages for entity of id " + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.trackedReferencesResource#hasReferencesInChildNodes
        * @methodOf umbraco.resources.trackedReferencesResource
         *
         * @description
         * Checks 
         *
         * ##usage
         * <pre>         
         * trackedReferencesResource.hasReferencesInDescendants(1,'MEDIA')
         *    .then(function(data) {
         *        console.log(data);
         *    });
         * </pre>
         *
         * @param {int} id Id of the  item to query for tracked references
         * @param {string} entityType  the type of tracked entity (default : DOCUMENT). Possible values DOCUMENT, MEDIA
         * @returns {Promise} resourcePromise object.
         *
         */
        hasReferencesInDescendants: function (id, entityType) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "trackedReferencesApiBaseUrl",
                        "HasReferencesInDescendants",
                        {
                            id: id,
                            entityType: entityType
                        }
                    )),
                "Failed to check for references in child nodes");

        }

    }
}

angular.module('umbraco.resources').factory('trackedReferencesResource', trackedReferencesResource);
