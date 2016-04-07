(function() {
    'use strict';

    function PermissionsController($scope, mediaTypeResource, iconHelper, contentTypeHelper, localizationService) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;
        var childNodeSelectorOverlayTitle = "";

        vm.mediaTypes = [];
        vm.selectedChildren = [];

        vm.addChild = addChild;
        vm.removeChild = removeChild;

        /* ---------- INIT ---------- */

        init();

        function init() {

            childNodeSelectorOverlayTitle = localizationService.localize("contentTypeEditor_chooseChildNode");

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
            vm.childNodeSelectorOverlay = {
                view: "itempicker",
                title: childNodeSelectorOverlayTitle,
                availableItems: vm.mediaTypes,
                selectedItems: vm.selectedChildren,
                event: $event,
                show: true,
                submit: function(model) {
                    vm.selectedChildren.push(model.selectedItem);
                    $scope.model.allowedContentTypes.push(model.selectedItem.id);
                    vm.childNodeSelectorOverlay.show = false;
                    vm.childNodeSelectorOverlay = null;
                }
            };
        }

        function removeChild(selectedChild, index) {
           // remove from vm
           vm.selectedChildren.splice(index, 1);

           // remove from content type model
           var selectedChildIndex = $scope.model.allowedContentTypes.indexOf(selectedChild.id);
           $scope.model.allowedContentTypes.splice(selectedChildIndex, 1);
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.MediaType.PermissionsController", PermissionsController);
})();
