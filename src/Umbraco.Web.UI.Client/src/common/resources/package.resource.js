/**
    * @ngdoc service
    * @name umbraco.resources.packageInstallResource
    * @description handles data for package installations
    **/
function packageResource($q, $http, umbDataFormatter, umbRequestHelper) {
    
    return {
        

        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#fetchPackage
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Downloads a package file from our.umbraco.org to the website server.
         * 
         * ##usage
         * <pre>
         * packageResource.download("guid-guid-guid-guid")
         *    .then(function(path) {
         *        alert('downloaded');
         *    });
         * </pre> 
         *  
         * @param {String} the unique package ID
         * @returns {String} path to the downloaded zip file.
         *
         */  
        fetch: function (id) {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "packageInstallApiBaseUrl",
                       "Fetch",
                       [{ packageGuid: id }])),
               'Failed to download package with guid ' + id);
        },
        
        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#createmanifest
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Creates a package manifest for a given folder of files. 
         * This manifest keeps track of all installed files and data items
         * so a package can be uninstalled at a later time.
         * After creating a manifest, you can use the ID to install files and data.
         * 
         * ##usage
         * <pre>
         * packageResource.createManifest("packages/id-of-install-file")
         *    .then(function(summary) {
         *        alert('unzipped');
         *    });
         * </pre> 
         *  
         * @param {String} folder the path to the temporary folder containing files
         * @returns {Int} the ID assigned to the saved package manifest
         *
         */ 
        import: function (package) {
           
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "Import"), package),
              'Failed to create package manifest for zip file ');
        }, 

        installFiles: function (package) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "InstallFiles"), package),
              'Failed to create package manifest for zip file ');
        }, 

        installData: function (package) {
           
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "InstallData"), package),
              'Failed to create package manifest for zip file ');
        }, 

        cleanUp: function (package) {
           
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "CleanUp"), package),
              'Failed to create package manifest for zip file ');
        }
    };
}

angular.module('umbraco.resources').factory('packageResource', packageResource);
