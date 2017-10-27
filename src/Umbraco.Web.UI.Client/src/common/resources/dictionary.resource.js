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

  /**
         * @ngdoc method
         * @name umbraco.resources.dictionaryResource#create
         * @methodOf umbraco.resources.dictionaryResource
         *
         * @description
         * Creates a dictionary item with the gieven key and parent id
         *
         * ##usage
         * <pre>
         * dictionaryResource.create(1234,"Item key")
         *    .then(function() {
         *        alert('its created!');
         *    });
         * </pre>
         *
         * @param {Int} parentid the parentid of the new dictionary item
         * @param {String} key the key of the new dictionary item
         * @returns {Promise} resourcePromise object.
         *
  **/
  function create(parentid, key) {
    return umbRequestHelper.resourcePromise(
      $http.post(
        umbRequestHelper.getApiUrl(
          "dictionaryApiBaseUrl",
          "Create",
          { parentId: parentid, key : key })),
      "Failed to create item ");
  }

    /**
         * @ngdoc method
         * @name umbraco.resources.dictionaryResource#deleteById
         * @methodOf umbraco.resources.dictionaryResource
         *
         * @description
         * Gets a dictionary item with a given id
         *
         * ##usage
         * <pre>
         * dictionaryResource.getById(1234)
         *    .then(function() {
         *        alert('Found it!');
         *    });
         * </pre>
         *
         * @param {Int} id id of dictionary item to get
         * @returns {Promise} resourcePromise object.
         *
  **/
    function getById(id) {
        return umbRequestHelper.resourcePromise(
            $http.post(
                umbRequestHelper.getApiUrl(
                    "dictionaryApiBaseUrl",
                    "GetById",
                    [{ id: id }])),
            "Failed to get item " + id);
    }

  var resource = {
    deleteById: deleteById,
    create: create,
    getById: getById
  };

  return resource;

  
}

angular.module("umbraco.resources").factory("dictionaryResource", dictionaryResource);