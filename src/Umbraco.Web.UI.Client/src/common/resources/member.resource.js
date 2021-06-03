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
            dataFormatter: function (c, a) {
                return umbDataFormatter.formatMemberPostData(c, a);
            }
        });
    }

    return {
        getPagedResults: function(memberTypeAlias, options) {

            if (memberTypeAlias === 'all-members') {
                memberTypeAlias = null;
            }

            var defaults = {
                pageSize: 25,
                pageNumber: 1,
                filter: '',
                orderDirection: "Ascending",
                orderBy: "LoginName",
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

            var params = [
                { pageNumber: options.pageNumber },
                { pageSize: options.pageSize },
                { orderBy: options.orderBy },
                { orderDirection: options.orderDirection },
                { orderBySystemField: toBool(options.orderBySystemField) },
                { filter: options.filter }
            ];
            if (memberTypeAlias != null) {
                params.push({ memberTypeAlias: memberTypeAlias });
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "memberApiBaseUrl",
                        "GetPagedResults",
                        params)),
                'Failed to retrieve member paged result');
        },

        getListNode: function(listName) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "memberApiBaseUrl",
                        "GetListNodeDisplay",
                        [{ listName: listName }])),
                'Failed to retrieve data for member list ' + listName);
        },

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
        getByKey: function(key) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "memberApiBaseUrl",
                        "GetByKey",
                        [{ key: key }])),
                'Failed to retrieve data for member id ' + key);
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
        deleteByKey: function(key) {
            return umbRequestHelper.resourcePromise(
                $http.post(
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
        getScaffold: function(alias) {

            if (alias) {
                return umbRequestHelper.resourcePromise(
                    $http.get(
                        umbRequestHelper.getApiUrl(
                            "memberApiBaseUrl",
                            "GetEmpty",
                            [{ contentTypeAlias: alias }])),
                    'Failed to retrieve data for empty member item type ' + alias);
            }
            else {
                return umbRequestHelper.resourcePromise(
                    $http.get(
                        umbRequestHelper.getApiUrl(
                            "memberApiBaseUrl",
                            "GetEmpty")),
                    'Failed to retrieve data for empty member item type ' + alias);
            }

        },

        /**
          * @ngdoc method
          * @name umbraco.resources.memberResource#save
          * @methodOf umbraco.resources.memberResource
          *
          * @description
          * Saves changes made to a member, if the member is new, the isNew paramater must be passed to force creation
          * if the member needs to have files attached, they must be provided as the files param and passed separately 
          * 
          * 
          * ##usage
          * <pre>
          * memberResource.getBykey("23234-sd8djsd-3h8d3j-sdh8d")
          *    .then(function(member) {
          *          member.name = "Bob";
          *          memberResource.save(member, false)
          *            .then(function(member){
          *                alert("Retrieved, updated and saved again");
          *            });
          *    });
          * </pre> 
          * 
          * @param {Object} media The member item object with changes applied
          * @param {Bool} isNew set to true to create a new item or to update an existing 
          * @param {Array} files collection of files for the media item      
          * @returns {Promise} resourcePromise object containing the saved media item.
          *
          */
        save: function(member, isNew, files) {
            return saveMember(member, "save" + (isNew ? "New" : ""), files);
        }
    };
}

angular.module('umbraco.resources').factory('memberResource', memberResource);
