/**
    * @ngdoc service
    * @name umbraco.resources.dataTypeResource
    * @description Loads in data for data types
    **/
function dataTypeResource($q, $http, umbDataFormatter, umbRequestHelper) {
    
    return {
        
        getPreValues: function (editorId, dataTypeId) {

            if (!dataTypeId) {
                dataTypeId = -1;
            }

            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "dataTypeApiBaseUrl",
                       "GetPreValues",
                       [{ editorId: editorId }, { dataTypeId: dataTypeId }])),
               'Failed to retreive pre values for editor id ' + editorId);
        },

        getById: function (id) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "dataTypeApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retreive data for data type id ' + id);
        },

        /** returns an empty content object which can be persistent on the content service
            requires the parent id and the alias of the content type to base the scaffold on */
        getScaffold: function (parentId, alias) {
            
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "dataTypeApiBaseUrl",
                       "GetEmpty")),
               'Failed to retreive data for empty media item type ' + alias);

        },

        /** saves or updates a data type object */
        save: function (dataType, preValues, isNew) {
            
            var saveModel = umbDataFormatter.formatDataTypePostData(dataType, preValues, "save" + (isNew ? "New" : ""));

            return umbRequestHelper.resourcePromise(
                 $http.post(umbRequestHelper.getApiUrl("dataTypeApiBaseUrl", "PostSave"), saveModel),
                'Failed to save data for data type id ' + dataType.id);
        }
    };
}

angular.module('umbraco.resources').factory('dataTypeResource', dataTypeResource);
