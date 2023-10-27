/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
(function () {
    'use strict';

    function PermissionsController($scope, $timeout, contentTypeResource, iconHelper, contentTypeHelper, editorService) {

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
        vm.toggleHistoryCleanupPreventCleanup = toggleHistoryCleanupPreventCleanup;

        /* ---------- INIT ---------- */

        init();

        function init() {

            contentTypeResource.getAll().then(contentTypes => {
                vm.contentTypes = contentTypes.filter(x => !x.isElement);

                // convert legacy icons
                iconHelper.formatContentTypeIcons(vm.contentTypes);

                vm.selectedChildren = contentTypeHelper.makeObjectArrayFromId($scope.model.allowedContentTypes, contentTypes);

                if ($scope.model.id === 0) {
                    contentTypeHelper.insertChildNodePlaceholder(vm.contentTypes, $scope.model.name, $scope.model.icon, $scope.model.id);
                }
            });

            // Can only switch to an element type if there are no content nodes already created from the type.
            if ($scope.model.id > 0 && !$scope.model.isElement) {
                contentTypeResource.hasContentNodes($scope.model.id).then(result => {
                    vm.canToggleIsElement = !result;
                });
            } else {
                vm.canToggleIsElement = true;
            }

            if(!$scope.model.historyCleanup){
                $scope.model.historyCleanup = {};
            }

        }

        function addChild($event) {

            var editor = {
                multiPicker: true,
                filterCssClass: 'not-allowed not-published',
                filter: item =>
                    !vm.contentTypes.some(x => x.udi == item.udi) || vm.selectedChildren.some(x => x.udi === item.udi),
                submit: model => {
                    model.selection.forEach(item =>
                        contentTypeResource.getById(item.id).then(contentType => {
                            vm.selectedChildren.push(contentType);
                            $scope.model.allowedContentTypes.push(item.id);
                        }));

                    editorService.close();
                },
                close: () => editorService.close()
            };

            editorService.contentTypePicker(editor);
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
            $timeout(() => $scope.model.allowedContentTypes = vm.selectedChildren.map(x => x.id));
        }

        // note: 'safe toggling' here ie handling cases where the value is undefined, etc

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

        function toggleHistoryCleanupPreventCleanup() {
            $scope.model.historyCleanup.preventCleanup = $scope.model.historyCleanup.preventCleanup ? false : true;
        }

    }

    angular.module('umbraco').controller('Umbraco.Editors.DocumentType.PermissionsController', PermissionsController);
})();
