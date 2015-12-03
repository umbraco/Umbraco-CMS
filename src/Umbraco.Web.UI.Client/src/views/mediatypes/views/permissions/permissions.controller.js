(function() {
    'use strict';

    function PermissionsController($scope, mediaTypeResource, iconHelper, contentTypeHelper) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.mediaTypes = [];
        vm.selectedChildren = [];

        vm.addChild = addChild;
        vm.removeChild = removeChild;

        /* ---------- INIT ---------- */

        init();

        function init() {

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
           vm.childNodeSelectorOverlay = {};
           vm.childNodeSelectorOverlay.view = "itempicker";
           vm.childNodeSelectorOverlay.title = "Choose child node";
           vm.childNodeSelectorOverlay.availableItems = vm.mediaTypes;
           vm.childNodeSelectorOverlay.selectedItems = vm.selectedChildren;
           vm.childNodeSelectorOverlay.event = $event;
           vm.childNodeSelectorOverlay.show = true;

           vm.childNodeSelectorOverlay.chooseItem = function(item) {

             vm.selectedChildren.push(item);
             $scope.model.allowedContentTypes.push(item.id);

             vm.childNodeSelectorOverlay.show = false;
             vm.childNodeSelectorOverlay = null;

           };

           vm.childNodeSelectorOverlay.close = function(oldModel) {
             vm.childNodeSelectorOverlay.show = false;
             vm.childNodeSelectorOverlay = null;
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
