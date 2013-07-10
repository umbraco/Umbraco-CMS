/**
    * @ngdoc service 
    * @name umbraco.resources.treeResource     
    * @description Loads in data for trees
    **/
function mediaResource($q, $http, umbDataFormatter, umbRequestHelper, angularHelper) {

    /** internal method to get the api url */
    function getMediaUrl(contentId) {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetById?id=" + contentId;
    }
    
    /** internal method to get the api url */
    function getEmptyMediaUrl(contentTypeAlias, parentId) {
        return Umbraco.Sys.ServerVariables.mediaApiBaseUrl + "GetEmpty?contentTypeAlias=" + contentTypeAlias + "&parentId=" + parentId;
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
        return umbRequestHelper.postSaveContent(getSaveUrl(content.id), content, action, files);
    }

    return {
        getById: function (id) {
            return angularHelper.resourcePromise(
                $http.get(getMediaUrl(id)),
                'Failed to retreive data for media id ' + id);
        },

        /** returns an empty content object which can be persistent on the content service
            requires the parent id and the alias of the content type to base the scaffold on */
        getScaffold: function (parentId, alias) {
            return angularHelper.resourcePromise(
                $http.get(getEmptyMediaUrl(alias, parentId)),
                'Failed to retreive data for empty content item type ' + alias);
        },

        rootMedia: function () {
            return angularHelper.resourcePromise(
                $http.get(getRootMediaUrl()),
                'Failed to retreive data for application tree ' + section);
        },

        getChildren: function (parentId) {
            return angularHelper.resourcePromise(
                $http.get(getChildrenMediaUrl(parentId)),
                'Failed to retreive data for application tree ' + section);
        },
        
        /** saves or updates a media object */
        saveMedia: function (media, isNew, files) {
            return saveMediaItem(media, "save" + (isNew ? "New" : ""), files);
        }
    };
}

angular.module('umbraco.resources').factory('mediaResource', mediaResource);
