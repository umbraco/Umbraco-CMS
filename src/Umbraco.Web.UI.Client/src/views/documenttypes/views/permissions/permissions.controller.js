/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
(function() {
    'use strict';

    function PermissionsController($scope, contentTypeResource, iconHelper) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.contentTypes = [];

        /* ---------- INIT ---------- */

        init();

        function init() {

            contentTypeResource.getAll().then(function(contentTypes){

                vm.contentTypes = contentTypes;

                // convert legacy icons
                iconHelper.formatContentTypeIcons(vm.contentTypes);

            });

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
})();
