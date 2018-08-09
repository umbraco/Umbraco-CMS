/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.TranslationTasksController
 * @function
 * 
 * @description
 * Controls the recycle bin for content
 * 
 */

function TranslationTasksController($scope, $routeParams, $location, $window, translationResource, notificationsService) {
    $scope.loading = true;
    $scope.tasks = null;
    $scope.uploadButtonState = "init";

    $scope.download = function (task) {
        translationResource.getTaskXml(task.id).then(function (content) {
            saveAs(new Blob([content], { type: "text/xml" }), task.properties[0].value.split(' ').join('_'));
        })
    }

    $scope.downloadAll = function () {
        translationResource.getTasksXml($scope.tasks.map(function (t) { return t.id; }).join(',')).then(function (content) {
            saveAs(new Blob([content], { type: "text/xml" }), "all.xml");
        });
    }

    $scope.openTask = function (task) {
        $location.path('/translation/translation/' + ($routeParams.id == "assignee" ? "opentasks/" : "yourtasks/") + task.id);
    }

    $scope.upload = function (file) {
        if (file !== null) {
            $scope.uploadButtonState = "busy";
            // [SEB] Error checking
            var reader = new FileReader();

            reader.onload = function () {
                $scope.$apply(function () {
                    try {
                        translationResource.submitTasks(-1, -1, file.name, reader.result).then(function (result) {

                            console.log(result)

                            for (var i = $scope.tasks.length - 1; i >= 0; --i) {
                                var task = $scope.tasks[i];
                                var r = result.filter(function (r) { return r.taskId === task.id });

                                if (r[0].entityId !== null) {
                                    task.done = true;
                                    task.entityId = r[0].entityId;
                                }
                                else {
                                    // [SEB] Error handling
                                }
                            }

                            $scope.uploadButtonState = "success";
                            notificationsService.success("Upload", "The task has been bla bla");

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

    $scope.preview = function (task) {
        // [SEB][ASK] Is it correct to reuse the preview page?
        var previewWindow = $window.open('preview/?init=true&id=' + task.entityId, 'umbpreview');
        var redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + task.entityId;
        previewWindow.location.href = redirect;
    }

    function $onInit() {
        ($routeParams.id == "assignee" ? translationResource.getAllTaskAssignedToMe() : translationResource.getAllTaskCreatedByMe()).then(function (tasks) {
            $scope.tasks = tasks;
            $scope.loading = false;
        })
    }

    $onInit();
}

angular.module('umbraco').controller("Umbraco.Editors.Translation.TasksController", TranslationTasksController);
