/**
    * @ngdoc service
    * @name umbraco.resources.memberResource
    * @description Loads in data for members
    **/
function memberResource($q, $http, umbDataFormatter, umbRequestHelper) {
    
    /** internal method process the saving of data and post processing the result */
    function saveMember(content, action, files) {
        return umbRequestHelper.postSaveContent(
            umbRequestHelper.getApiUrl(
                "memberApiBaseUrl",
                "PostSave"),
            content, action, files);
    }

    return {
        
      
        /**
         * @ngdoc method
         * @name umbraco.resources.memberResource#getByLogin
         * @methodOf umbraco.resources.memberResource
         *
         * @description
         * Gets a member item with a given id
         *
         * ##usage
         * <pre>
         * memberResource.getByLogin("tom")
         *    .then(function(member) {
         *        var mymember = member; 
         *        alert('its here!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of member item to return        
         * @returns {Promise} resourcePromise object containing the member item.
         *
         */
        getByLogin: function (loginName) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "memberApiBaseUrl",
                       "GetByLogin",
                       [{ loginName: loginName }])),
               'Failed to retreive data for member id ' + loginName);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.memberResource#deleteByLogin
         * @methodOf umbraco.resources.memberResource
         *
         * @description
         * Deletes a member item with a given id
         *
         * ##usage
         * <pre>
         * memberResource.deleteByLogin(1234)
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre> 
         * 
         * @param {Int} id id of member item to delete        
         * @returns {Promise} resourcePromise object.
         *
         */
        deleteByLogin: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.delete(
                    umbRequestHelper.getApiUrl(
                        "memberApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                'Failed to delete item ' + id);
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
