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
      * @name umbraco.resources.dictionaryResource#move
      * @methodOf umbraco.resources.dictionaryResource
      *
      * @description
      * Moves a dictionary item underneath a new parentId
      *
      * ##usage
      * <pre>
      * dictionaryResource.move({ parentId: 1244, id: 123 })
      *    .then(function() {
      *        alert("node was moved");
      *    }, function(err){
      *      alert("node didnt move:" + err.data.Message);
      *    });
      * </pre>
      * @param {Object} args arguments object
      * @param {int} args.id the int of the dictionary item to move
      * @param {int} args.parentId the int of the parent dictionary item to move to
      * @returns {Promise} resourcePromise object.
      *
      */
    function move (args) {
        if (!args) {
          throw "args cannot be null";
        }
        if (!args.parentId) {
          throw "args.parentId cannot be null";
        }
        if (!args.id) {
          throw "args.id cannot be null";
        }

        return umbRequestHelper.resourcePromise(
          $http.post(umbRequestHelper.getApiUrl("dictionaryApiBaseUrl", "PostMove"),
            {
              parentId: args.parentId,
              id: args.id
            }, { responseType: 'text' }));
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
        * @name umbraco.resources.dictionaryResource#export
        * @methodOf umbraco.resources.dictionaryResource
        *
        * @description
        * Export dictionary items of a given id.
        *
        * ##usage
        * <pre>
        * dictionaryResource.exportItem(1234){
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        *
        * @param {Int} id the ID of the dictionary item so export
        * @param {Bool?} includeChildren if children should also be exported
        * @returns {Promise} resourcePromise object.
        *
        */
      function exportItem(id, includeChildren) {
          if (!id) {
              throw "id cannot be null";
          }

          var url = umbRequestHelper.getApiUrl("dictionaryApiBaseUrl", "ExportDictionary", { id: id, includeChildren: includeChildren });

          return umbRequestHelper.downloadFile(url).then(function () {
              localizationService.localize("speechBubbles_dictionaryItemExportedSuccess").then(function(value) {
                  notificationsService.success(value);
              });
          }, function (data) {
              localizationService.localize("speechBubbles_dictionaryItemExportedError").then(function(value) {
                  notificationsService.error(value);
              });
          });
      }

    /**
        * @ngdoc method
        * @name umbraco.resources.dictionaryResource#import
        * @methodOf umbraco.resources.dictionaryResource
        *
        * @description
        * Import a dictionary item from a file
        *
        * ##usage
        * <pre>
        * dictionaryResource.importItem("path to file"){
        *    .then(function() {
        *       Do stuff..
        *    });
        * </pre>
        *
        * @param {String} file path of the file to import
        * @param {Int?} parentId the int of the parent dictionary item to move incomming dictionary items to
        * @returns {Promise} resourcePromise object.
        *
        */
    function importItem(file, parentId) {
        if (!file) {
            throw "file cannot be null";
        }

      return umbRequestHelper.resourcePromise(
        $http.post(umbRequestHelper.getApiUrl("dictionaryApiBaseUrl", "ImportDictionary", { file: file, parentId: parentId })),
            "Failed to import dictionary item " + file
        );
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
    move: move,
    exportItem: exportItem,
    importItem: importItem,
    getList : getList
  };

  return resource;

  
}

angular.module("umbraco.resources").factory("dictionaryResource", dictionaryResource);
