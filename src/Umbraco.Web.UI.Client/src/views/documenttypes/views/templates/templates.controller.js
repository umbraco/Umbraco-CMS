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

    function TemplatesController($scope, entityResource, contentTypeHelper, templateResource, contentTypeResource, $routeParams) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.availableTemplates = [];
        vm.canCreateTemplate = false;
        vm.updateTemplatePlaceholder = false;
        vm.loadingTemplates = false;
        vm.isElement = $scope.model.isElement;

        vm.createTemplate = createTemplate;

        /* ---------- INIT ---------- */

        function onInit() {
            vm.loadingTemplates = true;
            entityResource.getAll("Template").then(function (templates) {

                vm.availableTemplates = templates;

                // update placeholder template information on new doc types
                if (!$routeParams.notemplate && $scope.model.id === 0) {
                    vm.updateTemplatePlaceholder = true;
                    vm.availableTemplates = contentTypeHelper.insertTemplatePlaceholder(vm.availableTemplates);
                }

                vm.loadingTemplates = false;
                checkIfTemplateExists();

            });

        }

        function createTemplate() {

            vm.createTemplateButtonState = "busy";

            contentTypeResource.createDefaultTemplate($scope.model.id).then(function (savedTemplate) {
                // add icon
                savedTemplate.icon = "icon-layout";
                    
                vm.availableTemplates.push(savedTemplate);
                vm.canCreateTemplate = false;

                $scope.model.allowedTemplates.push(savedTemplate);

                if ($scope.model.defaultTemplate === null) {
                    $scope.model.defaultTemplate = savedTemplate;
                }

                vm.createTemplateButtonState = "success";

            }, function() {
                vm.createTemplateButtonState = "error";
            });

        };

        function checkIfTemplateExists() {
            if ($scope.model.id === 0) {
                return;
            }

            var existingTemplate = vm.availableTemplates.find(function (availableTemplate) {
                return (availableTemplate.name === $scope.model.name || availableTemplate.placeholder);
            });

            vm.canCreateTemplate = existingTemplate ? false : true;
        }

        var unbindWatcher = $scope.$watch("model.isElement",
            function(newValue, oldValue) {
                vm.isElement = newValue;
            }
        );
        $scope.$on("$destroy", function () {
            unbindWatcher();
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
