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


    var mgr = {
        /**
         * @ngdoc function
         * @name umbraco.services.fileManager#setFiles
         * @methodOf umbraco.services.fileManager
         * @function
         *
         * @description
         *  Attaches files to the current manager for the current editor for a particular property, if an empty array is set
         *   for the files collection that effectively clears the files for the specified editor.
         */
        setFiles: function (args) {

            //propertyAlias, files
            if (!Utilities.isString(args.propertyAlias)) {
                throw "args.propertyAlias must be a non empty string";
            }
            if (!Utilities.isObject(args.files)) {
                throw "args.files must be an object";
            }

            //normalize to null
            if (!args.culture) {
                args.culture = null;
            }

            if (!args.segment) {
                args.segment = null;
            }

            var metaData = [];
            if (Utilities.isArray(args.metaData)) {
                metaData = args.metaData;
            }

            //this will clear the files for the current property/culture/segment and then add the new ones for the current property
            fileCollection = _.reject(fileCollection, function (item) {
                return item.alias === args.propertyAlias && (!args.culture || args.culture === item.culture) && (!args.segment || args.segment === item.segment);
            });
            for (var i = 0; i < args.files.length; i++) {
                //save the file object to the files collection
                fileCollection.push({ alias: args.propertyAlias, file: args.files[i], culture: args.culture, segment: args.segment, metaData: metaData });
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

    //execute on each successful route (this is only bound once per application since a service is a singleton)
    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
        //reset the file manager on each route change, the file collection is only relavent
        // when working in an editor and submitting data to the server.
        //This ensures that memory remains clear of any files and that the editors don't have to manually clear the files.
        mgr.clearFiles();
    });

    return mgr;
}

angular.module('umbraco.services').factory('fileManager', fileManager);
