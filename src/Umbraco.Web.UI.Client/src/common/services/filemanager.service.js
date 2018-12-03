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
function fileManager() {

    var fileCollection = [];

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
        setFiles: function (args) {
            
            //propertyAlias, files
            if (!angular.isString(args.propertyAlias)) {
                throw "args.propertyAlias must be a non empty string";
            }
            if (!angular.isObject(args.files)) {
                throw "args.files must be an object";
            }

            //normalize to null
            if (!args.culture) {
                args.culture = null;
            }

            var metaData = [];
            if (angular.isArray(args.metaData)) {
                metaData = args.metaData;
            }

            //this will clear the files for the current property/culture and then add the new ones for the current property
            fileCollection = _.reject(fileCollection, function (item) {
                return item.alias === args.propertyAlias && (!args.culture || args.culture === item.culture);
            });
            for (var i = 0; i < args.files.length; i++) {
                //save the file object to the files collection
                fileCollection.push({ alias: args.propertyAlias, file: args.files[i], culture: args.culture, metaData: metaData });
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
