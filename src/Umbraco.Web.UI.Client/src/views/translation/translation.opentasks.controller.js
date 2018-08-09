/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.RecycleBinController
 * @function
 * 
 * @description
 * Controls the recycle bin for content
 * 
 */

function TranslationOpenTasksController($scope, $window, $location, $routeParams, $q, localizationService, contentResource, translationResource, notificationsService, treeService, appState) {
    $scope.labels = {};
    $scope.loading = true;
    $scope.closeButtonState = "init";
    $scope.uploadButtonState = "init";
    $scope.uploadedFile = null;
    $scope.previewId = null;

    $scope.download = function () {
        // [SEB] Use translation.service ?
        translationResource.getTaskXml($routeParams.id).then(function (content) {
            saveAs(new Blob([content], { type: "application/xml" }), $scope.task.properties[0].value.split(' ').join('_'));
        })
    }

    $scope.close = function () {
        $scope.closeButtonState = "busy";

        translationResource.closeTask($routeParams.id).then(function () {
            notificationsService.success("Close", "Cool");
            $scope.closeButtonState = "success";
            $scope.task.closed = true;
            // [SEB]: Probably refresh the tree
        }, function () {
            notificationsService.error("Close", "Uncool");
            $scope.closeButtonState = "error";
        });
    }

    $scope.upload = function (file) {
        if (file !== null) {
            $scope.uploadButtonState = "busy";
            // [SEB] Error checking
            var reader = new FileReader();

            reader.onload = function () {
                $scope.$apply(function () {
                    try {
                        translationResource.submitTasks($routeParams.id, $scope.task.nodeId, file.name, reader.result).then(function (id) {
                            $scope.uploadButtonState = "success";
                            $scope.task.closed = true;
                            notificationsService.success("Upload", "The task has been bla bla");

                            $scope.previewId = id;
                            // [SEB] refresh the properties
                        });
                    }
                    catch (ex) {
                        $scope.uploadButtonState = "error";
                        // [SEB] Translation
                        notificationsService.error("Upload", "The provided file is not valid");
                    }
                })
            };

            try {
                reader.readAsDataURL(file[0]);
            }
            catch (ex) {
                $scope.uploadButtonState = "error";
                // [SEB] Translation
                notificationsService.error("Upload", "The provided file could not be read");
            }
        }
    }

    $scope.preview = function () {
        // [SEB][ASK] Is it correct to reuse the preview page?
        var previewWindow = $window.open('preview/?init=true&id=' + $scope.previewId, 'umbpreview');
        var redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + $scope.previewId;
        previewWindow.location.href = redirect;
    }

    function $onInit() {
        // [SEB] Sanity checks

        $q.all([
            localizationService.localizeMany(['translation_assignedTasks']),
            translationResource.getTaskById($routeParams.id)
        ]).then(function (result) {
            $scope.labels.pageTitle = result[0][0];
            $scope.task = result[1];

            $scope.loading = false;
        });
    }

    $onInit();
}

angular.module('umbraco').controller("Umbraco.Editors.Translation.OpenTasksController", TranslationOpenTasksController);
