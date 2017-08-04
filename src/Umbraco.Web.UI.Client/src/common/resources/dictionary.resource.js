/**
    * @ngdoc service
    * @name umbraco.resources.dictionaryResource
    * @description Loads in data for dictionary items
**/
function dictionaryResource($q, $http, umbRequestHelper) {

  /**
         * @ngdoc method
         * @name umbraco.resources.dictionaryResource#deleteById
         * @methodOf umbraco.resources.dictionaryResource
         *
         * @description
         * Deletes a dictionary item with a given id
         *
         * ##usage
         * <pre>
         * dictionaryResource.deleteById(1234)
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre>
         *
         * @param {Int} id id of dictionary item to delete
         * @returns {Promise} resourcePromise object.
         *
  **/
  function deleteById(id) {
    return umbRequestHelper.resourcePromise(
      $http.post(
        umbRequestHelper.getApiUrl(
          "dictionaryApiBaseUrl",
          "DeleteById",
          [{ id: id }])),
      "Failed to delete item " + id);
  }

  var resource = {
    deleteById: deleteById
  };

  return resource;

  
}

angular.module("umbraco.resources").factory("dictionaryResource", dictionaryResource);