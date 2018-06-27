/**
    * @ngdoc service
    * @name umbraco.resources.dictionaryResource
    * @description Loads in data for dictionary items
**/
function dictionaryResource($q, $http, $location, umbRequestHelper, umbDataFormatter) {

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
            $http.get(
                umbRequestHelper.getApiUrl(
                    "dictionaryApiBaseUrl",
                    "GetById",
                    [{ id: id }])),
            "Failed to get item " + id);
    }

    /**
        * @ngdoc method
        * @name umbraco.resources.dictionaryResource#save
        * @methodOf umbraco.resources.dictionaryResource
        *
        * @description
        * Updates a dictionary
        *
        * @param {Object} dictionary  dictionary object to update     
        * @param {Bool} nameIsDirty set to true if the name has been changed
        * @returns {Promise} resourcePromise object.
        *
        */
    function save(dictionary, nameIsDirty) {

        var saveModel = umbDataFormatter.formatDictionaryPostData(dictionary, nameIsDirty);

       return umbRequestHelper.resourcePromise(
            $http.post(umbRequestHelper.getApiUrl("dictionaryApiBaseUrl", "PostSave"), saveModel),
            "Failed to save data for dictionary id " + dictionary.id);
    }

    /**
         * @ngdoc method
         * @name umbraco.resources.dictionaryResource#getList
         * @methodOf umbraco.resources.dictionaryResource
         *
         * @description
         * Gets a list of all dictionary items
         *
         * ##usage
         * <pre>
         * dictionaryResource.getList()
         *    .then(function() {
         *        alert('Found it!');
         *    });
         * </pre>
         *         
         * @returns {Promise} resourcePromise object.
         *
  **/
    function getList() {
        return umbRequestHelper.resourcePromise(
            $http.get(
                umbRequestHelper.getApiUrl(
                    "dictionaryApiBaseUrl",
                    "getList")),
            "Failed to get list");
    }
    
  var resource = {
    deleteById: deleteById,
    create: create,
    getById: getById,
    save: save,
    getList : getList
  };

  return resource;

  
}

angular.module("umbraco.resources").factory("dictionaryResource", dictionaryResource);
