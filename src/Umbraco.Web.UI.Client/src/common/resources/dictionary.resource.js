function dictionaryResource($q, $http, umbDataFormatter, umbRequestHelper) {

    return {

        getById: function (id) {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "dictionaryApiBaseUrl",
                       "GetById",
                       [{ id: id }])),
               'Failed to retrieve data for dictionary item id ' + id);
        },

        getAll: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "dictionaryApiBaseUrl",
                       "GetAll")),
               'Failed to retrieve data');
        },

        getScaffold: function () {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "dictionaryApiBaseUrl",
                       "GetEmpty")),
               'Failed to retrieve data for empty dictionary item');
        },

        deleteById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "dictionaryApiBaseUrl",
                        "DeleteById",
                        [{ id: id }])),
                'Failed to delete dictionary item ' + id);
        },

        save: function (dicationaryItem, translations, isNew) {

            var saveModel = umbDataFormatter.formatDictionaryItemPostData(dicationaryItem, translations, "save" + (isNew ? "New" : ""));

            return umbRequestHelper.resourcePromise(
                 $http.post(
                    umbRequestHelper.getApiUrl(
                        "dictionaryApiBaseUrl",
                        "PostSave"),
                        saveModel),
                'Failed to save data for dicationary item id ' + dicationaryItem.id);
        }
    };
}

angular.module('umbraco.resources').factory('dictionaryResource', dictionaryResource);
