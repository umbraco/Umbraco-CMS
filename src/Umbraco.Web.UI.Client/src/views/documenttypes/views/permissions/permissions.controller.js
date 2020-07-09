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

    function PermissionsController($scope, $timeout, contentTypeResource, iconHelper, contentTypeHelper, localizationService, overlayService) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.contentTypes = [];
        vm.selectedChildren = [];
        vm.showAllowSegmentationOption = Umbraco.Sys.ServerVariables.umbracoSettings.showAllowSegmentationForDocumentTypes || false;

        vm.addChild = addChild;
        vm.removeChild = removeChild;
        vm.sortChildren = sortChildren;
        vm.toggleAllowAsRoot = toggleAllowAsRoot;
        vm.toggleAllowCultureVariants = toggleAllowCultureVariants;
        vm.toggleAllowSegmentVariants = toggleAllowSegmentVariants;
        vm.canToggleIsElement = false;
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

            // Can only switch to an element type if there are no content nodes already created from the type.
            if ($scope.model.id > 0 && !$scope.model.isElement ) {
                contentTypeResource.hasContentNodes($scope.model.id).then(function (result) {
                    vm.canToggleIsElement = !result;
                });
            } else {
                vm.canToggleIsElement = true;
            }
        }

        function addChild($event) {
            
            const dialog = {
                view: "itempicker",
                availableItems: vm.contentTypes,
                selectedItems: vm.selectedChildren,
                position: "target",
                event: $event,
                submit: function (model) {
                    if (model.selectedItem) {
                        vm.selectedChildren.push(model.selectedItem);
                        $scope.model.allowedContentTypes.push(model.selectedItem.id);
                    }
                    overlayService.close();
                },
                close: function() {
                    overlayService.close();
                }
            };

            localizationService.localize("contentTypeEditor_chooseChildNode").then(value => {
                dialog.title = value;
                overlayService.open(dialog);
            });
        }

        function removeChild(selectedChild, index) {
           // remove from vm
           vm.selectedChildren.splice(index, 1);

           // remove from content type model
           var selectedChildIndex = $scope.model.allowedContentTypes.indexOf(selectedChild.id);
           $scope.model.allowedContentTypes.splice(selectedChildIndex, 1);
        }

        function sortChildren() {
            // we need to wait until the next digest cycle for vm.selectedChildren to be updated
            $timeout(function () {
                $scope.model.allowedContentTypes = _.pluck(vm.selectedChildren, "id");
            });
        }

        // note: "safe toggling" here ie handling cases where the value is undefined, etc

        function toggleAllowAsRoot() {
            $scope.model.allowAsRoot = $scope.model.allowAsRoot ? false : true;
        }

        function toggleAllowCultureVariants() {
            $scope.model.allowCultureVariant = $scope.model.allowCultureVariant ? false : true;
        }

        function toggleAllowSegmentVariants() {
            $scope.model.allowSegmentVariant = $scope.model.allowSegmentVariant ? false : true;
        }

        function toggleIsElement() {
            $scope.model.isElement = $scope.model.isElement ? false : true;
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
})();
