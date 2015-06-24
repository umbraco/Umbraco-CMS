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

    function PermissionsController($scope, contentTypeResource, $log, iconHelper) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.removeAllowedChildNode = removeAllowedChildNode;
        vm.addItemOverlay = addItemOverlay;

        vm.contentTypes = [];

        /* ---------- INIT ---------- */

        init();

        function init() {

            contentTypeResource.getAll().then(function(contentTypes){
                vm.contentTypes = contentTypes;
            });

        }

        function removeAllowedChildNode(selectedContentType) {

            // splice from array
            var selectedContentTypeIndex = $scope.model.allowedContentTypes.indexOf(selectedContentType);
            $scope.model.allowedContentTypes.splice(selectedContentTypeIndex, 1);

        }

        function addItemOverlay($event) {

            vm.showDialog = false;
            vm.dialogModel = {};
            vm.dialogModel.title = "Choose content type";
            vm.dialogModel.contentTypes = vm.contentTypes;
            vm.dialogModel.allowedContentTypes = $scope.model.allowedContentTypes;
            vm.dialogModel.event = $event;
            vm.dialogModel.view = "views/documentType/dialogs/contenttypes/contenttypes.html";
            vm.showDialog = true;

            vm.dialogModel.chooseContentType = function(selectedContentType) {

                // format content type to match service
                var reformatedContentType = {
                    "name": selectedContentType.name,
                    "id": {
                        "m_boxed": {
                            "m_value": selectedContentType.id
                        }
                    },
                    "icon": selectedContentType.icon,
                    "key": selectedContentType.key,
                    "alias": selectedContentType.alias
                };

                // push to content type model
                $scope.model.allowedContentTypes.push(reformatedContentType);

                vm.showDialog = false;
                vm.dialogModel = null;
            };

            vm.dialogModel.close = function(){
                vm.showDialog = false;
                vm.dialogModel = null;
            };
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
})();
