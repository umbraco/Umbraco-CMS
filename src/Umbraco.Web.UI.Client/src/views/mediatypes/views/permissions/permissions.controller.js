(function() {
    'use strict';

    function PermissionsController($scope, $timeout, mediaTypeResource, iconHelper, contentTypeHelper, editorService) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.mediaTypes = [];
        vm.selectedChildren = [];

        vm.addChild = addChild;
        vm.removeChild = removeChild;
        vm.sortChildren = sortChildren;
        vm.toggle = toggle;

        /* ---------- INIT ---------- */

        init();

        function init() {

            mediaTypeResource.getAll().then(mediaTypes => {

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

            var editor = {
                multiPicker: true,
                filterCssClass: 'not-allowed not-published',
                filter: item => 
                    !vm.mediaTypes.some(x => x.udi == item.udi) || vm.selectedChildren.some(x => x.udi === item.udi),                
                submit: model => {
                    model.selection.forEach(item => 
                        mediaTypeResource.getById(item.id).then(contentType => {
                            vm.selectedChildren.push(contentType);
                            $scope.model.allowedContentTypes.push(item.id);
                        }));

                    editorService.close();
                },
                close: () => editorService.close()                
            };

            editorService.mediaTypePicker(editor);
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

    angular.module('umbraco').controller('Umbraco.Editors.MediaType.PermissionsController', PermissionsController);
})();
