angular.module("umbraco")
    .controller("Umbraco.Editors.DocumentTypes.ImportController",
        function ($scope, contentTypeResource, navigationService, Upload, umbRequestHelper) {
            var vm = this;
            vm.serverErrorMessage = "";
            vm.state = "upload";
            vm.model = {};
            vm.uploadStatus = "";

            vm.cancelButtonLabel = "cancel";

            $scope.handleFiles = function (files, event, invalidFiles) {
                if (files && files.length > 0) {
                    $scope.upload(files[0]);
                }
            };

            $scope.upload = function (file) {
                Upload.upload({
                    url: umbRequestHelper.getApiUrl("contentTypeApiBaseUrl", "Upload"),
                    fields: {},
                    file: file
                }).success(function (data, status, headers, config) {
                    if (data.notifications && data.notifications.length > 0) {

                        // set error status on file
                        vm.uploadStatus = "error";

                        // Throw message back to user with the cause of the error
                        vm.serverErrorMessage = data.notifications[0].message;
                    } else {

                        // set done status on file
                        vm.uploadStatus = "done";
                        vm.model = data;
                        vm.state = "confirm";
                    }
                }).error(function (evt, status, headers, config) {

                    // set status done
                    $scope.uploadStatus = "error";

                    // If file not found, server will return a 404 and display this message
                    if (status === 404) {
                        $scope.serverErrorMessage = "File not found";
                    }
                    else if (status == 400) {
                        //it's a validation error
                        $scope.serverErrorMessage = evt.message;
                    }
                    else {
                        //it's an unhandled error
                        //if the service returns a detailed error
                        if (evt.InnerException) {
                            $scope.serverErrorMessage = evt.InnerException.ExceptionMessage;

                            //Check if its the common "too large file" exception
                            if (evt.InnerException.StackTrace && evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                                $scope.serverErrorMessage = "File too large to upload";
                            }

                        } else if (evt.Message) {
                            $scope.serverErrorMessage = evt.Message;
                        }
                    }
                });
            };

            $scope.import = function () {
                contentTypeResource.import(vm.model.tempFileName);
                vm.state = "done";

                vm.cancelButtonLabel = "general_close";
            }

            $scope.close = function () {
                navigationService.hideDialog();
            };

        });
