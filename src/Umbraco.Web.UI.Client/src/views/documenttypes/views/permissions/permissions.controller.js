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

    function PermissionsController($scope, contentTypeResource, iconHelper, contentTypeHelper, localizationService, overlayService) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;
        var childNodeSelectorOverlayTitle = "";

        vm.contentTypes = [];
        vm.selectedChildren = [];

        vm.overlayTitle = "";

        vm.addChild = addChild;
        vm.removeChild = removeChild;
        vm.toggleAllowAsRoot = toggleAllowAsRoot;
        vm.toggleAllowCultureVariants = toggleAllowCultureVariants;

        /* ---------- INIT ---------- */

        init();

        function init() {

            localizationService.localize("contentTypeEditor_chooseChildNode").then(function(value){
                childNodeSelectorOverlayTitle = value;
            });

            contentTypeResource.getAll().then(function(contentTypes){

                vm.contentTypes = contentTypes;

                // convert legacy icons
                iconHelper.formatContentTypeIcons(vm.contentTypes);

                vm.selectedChildren = contentTypeHelper.makeObjectArrayFromId($scope.model.allowedContentTypes, vm.contentTypes);

                if($scope.model.id === 0) {
                   contentTypeHelper.insertChildNodePlaceholder(vm.contentTypes, $scope.model.name, $scope.model.icon, $scope.model.id);
                }

            });

        }

        function addChild($event) {
            var childNodeSelectorOverlay = {
                view: "itempicker",
                title: childNodeSelectorOverlayTitle,
                availableItems: vm.contentTypes,
                selectedItems: vm.selectedChildren,
                position: "target",
                event: $event,
                submit: function(model) {
                    vm.selectedChildren.push(model.selectedItem);
                    $scope.model.allowedContentTypes.push(model.selectedItem.id);
                    overlayService.close();
                },
                close: function() {
                    overlayService.close();
                }
            };

            overlayService.open(childNodeSelectorOverlay);

        }

        function removeChild(selectedChild, index) {
           // remove from vm
           vm.selectedChildren.splice(index, 1);

           // remove from content type model
           var selectedChildIndex = $scope.model.allowedContentTypes.indexOf(selectedChild.id);
           $scope.model.allowedContentTypes.splice(selectedChildIndex, 1);
        }

        /**
         * Toggle the $scope.model.allowAsRoot value to either true or false
         */
        function toggleAllowAsRoot(){
            if($scope.model.allowAsRoot){
                $scope.model.allowAsRoot = false;
                return;
            }

            $scope.model.allowAsRoot = true;
        }

        function toggleAllowCultureVariants() {
            if ($scope.model.allowCultureVariant) {
                $scope.model.allowCultureVariant = false;
                return;
            }

            $scope.model.allowCultureVariant = true;
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
})();
