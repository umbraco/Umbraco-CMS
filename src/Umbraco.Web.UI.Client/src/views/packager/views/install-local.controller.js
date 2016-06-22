(function () {
    "use strict";

    function PackagesInstallLocalController($scope, $route, $location, Upload, umbRequestHelper, packageResource, $cookieStore, $timeout) {

        var vm = this;
        vm.state = "upload";

        vm.localPackage = {};
        vm.installPackage = installPackage;
        vm.installState = {
            status: ""
        };
        vm.zipFile = {
            uploadStatus: "idle",
            uploadProgress: 0,
            serverErrorMessage: null
        };

        $scope.handleFiles = function (files, event) {
            for (var i = 0; i < files.length; i++) {
                upload(files[i]);
            }
        };

        function upload(file) {

            Upload.upload({
                url: umbRequestHelper.getApiUrl("packageInstallApiBaseUrl", "UploadLocalPackage"),
                fields: {},
                file: file
            }).progress(function (evt) {

                // calculate progress in percentage
                var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);

                // set percentage property on file
                vm.zipFile.uploadProgress = progressPercentage;

                // set uploading status on file
                vm.zipFile.uploadStatus = "uploading";

            }).success(function (data, status, headers, config) {

                if (data.notifications && data.notifications.length > 0) {

                    // set error status on file
                    vm.zipFile.uploadStatus = "error";

                    // Throw message back to user with the cause of the error
                    vm.zipFile.serverErrorMessage = data.notifications[0].message;

                    //TODO: Handle the error in UI

                } else {

                    // set done status on file
                    vm.zipFile.uploadStatus = "done";
                    loadPackage();
                    vm.localPackage = data;
                }

            }).error(function (evt, status, headers, config) {

                //TODO: Handle the error in UI

                // set status done
                vm.zipFile.uploadStatus = "error";

                //if the service returns a detailed error
                if (evt.InnerException) {
                    vm.zipFile.serverErrorMessage = evt.InnerException.ExceptionMessage;

                    //Check if its the common "too large file" exception
                    if (evt.InnerException.StackTrace && evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                        vm.zipFile.serverErrorMessage = "File too large to upload";
                    }

                } else if (evt.Message) {
                    file.serverErrorMessage = evt.Message;
                }

                // If file not found, server will return a 404 and display this message
                if (status === 404) {
                    vm.zipFile.serverErrorMessage = "File not found";
                }

            });
        }

        function loadPackage() {
            if (vm.zipFile.uploadStatus === "done") {
                vm.state = "packageDetails";
            }
        }

        function installPackage() {
            vm.installState.status = "Installing";

            //TODO: If any of these fail, will they keep calling the next one?
            packageResource
                .installFiles(vm.localPackage)
                .then(function(pack) {
                        vm.installState.status = "Importing...";
                        return packageResource.import(pack);
                    },
                    installError)
                .then(function(pack) {
                        vm.installState.status = "Installing...";
                        return packageResource.installFiles(pack);
                    },
                    installError)
                .then(function(pack) {
                        vm.installState.status = "Restarting, please wait...";
                        return packageResource.installData(pack);
                    },
                    installError)
                .then(function(pack) {
                        vm.installState.status = "All done, your browser will now refresh";
                        return packageResource.cleanUp(pack);
                    },
                    installError)
                .then(function(result) {

                        if (result.postInstallationPath) {
                            //Put the redirect Uri in a cookie so we can use after reloading
                            window.localStorage.setItem("packageInstallUri", result.postInstallationPath);
                        }
                        //reload on next digest (after cookie)
                        $timeout(function() {
                            window.location.reload(true);
                        });


                    },
                    installError);
        }

        function installError() {
            //TODO: Need to do something about this?    
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.InstallLocalController", PackagesInstallLocalController);

})();
