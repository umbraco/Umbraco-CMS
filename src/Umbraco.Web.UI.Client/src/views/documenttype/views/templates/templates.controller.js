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

    function TemplatesController($scope, entityResource) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.removeTemplate = removeTemplate;
        vm.removeDefaultTemplate = removeDefaultTemplate;
        vm.openTemplatesPicker = openTemplatesPicker;
        vm.setAsDefaultTemplate = setAsDefaultTemplate;

        vm.availableTemplates =[];


        /* ---------- INIT ---------- */

        init();

        function init() {

            entityResource.getAll("Template").then(function(templates){
                vm.availableTemplates = templates;
            });

        }

        function removeTemplate(selectedTemplate) {

            // splice from array
            var selectedTemplateIndex = $scope.model.allowedTemplates.indexOf(selectedTemplate);
            $scope.model.allowedTemplates.splice(selectedTemplateIndex, 1);

        }

        function removeDefaultTemplate() {

            // remove default template from array - it will be the last template so we can clear the array
            $scope.model.allowedTemplates = [];

            // remove as default template
            $scope.model.defaultTemplate = null;

        }

        function openTemplatesPicker($event, setAsDefaultTemplateBool){
            vm.showDialog = false;
            vm.dialogModel = {};
            vm.dialogModel.title = "Choose template";
            vm.dialogModel.availableTemplates = vm.availableTemplates;
            vm.dialogModel.allowedTemplates = $scope.model.allowedTemplates;
            vm.dialogModel.event = $event;
            vm.dialogModel.view = "views/documentType/dialogs/templates/templates.html";
            vm.showDialog = true;

            vm.dialogModel.chooseTemplate = function(selectedTemplate) {

                // push template as allowed template
                $scope.model.allowedTemplates.push(selectedTemplate);

                // if true set template as default template
                if(setAsDefaultTemplateBool) {
                    setAsDefaultTemplate(selectedTemplate);
                }

                // hide dialog
                vm.showDialog = false;
                vm.dialogModel = null;
            };

            vm.dialogModel.close = function(){
                vm.showDialog = false;
                vm.dialogModel = null;
            };

        }

        function setAsDefaultTemplate(template) {
            $scope.model.defaultTemplate = {};
            $scope.model.defaultTemplate = template;
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
