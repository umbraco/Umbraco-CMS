(function () {
    'use strict';

    function UmbTranslationTaskController($scope, $window, $location, $routeParams, $q, $timeout, localizationService, contentResource, translationResource, translationService, notificationsService, treeService, navigationService, appState) {
        $scope.labels = {};
        $scope.loading = true;
        $scope.closeButtonState = "init";
        $scope.uploadButtonState = "init";
        $scope.uploadedFile = null;
        $scope.previewId = null;
        $scope.outcome = null;

        /**
         * Download the task as XML
         * */
        $scope.download = function () {
            translationService.downloadXml([$scope.task]);
        }

        /**
         * Close the task
         * */
        $scope.close = function () {
            $scope.closeButtonState = "busy";

            translationResource.closeTask($routeParams.id).then(function () {
                $scope.closeButtonState = "success";
                $scope.task.closed = true;
                removeCurrentTreeNode();
            });
        }

        /**
         * Upload the file as translation
         * 
         * @param {any} file File
         */
        $scope.upload = function (file) {
            $scope.uploadButtonState = "busy";

            translationService.upload(file, $scope.task).then(function (report) {
                $scope.outcome = report.outcome;

                if (report.outcome[$scope.task.id]) {
                    $scope.task.closed = true;
                    $scope.task.entityId = report.outcome[$scope.task.id];
                    $scope.uploadButtonState = "success";
                    removeCurrentTreeNode();
                }
                else {
                    $scope.uploadButtonState = "error";
                }
            });
        }

        $scope.preview = function () {
            translationService.preview($scope.task);
        }

        function removeCurrentTreeNode() {
            var selectedNode = appState.getTreeState("selectedNode");

            if (selectedNode) {
                treeService.removeNode(selectedNode);
            }
        }

        function $onInit() {
            $q.all([
                localizationService.localizeMany([$scope.type == translationService.TaskUserType.ASSIGNEE ? 'translation_assignedTasks' : 'translation_ownedTasks']),
                translationResource.getTaskById($routeParams.id)
            ]).then(function (result) {
                $scope.labels.pageTitle = result[0][0];
                $scope.task = result[1];

                $scope.loading = false;

                $timeout(function () { navigationService.syncTree({ tree: $scope.type + "TranslationTasks", path: ["-1", $routeParams.id], forceReload: true, activate: true }); });
            });
        }

        $onInit();
    }

    angular.module('umbraco.directives').directive('umbTranslationTask', function () {
        return {
            restrict: "E",
            replace: true,
            controller: UmbTranslationTaskController,
            templateUrl: "views/components/translation/umb-translation-task.html",
            scope: {
                type: "@"
            }
        };
    });
})();
