
//This is the file manager class, for the normal angular service 'fileManager' an instance of this class is returned,
// however in some cases we also want to create sub-instances (non-singleton) of this class. An example for this is the mini
// content editor that is launched from inside of a content editor. If the global singleton instance is used for both editors, then
// they will be sharing the same file collection state which will cause unwanted side affects. So in the mini editor, we create
// a standalone instance of the file manager and dipose of it when the scope disposes.
function FileManager() {

    var fileCollection = [];

    return {

        /**
         * @ngdoc function
         * @name umbraco.services.fileManager#createStandaloneInstance
         * @methodOf umbraco.services.fileManager
         * @function
         *
         * @description
         *  Attaches files to the current manager for the current editor for a particular property, if an empty array is set
         *   for the files collection that effectively clears the files for the specified editor.
         */
        createStandaloneInstance: function() {
            return new FileManager();
        },

        /**
         * @ngdoc function
         * @name umbraco.services.fileManager#addFiles
         * @methodOf umbraco.services.fileManager
         * @function
         *
         * @description
         *  in some cases we want to create stand alone instances (non-singleton) of the fileManager. An example for this is the mini
         * content editor that is launched from inside of a content editor. If the default global singleton instance is used for both editors, then
         * they will be sharing the same file collection state which will cause unwanted side affects. So in the mini editor, we create
         * a standalone instance of the file manager and dipose of it when the scope disposes.
         */
        setFiles: function (propertyAlias, files) {
            //this will clear the files for the current property and then add the new ones for the current property
            fileCollection = _.reject(fileCollection, function (item) {
                return item.alias === propertyAlias;
            });
            for (var i = 0; i < files.length; i++) {
                //save the file object to the files collection
                fileCollection.push({ alias: propertyAlias, file: files[i] });
            }
        },

        /**
         * @ngdoc function
         * @name umbraco.services.fileManager#getFiles
         * @methodOf umbraco.services.fileManager
         * @function
         *
         * @description
         *  Returns all of the files attached to the file manager
         */
        getFiles: function () {
            return fileCollection;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.fileManager#clearFiles
         * @methodOf umbraco.services.fileManager
         * @function
         *
         * @description
         *  Removes all files from the manager
         */
        clearFiles: function () {
            fileCollection = [];
        }
    };
}


angular.module('umbraco.services').factory('fileManager',

    /**
     * @ngdoc service
     * @name umbraco.services.fileManager
     * @function
     *
     * @description
     * Used by editors to manage any files that require uploading with the posted data, normally called by property editors
     * that need to attach files.
     * When a route changes successfully, we ensure that the collection is cleared.
     */
    function () {
        //return a new instance of a file Manager - this will be a singleton per app
        return new FileManager();
    });