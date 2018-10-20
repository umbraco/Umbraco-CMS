/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.TemplatesController
 * @function
 *
 * @description
 * The controller for the content type editor templates sub view
 */
(function() {
    'use strict';

    function TemplatesController($scope, entityResource, contentTypeHelper, templateResource, $routeParams) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.availableTemplates = [];
        vm.updateTemplatePlaceholder = false;


        /* ---------- INIT ---------- */

        init(function () {

            // update placeholder template information on new doc types
            if (!$routeParams.notemplate && $scope.model.id === 0) {
                vm.updateTemplatePlaceholder = true;
                vm.availableTemplates = contentTypeHelper.insertTemplatePlaceholder(vm.availableTemplates);
            }
        });

        function init(callback) {

            entityResource.getAll("Template").then(function(templates){

                vm.availableTemplates = templates;

                callback();

            });

        }

        vm.createTemplate = function () {
            templateResource.getScaffold(-1).then(function (template) {

                template.alias = $scope.model.alias;
                template.name = $scope.model.name;

                templateResource.save(template).then(function (savedTemplate) {

                    init(function () {

                        var newTemplate = vm.availableTemplates.filter(function (t) { return t.id === savedTemplate.id });
                        if (newTemplate.length > 0) {
                            $scope.model.allowedTemplates.push(newTemplate[0]);

                            if ($scope.model.defaultTemplate === null) {
                                $scope.model.defaultTemplate = newTemplate[0];
                            }
                        }

                    });

                });

            });
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
