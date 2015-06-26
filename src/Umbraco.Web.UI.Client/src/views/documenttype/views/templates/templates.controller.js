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

    function TemplatesController(entityResource) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.availableTemplates =[];


        /* ---------- INIT ---------- */

        init();

        function init() {

            entityResource.getAll("Template").then(function(templates){
                vm.availableTemplates = templates;
            });

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
