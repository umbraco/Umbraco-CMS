(function () {
    "use strict";

    function ContentSortController($scope, $timeout, contentResource, navigationService) {

        var vm = this;
        var parentId = $scope.currentNode.parentId;
        var id = $scope.currentNode.id;

        vm.loading = false;
        vm.children = [];
        vm.saveButtonState = "init";
        vm.sortableOptions = {
            distance: 10,
            tolerance: "pointer",
            opacity: 0.7,
            scroll: true,
            cursor: "move",
            helper: fixSortableHelper
        };

        vm.save = save;

        function onInit() {
            vm.loading = true;
            contentResource.getChildren(id)
                .then(function(data){
                    vm.children = data.items;
                    vm.loading = false;
                });
        }

        function save() {
            vm.saveButtonState = "busy";
            
            var args = {
                parentId: parentId,
                sortedIds: _.map(vm.children, function(child){ return child.id; })
            };

            contentResource.sort(args)
                .then(function(){
                    navigationService.syncTree({ tree: "content", path: $scope.currentNode.path, forceReload: true, activate: false });
                    vm.saveButtonState = "success";
                }).catch(function(error) {
                    vm.error = error;
                    vm.saveButtonState = "error";
                });
        }

        function fixSortableHelper(e, ui) {
            // keep the correct width of each table cell when sorting
            ui.children().each(function () {
                $(this).width($(this).width());
            });
            return ui;
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.SortController", ContentSortController);
})();