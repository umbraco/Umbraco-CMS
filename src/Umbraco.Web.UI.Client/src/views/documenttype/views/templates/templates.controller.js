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

    function TemplatesController($scope, entityResource, contentTypeHelper) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.availableTemplates =[];


        /* ---------- INIT ---------- */

        init();

        function init() {

            entityResource.getAll("Template").then(function(templates){
                vm.availableTemplates = templates;

                if($scope.model.id === 0) {

                  // update template placeholder name
                  $scope.model = contentTypeHelper.updateTemplateHolder($scope.model, true);

                  // add template placeholder to available templates
                  vm.availableTemplates = contentTypeHelper.insertTemplateHolder($scope.model, vm.availableTemplates);

                }

            });

        }

        // watch for changes in content type name change
        $scope.$watch('model.name', function(newValue, oldValue){

          // update template placeholder name
          $scope.model = contentTypeHelper.updateTemplateHolder($scope.model, true);

          vm.availableTemplates = contentTypeHelper.insertTemplateHolder($scope.model, vm.availableTemplates);

        });

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
