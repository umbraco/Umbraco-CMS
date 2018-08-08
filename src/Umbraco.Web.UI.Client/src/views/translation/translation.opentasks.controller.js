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
    
    $scope.download = function () {
        // [SEB] This probably needs to be changed by something else
        translationResource.getTaskXml($routeParams.id).then(function (content) {
            saveAs(new Blob([content], { type: "application/xml" }), $scope.task.properties[0].value);
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

    $scope.upload = function () {

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
