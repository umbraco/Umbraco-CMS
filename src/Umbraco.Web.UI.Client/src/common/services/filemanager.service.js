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
function fileManager($rootScope) {

    var fileCollection = [];

    //Whenever a route changes - clear the curent file collection, the file collection is only relavent
    // when working in an editor and submitting data to the server.
    //This ensures that memory remains clear of any files and that the editors don't have to manually clear the files.
    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
        fileCollection = [];
    });

    return {
        /**
         * @ngdoc function
         * @name umbraco.services.fileManager#addFiles
         * @methodOf umbraco.services.fileManager
         * @function
         *
         * @description
         *  Attaches files to the current manager for the current editor for a particular property, if an empty array is set
         *   for the files collection that effectively clears the files for the specified editor.
         */
        setFiles: function(propertyId, files) {
            //this will clear the files for the current property and then add the new ones for the current property
            fileCollection = _.reject(fileCollection, function (item) {
                return item.id === propertyId;
            });
            for (var i = 0; i < files.length; i++) {
                //save the file object to the files collection
                fileCollection.push({ id: propertyId, file: files[i] });
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
        getFiles: function() {
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

angular.module('umbraco.services').factory('fileManager', fileManager);