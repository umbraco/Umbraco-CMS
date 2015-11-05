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

    function PermissionsController($scope, contentTypeResource, iconHelper, contentTypeHelper) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.contentTypes = [];
        vm.selectedChildren = [];

        vm.addChild = addChild;
        vm.removeChild = removeChild;

        /* ---------- INIT ---------- */

        init();

        function init() {

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
           vm.childNodeSelectorOverlay = {};
           vm.childNodeSelectorOverlay.view = "itempicker";
           vm.childNodeSelectorOverlay.title = "Choose child node";
           vm.childNodeSelectorOverlay.availableItems = vm.contentTypes;
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

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
})();
