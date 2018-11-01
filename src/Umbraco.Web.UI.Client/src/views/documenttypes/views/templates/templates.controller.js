/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.TemplatesController
 * @function
 *
 * @description
 * The controller for the content type editor templates sub view
 */
(function () {
    'use strict';

    function TemplatesController($scope, entityResource, contentTypeHelper, templateResource, $routeParams) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.availableTemplates = [];
        vm.canCreateTemplate = false;
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
            checkIfTemplateExists();

            callback();
        }

        vm.createTemplate = function () {
            templateResource.getScaffold(-1).then(function (template) {

                template.alias = $scope.model.alias;
                template.name = $scope.model.name;

                templateResource.save(template).then(function (savedTemplate) {

                    vm.availableTemplates.push(savedTemplate);
                    vm.canCreateTemplate = false;

                    $scope.model.allowedTemplates.push(savedTemplate);

                    if ($scope.model.defaultTemplate === null) {
                        $scope.model.defaultTemplate = savedTemplate;
                    }

                });

            });
        };

        function checkIfTemplateExists() {
            entityResource.getAll("Template").then(function (templates) {

                vm.availableTemplates = templates;

                var existingTemplate = vm.availableTemplates.find(function (availableTemplate) {
                    return availableTemplate.name === $scope.model.name;
                });

                vm.canCreateTemplate = existingTemplate ? false : true;

            });
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
