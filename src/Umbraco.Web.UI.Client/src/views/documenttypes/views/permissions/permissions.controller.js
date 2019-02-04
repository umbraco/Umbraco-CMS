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
        vm.toggleIsElement = toggleIsElement;

        /* ---------- INIT ---------- */

        init();

        function init() {

            localizationService.localize("contentTypeEditor_chooseChildNode").then(function(value){
                childNodeSelectorOverlayTitle = value;
            });

            contentTypeResource.getAll().then(function(contentTypes){
                vm.contentTypes = _.where(contentTypes, {isElement: false});

                // convert legacy icons
                iconHelper.formatContentTypeIcons(vm.contentTypes);

                vm.selectedChildren = contentTypeHelper.makeObjectArrayFromId($scope.model.allowedContentTypes, contentTypes);

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

        // note: "safe toggling" here ie handling cases where the value is undefined, etc

        function toggleAllowAsRoot() {
            $scope.model.allowAsRoot = $scope.model.allowAsRoot ? false : true;
        }

        function toggleAllowCultureVariants() {
            $scope.model.allowCultureVariant = $scope.model.allowCultureVariant ? false : true;
        }

        function toggleIsElement() {
            $scope.model.isElement = $scope.model.isElement ? false : true;
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
})();
