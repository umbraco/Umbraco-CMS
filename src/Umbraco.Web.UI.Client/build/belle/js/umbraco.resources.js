/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-10
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app', 'angular'], function (app, angular) {
angular.module("umbraco.resources", []);
/**
    * @ngdoc factory 
    * @name umbraco.resources.contentResource
    * @description Loads/saves in data for content
    **/
function contentResource($q, $http, umbDataFormatter, umbRequestHelper) {

    /** internal method to get the api url */
    function getContentUrl(contentId) {
        return Umbraco.Sys.ServerVariables.contentApiBaseUrl + "GetContent?id=" + contentId;
    }
    /** internal method to get the api url */
    function getEmptyContentUrl(contentTypeAlias, parentId) {
        return Umbraco.Sys.ServerVariables.contentApiBaseUrl + "GetEmptyContent?contentTypeAlias=" + contentTypeAlias + "&parentId=" + parentId;
    }
    /** internal method to get the api url for publishing */
    function getSaveUrl() {
        return Umbraco.Sys.ServerVariables.contentApiBaseUrl + "PostSave";
    }
    /** internal method process the saving of data and post processing the result */
    function saveContentItem(content, action) {
        var deferred = $q.defer();

        //save the active tab id so we can set it when the data is returned.
        var activeTab = _.find(content.tabs, function(item) {
            return item.active;
        });
        var activeTabIndex = (activeTab === undefined ? 0 : _.indexOf(content.tabs, activeTab));

        //save the data
        umbRequestHelper.postMultiPartRequest(
            getSaveUrl(content.id),
            { key: "contentItem", value: umbDataFormatter.formatContentPostData(content, action) },
            function (data) {
                //TODO: transform the request callback and add the files associated with the request
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

                deferred.reject('Failed to publish data for content id ' + content.id);
            });

        return deferred.promise;
    }

    return {
        getContent: function (id) {

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

        /** returns an empty content object which can be persistent on the content service
            requires the parent id and the alias of the content type to base the scaffold on */
        getContentScaffold: function (parentId, alias) {

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
        saveContent: function (content, isNew) {
            return saveContentItem(content, "save" + (isNew ? "New" : ""));
        },

        /** saves and publishes a content object */
        publishContent: function (content, isNew) {
            return saveContentItem(content, "publish" + (isNew ? "New" : ""));
        }

    };
}

angular.module('umbraco.resources').factory('contentResource', contentResource);

/**
    * @ngdoc factory 
    * @name umbraco.resources.contentTypeResource
    * @description Loads in data for content types
    **/
function contentTypeResource($q, $http) {

    /** internal method to get the api url */
    function getChildContentTypesUrl(contentId) {
        return Umbraco.Sys.ServerVariables.contentTypeApiBaseUrl + "GetAllowedChildrenForContent?contentId=" + contentId;
    }

    return {

        //return a content type with a given ID
        getContentType: function (id) {

            return {
                name: "News Article",
                alias: "newsArticle",
                id: id,
                tabs: []
            };

        },
        //return all available types
        all: function () {
            return [];
        },

        //return children inheriting a given type
        children: function (id) {
            return [];
        },

        //return all content types a type inherits from
        parents: function (id) {
            return [];
        },

        //return all types allowed under given document
        getAllowedTypes: function (contentId) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getChildContentTypesUrl(contentId)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for content id ' + contentId);
                });

            return deferred.promise;
        }

    };
}
angular.module('umbraco.resources').factory('contentTypeResource', contentTypeResource);
/**
    * @ngdoc factory 
    * @name umbraco.resources.treeResource     
    * @description Loads in data for trees
    **/
function mediaResource($q, $http) {

    /** internal method to get the api url */
    function getRootMediaUrl() {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetRootMedia";
    }

    /** internal method to get the api url */
    function getChildrenMediaUrl(parentId) {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetChildren?parentId=" + parentId;
    }

    return {
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
        }
    };
}

angular.module('umbraco.resources').factory('mediaResource', mediaResource);

/**
    * @ngdoc factory 
    * @name umbraco.resources.treeResource     
    * @description Loads in data for trees
    **/
function treeResource($q, $http) {

    /** internal method to get the tree app url */
    function getTreeAppUrl(section) {
        return Umbraco.Sys.ServerVariables.treeApplicationApiBaseUrl + "GetApplicationTrees?application=" + section;
    }
    /** internal method to get the tree node's children url */
    function getTreeNodesUrl(node) {
        if (!node.childNodesUrl){
            throw "No childNodesUrl property found on the tree node, cannot load child nodes";
        }
        return node.childNodesUrl;
    }

    //the factory object returned
    return {
        /** Loads in the data to display the nodes for an application */
        loadApplication: function (section) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getTreeAppUrl(section)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for application tree ' + section);
                });

            return deferred.promise;
        },
        /** Loads in the data to display the child nodes for a given node */
        loadNodes: function (section, node) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getTreeNodesUrl(node)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for child nodes ' + node.nodeId);
                });

            return deferred.promise;

        }
    };
}

angular.module('umbraco.resources').factory('treeResource', treeResource);

return angular;
});