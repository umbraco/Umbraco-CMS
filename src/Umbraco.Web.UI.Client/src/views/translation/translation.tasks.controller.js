/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.TranslationTasksController
 * @function
 * 
 * @description
 * Controls the list of all assigned/owned translations of a user
 * 
 */

function TranslationTasksController($scope, $routeParams, $location, $window, $q, localizationService, translationResource, translationService, notificationsService, treeService) {
    $scope.loading = true;
    $scope.tasks = null;
    $scope.uploadButtonState = "init";
    $scope.labels = {}
    $scope.report = null;

    /**
     * Download the XML of one or several tasks
     * 
     * @param {any} task Array of task
     */
    $scope.download = function (tasks) {
        translationService.downloadXml(tasks);
    }

    /**
     * Hide the import summary
     * */
    $scope.hideReport = function () {
        $scope.report = null;
    }

    /**
     * Open the page for a task
     * 
     * @param {any} task Task
     */
    $scope.openTask = function (task) {
        $location.path('/translation/translation/' + $routeParams.id + "/" + task.id);
    }

    /**
     * Upload a file for import
     * 
     * @param {any} file Imported file
     */
    $scope.upload = function (file) {
        $scope.uploadButtonState = "busy";

        translationService.upload(file).then(function (report) {
            $scope.report = report;

            for (var i = 0, length = $scope.tasks.length; i < length; ++i) {
                var task = $scope.tasks[i];
                var outcome = report.outcome[task.id];

                if (outcome) {
                    task.done = true;
                    task.entityId = outcome.entityId;
                }
            }

            $scope.tasks.sort(function (t1, t2) {
                if (t1.done === true && !t2.done) {
                    return 1;
                }

                if (t2.done === true && !t1.done) {
                    return -1;
                }

                return 0;
            });

            // [SEB][ASK]: The tree does not refresh
            translationService.refreshTree($routeParams.id);

            $scope.uploadButtonState = "success";
        });
    }

    /**
     * Preview an imported tasl
     * 
     * @param {any} task Task
     */
    $scope.preview = function (task) {
        translationService.preview(task);
    }

    function $onInit() {
        $q.all([
            $routeParams.id == translationService.TaskUserType.ASSIGNEE ? translationResource.getAllTaskAssignedToMe() : translationResource.getAllTaskCreatedByMe(),
            localizationService.localize($routeParams.id == translationService.TaskUserType.ASSIGNEE ? "translation_assignedTasks" : "translation_ownedTasks")
        ]).then(function (result) {
            $scope.tasks = result[0];
            $scope.labels.pageTitle = result[1];
            $scope.loading = false;
        });
    }

    $onInit();
}

angular.module('umbraco').controller("Umbraco.Editors.Translation.TasksController", TranslationTasksController);
