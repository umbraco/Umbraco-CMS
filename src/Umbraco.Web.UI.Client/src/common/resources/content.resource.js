/**
    * @ngdoc factory 
    * @name umbraco.resources.contentResource
    * @description Loads/saves in data for content
    **/
function contentResource($q, $http, umbDataFormatter, umbRequestHelper) {

    /** internal method to get the api url */
    function getContentUrl(contentId) {
        return Umbraco.Sys.ServerVariables.contentApiBaseUrl + "GetById?id=" + contentId;
    }
    /** internal method to get the api url */
    function getByIdsUrl(ids) {
        var idQuery = "";
        _.each(ids, function(item) {
            idQuery += "ids=" + item + "&";
        });
        return Umbraco.Sys.ServerVariables.contentApiBaseUrl + "GetById?" + idQuery;
    }
    /** internal method to get the api url */
    function getEmptyContentUrl(contentTypeAlias, parentId) {
        return Umbraco.Sys.ServerVariables.contentApiBaseUrl + "GetEmpty?contentTypeAlias=" + contentTypeAlias + "&parentId=" + parentId;
    }
    /** internal method to get the api url for publishing */
    function getSaveUrl() {
        return Umbraco.Sys.ServerVariables.contentApiBaseUrl + "PostSave";
    }
    /** internal method process the saving of data and post processing the result */
    function saveContentItem(content, action, files) {
        return umbRequestHelper.postSaveContent(getSaveUrl(content.id), content, action, files);
    }

    return {
        getById: function (id) {

            var deferred = $q.defer();

            //go and get the data
            $http.get(getContentUrl(id)).
                success(function (data, status, headers, config) {
                    //set the first tab to active
                    _.each(data.tabs, function (item) {
                        item.active = false;
                    });
                    if (data.tabs.length > 0){
                        data.tabs[0].active = true;
                    }

                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for content id ' + id);
                });

            return deferred.promise;
        },
        
        getByIds: function (ids) {
            var deferred = $q.defer();

            //go and get the data
            $http.get(getByIdsUrl(ids)).
                success(function (data, status, headers, config) {                    
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for content ids ' + ids);
                });

            return deferred.promise;
        },

        /** returns an empty content object which can be persistent on the content service
            requires the parent id and the alias of the content type to base the scaffold on */
        getScaffold: function (parentId, alias) {

            var deferred = $q.defer();

            //go and get the data
            $http.get(getEmptyContentUrl(alias, parentId)).
                success(function (data, status, headers, config) {
                    //set the first tab to active
                    _.each(data.tabs, function (item) {
                        item.active = false;
                    });
                    if (data.tabs.length > 0){
                        data.tabs[0].active = true;
                    }

                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for empty content item type ' + alias);
                });

            return deferred.promise;
        },

        getChildren: function (parentId, options) {

            if (options === undefined) {
                options = {
                    take: 10,
                    offset: 0,
                    filter: ''
                };
            }

            var collection = { take: 10, total: 68, pages: 7, currentPage: options.offset, filter: options.filter };
            collection.total = 56 - (options.filter.length);
            collection.pages = Math.round(collection.total / collection.take);
            collection.resultSet = [];

            if (collection.total < options.take) {
                collection.take = collection.total;
            } else {
                collection.take = options.take;
            }


            var _id = 0;
            for (var i = 0; i < collection.take; i++) {
                _id = (parentId + i) * options.offset;
                var cnt = this.getContent(_id);

                //here we fake filtering
                if (options.filter !== '') {
                    cnt.name = options.filter + cnt.name;
                }

                collection.resultSet.push(cnt);
            }

            return collection;
        },

        /** saves or updates a content object */
        saveContent: function (content, isNew, files) {
            return saveContentItem(content, "save" + (isNew ? "New" : ""), files);
        },

        /** saves and publishes a content object */
        publishContent: function (content, isNew, files) {
            return saveContentItem(content, "publish" + (isNew ? "New" : ""), files);
        }

    };
}

angular.module('umbraco.resources').factory('contentResource', contentResource);
