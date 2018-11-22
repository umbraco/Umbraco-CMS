(function() {
    'use strict';

    function PermissionsController($scope, mediaTypeResource, iconHelper, contentTypeHelper, localizationService, overlayService) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;
        var childNodeSelectorOverlayTitle = "";

        vm.mediaTypes = [];
        vm.selectedChildren = [];

        vm.addChild = addChild;
        vm.removeChild = removeChild;
        vm.toggle = toggle;

        /* ---------- INIT ---------- */

        init();

        function init() {

            localizationService.localize("contentTypeEditor_chooseChildNode").then(function(value){
                childNodeSelectorOverlayTitle = value;
            });

            mediaTypeResource.getAll().then(function(mediaTypes){

                vm.mediaTypes = mediaTypes;

                // convert legacy icons
                iconHelper.formatContentTypeIcons(vm.mediaTypes);

                vm.selectedChildren = contentTypeHelper.makeObjectArrayFromId($scope.model.allowedContentTypes, vm.mediaTypes);

                if($scope.model.id === 0) {
                   contentTypeHelper.insertChildNodePlaceholder(vm.mediaTypes, $scope.model.name, $scope.model.icon, $scope.model.id);
                }

            });

        }

        function addChild($event) {
            var childNodeSelectorOverlay = {
                view: "itempicker",
                title: childNodeSelectorOverlayTitle,
                availableItems: vm.mediaTypes,
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
        function toggle(){
            if($scope.model.allowAsRoot){
                $scope.model.allowAsRoot = false;
                return;
            }

            $scope.model.allowAsRoot = true;
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.MediaType.PermissionsController", PermissionsController);
})();
