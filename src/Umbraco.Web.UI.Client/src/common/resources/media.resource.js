/**
    * @ngdoc factory 
    * @name umbraco.resources.treeResource     
    * @description Loads in data for trees
    **/
function mediaResource($q, $http) {

    /** internal method to get the api url */
    function getMediaUrl(contentId) {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetById?id=" + contentId;
    }
    /** internal method to get the api url */
    function getRootMediaUrl() {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetRootMedia";
    }

    /** internal method to get the api url */
    function getChildrenMediaUrl(parentId) {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetChildren?parentId=" + parentId;
    }

    /** internal method to get the api url for publishing */
    function getSaveUrl() {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "PostSave";
    }
    
    /** internal method process the saving of data and post processing the result */
    function saveMediaItem(content, action, files) {
        var deferred = $q.defer();

        //save the active tab id so we can set it when the data is returned.
        var activeTab = _.find(content.tabs, function (item) {
            return item.active;
        });
        var activeTabIndex = (activeTab === undefined ? 0 : _.indexOf(content.tabs, activeTab));

        //save the data
        umbRequestHelper.postMultiPartRequest(
            getSaveUrl(content.id),
            { key: "mediaItem", value: umbDataFormatter.formatContentPostData(content, action) },
            function (data, formData) {
                //now add all of the assigned files
                for (var f in files) {
                    //each item has a property id and the file object, we'll ensure that the id is suffixed to the key
                    // so we know which property it belongs to on the server side
                    formData.append("file_" + files[f].id, files[f].file);
                }

            },
            function (data, status, headers, config) {
                //success callback

                //reset the tabs and set the active one
                _.each(data.tabs, function (item) {
                    item.active = false;
                });
                data.tabs[activeTabIndex].active = true;

                //the data returned is the up-to-date data so the UI will refresh
                deferred.resolve(data);
            },
            function (data, status, headers, config) {
                //failure callback

                deferred.reject('Failed to save data for media id ' + content.id);
            });

        return deferred.promise;
    }

    return {
        getById: function (id) {

            var deferred = $q.defer();

            //go and get the data
            $http.get(getMediaUrl(id)).
                success(function (data, status, headers, config) {
                    //set the first tab to active
                    _.each(data.tabs, function (item) {
                        item.active = false;
                    });
                    if (data.tabs.length > 0) {
                        data.tabs[0].active = true;
                    }

                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for media id ' + id);
                });

            return deferred.promise;
        },

        rootMedia: function () {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getRootMediaUrl()).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for application tree ' + section);
                });

            return deferred.promise;
        },

        getChildren: function (parentId) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getChildrenMediaUrl(parentId)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for application tree ' + section);
                });

            return deferred.promise;
        },
        
        /** saves or updates a media object */
        saveMedia: function (media, isNew, files) {
            return saveMediaItem(media, "save" + (isNew ? "New" : ""), files);
        },
    };
}

angular.module('umbraco.resources').factory('mediaResource', mediaResource);
