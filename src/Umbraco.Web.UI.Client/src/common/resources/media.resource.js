/**
    * @ngdoc service 
    * @name umbraco.resources.mediaResource     
    * @description Loads in data for media
    **/
function mediaResource($q, $http, umbDataFormatter, umbRequestHelper) {
    
    /** internal method process the saving of data and post processing the result */
    function saveMediaItem(content, action, files) {
        return umbRequestHelper.postSaveContent(
            umbRequestHelper.getApiUrl(
                "mediaApiBaseUrl",
                "PostSave"),
            content, action, files);
    }

    return {
        getById: function (id) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retreive data for media id ' + id);
        },

        /** returns an empty content object which can be persistent on the content service
            requires the parent id and the alias of the content type to base the scaffold on */
        getScaffold: function (parentId, alias) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaApiBaseUrl",
                       "GetEmpty",
                       [{ contentTypeAlias: contentTypeAlias }, { parentId: parentId }])),
               'Failed to retreive data for empty media item type ' + alias);

        },

        rootMedia: function () {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "mediaApiBaseUrl",
                       "GetRootMedia")),
               'Failed to retreive data for root media');

        },

        getChildren: function (parentId) {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "mediaApiBaseUrl",
                        "GetChildren",
                        [{ parentId: parentId }])),
                'Failed to retreive data for root media');
        },
        
        /** saves or updates a media object */
        saveMedia: function (media, isNew, files) {
            return saveMediaItem(media, "save" + (isNew ? "New" : ""), files);
        }
    };
}

angular.module('umbraco.resources').factory('mediaResource', mediaResource);
