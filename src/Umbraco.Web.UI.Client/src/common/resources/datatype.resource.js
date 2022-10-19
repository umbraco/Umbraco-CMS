/**
    * @ngdoc service
    * @name umbraco.resources.dataTypeResource
    * @description Loads in data for data types
    **/
function dataTypeResource($q, $http, umbDataFormatter, umbRequestHelper) {

  return {

        /**
         * @ngdoc method
         * @name umbraco.resources.dataTypeResource#getPreValues
         * @methodOf umbraco.resources.dataTypeResource
         *
         * @description
         * Retrieves available prevalues for a given data type + editor
         *
         * ##usage
         * <pre>
         * dataTypeResource.getPreValues("Umbraco.MediaPicker", 1234)
         *    .then(function(prevalues) {
         *        alert('its gone!');
         *    });
         * </pre>
         *
         * @param {String} editorAlias string alias of editor type to retrieve prevalues configuration for
         * @param {Int} id id of datatype to retrieve prevalues for
         * @returns {Promise} resourcePromise object.
         *
         */
        getPreValues: function (editorAlias, dataTypeId) {

      if (!dataTypeId) {
        dataTypeId = -1;
      }

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetPreValues",
            [{ editorAlias: editorAlias }, { dataTypeId: dataTypeId }])),
        "Failed to retrieve pre values for editor alias " + editorAlias);
    },

    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getReferences
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Retrieves references of a given data type.
     *
     * @param {Int} id id of datatype to retrieve references for
     * @returns {Promise} resourcePromise object.
     *
     */
    getReferences: function (id) {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetReferences",
            { id: id })),
        "Failed to retrieve usages for data type of id " + id);

    },

    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getById
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Gets a data type item with a given id
     *
     * ##usage
     * <pre>
     * dataTypeResource.getById(1234)
     *    .then(function(datatype) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {Int} id id of data type to retrieve
     * @returns {Promise} resourcePromise object.
     *
     */
    getById: function (id) {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetById",
            [{ id: id }])),
        "Failed to retrieve data for data type id " + id);
    },

    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getByName
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Gets a data type item with a given name
     *
     * ##usage
     * <pre>
     * dataTypeResource.getByName("upload")
     *    .then(function(datatype) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {String} name Name of data type to retrieve
     * @returns {Promise} resourcePromise object.
     *
     */
    getByName: function (name) {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetByName",
            [{ name: name }])),
        "Failed to retrieve data for data type with name: " + name);
    },

    getAll: function () {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetAll")),
        "Failed to retrieve data");
    },

    getGroupedDataTypes: function () {
      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetGroupedDataTypes")),
        "Failed to retrieve data");
    },

    getGroupedPropertyEditors: function () {
      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetGroupedPropertyEditors")),
        "Failed to retrieve data");
    },

    getAllPropertyEditors: function () {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetAllPropertyEditors")),
        "Failed to retrieve data");
    },

    /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getScaffold
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Returns a scaffold of an empty data type item
     *
     * The scaffold is used to build editors for data types that has not yet been populated with data.
     *
     * ##usage
     * <pre>
     * dataTypeResource.getScaffold()
     *    .then(function(scaffold) {
     *        var myType = scaffold;
     *        myType.name = "My new data type";
     *
     *        dataTypeResource.save(myType, myType.preValues, true)
     *            .then(function(type){
     *                alert("Retrieved, updated and saved again");
     *            });
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object containing the data type scaffold.
     *
     */
    getScaffold: function (parentId) {

      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetEmpty", { parentId: parentId })),
        "Failed to retrieve data for empty datatype");
    },
    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#deleteById
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Deletes a data type with a given id
     *
     * ##usage
     * <pre>
     * dataTypeResource.deleteById(1234)
     *    .then(function() {
     *        alert('its gone!');
     *    });
     * </pre>
     *
     * @param {Int} id id of content item to delete
     * @returns {Promise} resourcePromise object.
     *
     */
    deleteById: function (id) {
      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "DeleteById",
            [{ id: id }])),
        "Failed to delete item " + id);
    },

    deleteContainerById: function (id) {

      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "DeleteContainer",
            [{ id: id }])),
        'Failed to delete content type contaier');
    },



    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getCustomListView
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Returns a custom listview, given a content types alias
     *
     *
     * ##usage
     * <pre>
     * dataTypeResource.getCustomListView("home")
     *    .then(function(listview) {
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object containing the listview datatype.
     *
     */

    getCustomListView: function (contentTypeAlias) {
      return umbRequestHelper.resourcePromise(
        $http.get(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "GetCustomListView",
            { contentTypeAlias: contentTypeAlias }
          )),
        "Failed to retrieve data for custom listview datatype");
    },

    /**
    * @ngdoc method
    * @name umbraco.resources.dataTypeResource#createCustomListView
    * @methodOf umbraco.resources.dataTypeResource
    *
    * @description
    * Creates and returns a custom listview, given a content types alias
    *
    * ##usage
    * <pre>
    * dataTypeResource.createCustomListView("home")
    *    .then(function(listview) {
    *    });
    * </pre>
    *
    * @returns {Promise} resourcePromise object containing the listview datatype.
    *
    */
    createCustomListView: function (contentTypeAlias) {
      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "PostCreateCustomListView",
            { contentTypeAlias: contentTypeAlias }
          )),
        "Failed to create a custom listview datatype");
    },

    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#save
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Saves or update a data type
     *
     * @param {Object} dataType data type object to create/update
     * @param {Array} preValues collection of prevalues on the datatype
     * @param {Bool} isNew set to true if type should be create instead of updated
     * @returns {Promise} resourcePromise object.
     *
     */
    save: function (dataType, preValues, isNew) {

      var saveModel = umbDataFormatter.formatDataTypePostData(dataType, preValues, "save" + (isNew ? "New" : ""));

      return umbRequestHelper.resourcePromise(
        $http.post(umbRequestHelper.getApiUrl("dataTypeApiBaseUrl", "PostSave"), saveModel),
        "Failed to save data for data type id " + dataType.id);
    },

    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#move
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Moves a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * dataTypeResource.move({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("node was moved");
     *    }, function(err){
     *      alert("node didnt move:" + err.data.Message); 
     *    });
     * </pre> 
     * @param {Object} args arguments object
     * @param {Int} args.id the ID of the node to move
     * @param {Int} args.parentId the ID of the parent node to move to
     * @returns {Promise} resourcePromise object.
     *
     */
    move: function (args) {
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
        $http.post(umbRequestHelper.getApiUrl("dataTypeApiBaseUrl", "PostMove"),
          {
            parentId: args.parentId,
            id: args.id
          }, { responseType: 'text' }),
        'Failed to move content');
    },

    /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#copy
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Copies a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * dataTypeResource.copy({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("node has been copied");
     *    }, function(err){
     *      alert("node didnt copy:" + err.data.Message);
     *    });
     * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.idd the ID of the node to copy
     * @param {Int} args.parentId the ID of the parent node to copy to
     * @returns {Promise} resourcePromise object.
     *
     */
    copy: function (args) {
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
        $http.post(umbRequestHelper.getApiUrl("dataTypeApiBaseUrl", "PostCopy"),
          {
            parentId: args.parentId,
            id: args.id
          }, { responseType: 'text' }),
        'Failed to copy content');
    },

    createContainer: function (parentId, name) {

      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "dataTypeApiBaseUrl",
            "PostCreateContainer",
            { parentId: parentId, name: encodeURIComponent(name) })),
        'Failed to create a folder under parent id ' + parentId);
    },

        renameContainer: function (id, name) {
            return umbRequestHelper.resourcePromise(
                $http.post
                    (umbRequestHelper.getApiUrl(
                        "dataTypeApiBaseUrl",
                        "PostRenameContainer",
                        { id: id, name: encodeURIComponent(name) })),
                "Failed to rename the folder with id " + id);
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.dataTypeResource#hasValues
         * @methodOf umbraco.resources.dataTypeResource
         *
         * @description
         * Checks for values stored for the data type
         *
         * ##usage
         * <pre>
         * dataTypeResource.hasValues(id)
         *    .then(function(data) {
         *        console.log(data.hasValues);
         *    }, function(err) {
         *      console.log("failed to check if data type has values", err);
         *    });
         * </pre> 
         * 
         * @param {Int} id id of data type
         * @returns {Promise} resourcePromise object.
         */
        hasValues: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "dataTypeApiBaseUrl",
                        "hasvalues",
                        { id }
                    )),
                "Failed to check if the data type with " + id + " has values");
        }
    };
}

angular.module("umbraco.resources").factory("dataTypeResource", dataTypeResource);
