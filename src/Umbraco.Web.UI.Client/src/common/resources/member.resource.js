/**
    * @ngdoc service
    * @name umbraco.resources.memberResource
    * @description Loads in data for members
    **/
function memberResource($q, $http, umbDataFormatter, umbRequestHelper) {
    
    /** internal method process the saving of data and post processing the result */
    function saveMember(content, action, files) {
        
        return umbRequestHelper.postSaveContent({
            restApiUrl: umbRequestHelper.getApiUrl(
                "memberApiBaseUrl",
                "PostSave"),
            content: content,
            action: action,
            files: files,            
            dataFormatter: function(c, a) {
                return umbDataFormatter.formatMemberPostData(c, a);
            }
        });
    }

    return {
        
      
        /**
         * @ngdoc method
         * @name umbraco.resources.memberResource#getByKey
         * @methodOf umbraco.resources.memberResource
         *
         * @description
         * Gets a member item with a given key
         *
         * ##usage
         * <pre>
         * memberResource.getByKey("0000-0000-000-00000-000")
         *    .then(function(member) {
         *        var mymember = member; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Guid} key key of member item to return        
         * @returns {Promise} resourcePromise object containing the member item.
         *
         */
        getByKey: function (key) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberApiBaseUrl",
                       "GetByKey",
                       [{ key: key }])),
               'Failed to retreive data for member id ' + key);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.memberResource#deleteByKey
         * @methodOf umbraco.resources.memberResource
         *
         * @description
         * Deletes a member item with a given key
         *
         * ##usage
         * <pre>
         * memberResource.deleteByKey("0000-0000-000-00000-000")
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre> 
         * 
         * @param {Guid} key id of member item to delete        
         * @returns {Promise} resourcePromise object.
         *
         */
        deleteByKey: function (key) {
            return umbRequestHelper.resourcePromise(
                $http.delete(
                    umbRequestHelper.getApiUrl(
                        "memberApiBaseUrl",
                        "DeleteByKey",
                        [{ key: key }])),
                'Failed to delete item ' + key);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.memberResource#getScaffold
         * @methodOf umbraco.resources.memberResource
         *
         * @description
         * Returns a scaffold of an empty member item, given the id of the member item to place it underneath and the member type alias.
         *         
         * - Member Type alias must be provided so umbraco knows which properties to put on the member scaffold 
         * 
         * The scaffold is used to build editors for member that has not yet been populated with data.
         * 
         * ##usage
         * <pre>
         * memberResource.getScaffold('client')
         *    .then(function(scaffold) {
         *        var myDoc = scaffold;
         *        myDoc.name = "My new member item"; 
         *
         *        memberResource.save(myDoc, true)
         *            .then(function(member){
         *                alert("Retrieved, updated and saved again");
         *            });
         *    });
         * </pre> 
         * 
         * @param {String} alias membertype alias to base the scaffold on        
         * @returns {Promise} resourcePromise object containing the member scaffold.
         *
         */
        getScaffold: function (alias) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberApiBaseUrl",
                       "GetEmpty",
                       [{ contentTypeAlias: alias }])),
               'Failed to retreive data for empty member item type ' + alias);

        },
        
        /** saves or updates a member object */
        save: function (member, isNew, files) {
            return saveMember(member, "save" + (isNew ? "New" : ""), files);
        }
    };
}

angular.module('umbraco.resources').factory('memberResource', memberResource);
