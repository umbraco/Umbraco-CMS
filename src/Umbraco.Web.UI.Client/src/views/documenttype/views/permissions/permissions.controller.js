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
        vm.parent = {};

        /* ---------- INIT ---------- */

        init();

        function init() {

            contentTypeResource.getAll().then(function(contentTypes){

                vm.contentTypes = contentTypes;

                // convert legacy icons
                iconHelper.formatContentTypeIcons(vm.contentTypes);

                // set parent
                vm.parent = {
                    "name": $scope.model.name,
                    "icon": $scope.model.icon
                };

            });

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
})();
