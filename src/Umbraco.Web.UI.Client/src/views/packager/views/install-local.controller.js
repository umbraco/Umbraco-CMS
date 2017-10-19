(function () {
    "use strict";

    function PackagesInstallLocalController($scope, $route, $location, Upload, umbRequestHelper, packageResource, localStorageService, $timeout, $window, localizationService, $q) {

        var vm = this;
        vm.state = "upload";

        vm.localPackage = {};
        vm.installPackage = installPackage;
        vm.installState = {
            status: "",
            progress:0
        };
        vm.installCompleted = false;
        vm.zipFile = {
            uploadStatus: "idle",
            uploadProgress: 0,
            serverErrorMessage: null
        };

        $scope.handleFiles = function (files, event) {
            if (files) {
                for (var i = 0; i < files.length; i++) {
                    upload(files[i]);
                }
            }
        };

        function upload(file) {

            Upload.upload({
                url: umbRequestHelper.getApiUrl("packageInstallApiBaseUrl", "UploadLocalPackage"),
                fields: {},
                file: file
            }).progress(function (evt) {
                
                // hack: in some browsers the progress event is called after success
                // this prevents the UI from going back to a uploading state
                if(vm.zipFile.uploadStatus !== "done" && vm.zipFile.uploadStatus !== "error") {

                    // set view state to uploading
                    vm.state = 'uploading';

                    // calculate progress in percentage
                    var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);

                    // set percentage property on file
                    vm.zipFile.uploadProgress = progressPercentage;

                    // set uploading status on file
                    vm.zipFile.uploadStatus = "uploading";

                }

            }).success(function (data, status, headers, config) {

                if (data.notifications && data.notifications.length > 0) {

                    // set error status on file
                    vm.zipFile.uploadStatus = "error";

                    // Throw message back to user with the cause of the error
                    vm.zipFile.serverErrorMessage = data.notifications[0].message;

                } else {

                    // set done status on file
                    vm.zipFile.uploadStatus = "done";
                    loadPackage();
                    vm.zipFile.uploadProgress = 100;
                    vm.localPackage = data;
                }

            }).error(function (evt, status, headers, config) {

                // set status done
                vm.zipFile.uploadStatus = "error";

                // If file not found, server will return a 404 and display this message
                if (status === 404) {
                    vm.zipFile.serverErrorMessage = "File not found";
                }
                else if (status == 400) {
                    //it's a validation error
                    vm.zipFile.serverErrorMessage = evt.message;
                }
                else {
                    //it's an unhandled error
                    //if the service returns a detailed error
                    if (evt.InnerException) {
                        vm.zipFile.serverErrorMessage = evt.InnerException.ExceptionMessage;

                        //Check if its the common "too large file" exception
                        if (evt.InnerException.StackTrace && evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                            vm.zipFile.serverErrorMessage = "File too large to upload";
                        }

                    } else if (evt.Message) {
                        vm.zipFile.serverErrorMessage = evt.Message;
                    }
                }
            });
        }

        function loadPackage() {
            if (vm.zipFile.uploadStatus === "done") {
                vm.state = "packageDetails";
            }
        }

        function installPackage() {
            vm.installState.status = localizationService.localize("packager_installStateImporting");
            vm.installState.progress = "0";

            packageResource
                .import(vm.localPackage)                
                .then(function(pack) {
                        vm.installState.progress = "25";
                        vm.installState.status = localizationService.localize("packager_installStateInstalling");
                        return packageResource.installFiles(pack);
                    },
                    installError)
                .then(function(pack) {
                        vm.installState.status = localizationService.localize("packager_installStateRestarting");
                        vm.installState.progress = "50";
                        var deferred = $q.defer();

                        //check if the app domain is restarted ever 2 seconds
                        var count = 0;
                        function checkRestart() {
                          $timeout(function () {
                            packageResource.checkRestart(pack).then(function (d) {
                                count++;
                                //if there is an id it means it's not restarted yet but we'll limit it to only check 10 times
                                if (d.isRestarting && count < 10) {
                                  checkRestart();
                                }
                                else {
                                  //it's restarted!
                                  deferred.resolve(d);
                                }
                              },
                              installError);
                          }, 2000);
                        }

                        checkRestart();
                        
                        return deferred.promise;
                    }, installError)
                .then(function(pack) {
                    vm.installState.status = localizationService.localize("packager_installStateRestarting");
                        vm.installState.progress = "75";
                        return packageResource.installData(pack);
                    },
                    installError)
                .then(function(pack) {
                    vm.installState.status = localizationService.localize("packager_installStateComplete");
                        vm.installState.progress = "100";
                        return packageResource.cleanUp(pack);
                    },
                    installError)
                .then(function(result) {

                        if (result.postInstallationPath) {
                            //Put the redirect Uri in a cookie so we can use after reloading
                            localStorageService.set("packageInstallUri", result.postInstallationPath);
                        }
                        else {
                            //set to a constant value so it knows to just go to the installed view
                            localStorageService.set("packageInstallUri", "installed");
                        }

                        vm.installState.status = localizationService.localize("packager_installStateCompleted");
                        vm.installCompleted = true;
                        
                        

                    },
                    installError);
        }
        
        function installError() {
            //This will return a rejection meaning that the promise change above will stop
            return $q.reject();
        }

        vm.reloadPage = function() {
            //reload on next digest (after cookie)
            $timeout(function () {
                $window.location.reload(true);
            });
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.InstallLocalController", PackagesInstallLocalController);

})();
