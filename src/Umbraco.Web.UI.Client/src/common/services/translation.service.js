/**
 @ngdoc service
 * @name umbraco.services.translationService
 *
 * @description
 * <b>Added in Umbraco 7.8</b>. Application-wide service for handling tours.
 */
(function () {
    'use strict';

    var taskUserType = Object.freeze({
        ASSIGNEE: "assignee",
        OWNER: "owner"
    });

    function translationService($rootScope, $q, $window, translationResource, navigationService) {

        /**
         * 
         * @param {any} tasks
         */
        function downloadXml(tasks) {
            var defer = $q.defer();

            var filename = tasks.length === 1 ? tasks[0].properties[0].value.split(' ').join('_') : "all.xml";
            var ids = tasks.map(function (t) { return t.id; }).join(',');

            translationResource.getTasksXml(ids).then(function (content) {
                saveAs(new Blob([content], { type: "text/xml" }), filename);

                defer.resolve();
            });

            return defer.promise;
        }

        /**
         * 
         * @param {any} task
         */
        function preview(task) {
            // [SEB][ASK] Is it correct to reuse the preview page?
            var previewWindow = $window.open('preview/?init=true&id=' + task.entityId, 'umbpreview');

            previewWindow.location.href = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + task.entityId;
        }

        /**
         * 
         * @param {any} taskUserType
         */
        function refreshTree(taskUserType) {
            navigationService.syncTree({ tree: taskUserType + "TranslationTasks", path: ["-1"], forceReload: true });
        }

        /**
         * 
         * @param {any} file
         * @param {any} task
         */
        function upload(file, task) {
            var defer = $q.defer();

            if (file !== null) {
                var reader = new FileReader();

                reader.onload = function () {
                    $rootScope.$apply(function () {
                        try {
                            translationResource.submitTasks(task ? task.entityId : null, reader.result).then(function (result) {
                                defer.resolve(result);
                            });
                        }
                        catch (ex) {
                            defer.reject();
                        }
                    });
                };

                try {
                    reader.readAsDataURL(file[0]);
                }
                catch (ex) {
                    defer.reject();
                }
            }
            else {
                defer.resolve(null);
            }

            return defer.promise;
        }

        var service = {
            downloadXml: downloadXml,
            preview: preview,
            refreshTree: refreshTree,
            upload: upload,
            TaskUserType: taskUserType
        };

        return service;

    }

    angular.module("umbraco.services").factory("translationService", translationService);
})();
