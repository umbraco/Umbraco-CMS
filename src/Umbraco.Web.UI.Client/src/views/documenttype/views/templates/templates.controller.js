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

        vm.availableTemplates =[];


        /* ---------- INIT ---------- */

        init();

        function init() {

            entityResource.getAll("Template").then(function(templates){
                vm.availableTemplates = templates;

                // if new content type make default template based on type
                if($scope.model.id === 0 && $scope.model.defaultTemplate === null) {

                  var template = {
                    "name": $scope.model.name,
                    "id": -1,
                    "icon": "icon-layout",
                    "alias": $scope.model.alias
                  };

                  // set fake content type template as default
                  $scope.model.defaultTemplate = template;

                  // push fake content type template to allowed templates
                  if(checkExistense($scope.model.allowedTemplates, template) === false ) {
                    $scope.model.allowedTemplates.push(template);
                  }

                  // push fake content type template to available templates
                  if(checkExistense(vm.availableTemplates, template) === false); {
                    vm.availableTemplates.push(template);
                  }

                }

            });

        }

        function checkExistense(array, item) {

          var found = false;

          angular.forEach(array, function(arrayItem){
            if(parseFloat(arrayItem.id) === parseFloat(item.id)) {
              found = true;
            }
          });

          if(found){
            return true;
          } else {
            return false;
          }

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
