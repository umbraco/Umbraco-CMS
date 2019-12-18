/**
    * @ngdoc service
    * @name umbraco.resources.packageInstallResource
    * @description handles data for package installations
    **/
function packageResource($q, $http, umbDataFormatter, umbRequestHelper) {
    
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#getInstalled
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Gets a list of installed packages       
         */
        getInstalled: function() {
            return umbRequestHelper.resourcePromise(
               $http.get(
                   umbRequestHelper.getApiUrl(
                       "packageApiBaseUrl",
                       "GetInstalled")),
               'Failed to get installed packages');
        },

        validateInstalled: function (name, version) {
            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "packageInstallApiBaseUrl",
                       "ValidateInstalled", { name: name, version: version })),
               'Failed to validate package ' + name);
        },

        uninstall: function(packageId) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "Uninstall", { packageId: packageId })),
              'Failed to uninstall package');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#fetchPackage
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Downloads a package file from our.umbraco.com to the website server.
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
        import: function (umbPackage) {
           
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "Import"), umbPackage),
              'Failed to install package. Error during the step "Import" ');
        }, 

        installFiles: function (umbPackage) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "InstallFiles"), umbPackage),
              'Failed to install package. Error during the step "InstallFiles" ');
        }, 

        checkRestart: function (umbPackage) {

          return umbRequestHelper.resourcePromise(
            $http.post(
              umbRequestHelper.getApiUrl(
                "packageInstallApiBaseUrl",
                "CheckRestart"), umbPackage),
            'Failed to install package. Error during the step "CheckRestart" ');
        }, 

        installData: function (umbPackage) {
           
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "InstallData"), umbPackage),
              'Failed to install package. Error during the step "InstallData" ');
        }, 

        cleanUp: function (umbPackage) {
           
            return umbRequestHelper.resourcePromise(
                $http.post(
                  umbRequestHelper.getApiUrl(
                      "packageInstallApiBaseUrl",
                      "CleanUp"), umbPackage),
              'Failed to install package. Error during the step "CleanUp" ');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#getCreated
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Gets a list of created packages       
         */
        getAllCreated: function() {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "packageApiBaseUrl",
                        "GetCreatedPackages")),
                'Failed to get created packages');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#getCreatedById
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Gets a created package by id       
         */
        getCreatedById: function(id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "packageApiBaseUrl",
                        "GetCreatedPackageById", 
                        { id: id })),
                'Failed to get package');
        },

        getInstalledById: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "packageApiBaseUrl",
                        "GetInstalledPackageById",
                        { id: id })),
                'Failed to get package');
        },

        getEmpty: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "packageApiBaseUrl",
                        "getEmpty")),
                'Failed to get scaffold');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#savePackage
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Creates or updates a package
         */
        savePackage: function (umbPackage) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "packageApiBaseUrl",
                        "PostSavePackage"), umbPackage),
                'Failed to create package');
        },

        /**
         * @ngdoc method
         * @name umbraco.resources.packageInstallResource#deleteCreatedPackage
         * @methodOf umbraco.resources.packageInstallResource
         *
         * @description
         * Detes a created package
         */
        deleteCreatedPackage: function (packageId) {
            return umbRequestHelper.resourcePromise(
               $http.post(
                   umbRequestHelper.getApiUrl(
                       "packageApiBaseUrl",
                       "DeleteCreatedPackage", { packageId: packageId })),
               'Failed to delete package ' + packageId);
        }
    };
}

angular.module('umbraco.resources').factory('packageResource', packageResource);
