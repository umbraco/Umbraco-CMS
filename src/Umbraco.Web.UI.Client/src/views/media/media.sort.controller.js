(function () {
    "use strict";

    function MediaSortController($scope, $filter, mediaResource, navigationService, eventsService) {

        var vm = this;
        var id = $scope.currentNode.id;

        vm.loading = false;
        vm.children = [];
        vm.saveButtonState = "init";
        vm.sortOrder = {};
        vm.sortableOptions = {
            distance: 10,
            tolerance: "pointer",
            opacity: 0.7,
            scroll: true,
            cursor: "move",
            helper: fixSortableHelper,
            update: function() {
                // clear the sort order when drag and drop is used
                vm.sortOrder.column = "";
                vm.sortOrder.reverse = false;
            }
        };

        vm.save = save;
        vm.sort = sort;
        vm.close = close;

        function onInit() {
            vm.loading = true;
            mediaResource.getChildren(id)
                .then(function(data){
                    vm.children = data.items;
                    vm.loading = false;
                });
        }

        function save() {
            vm.saveButtonState = "busy";
            
            var args = {
                parentId: id,
                sortedIds: _.map(vm.children, function(child){ return child.id; })
            };

            mediaResource.sort(args)
                .then(function(){
                    navigationService.syncTree({ tree: "media", path: $scope.currentNode.path, forceReload: true })
                        .then(() => navigationService.reloadNode($scope.currentNode));

                    eventsService.emit("sortCompleted", { id: id });
                    vm.saveButtonState = "success";
                }, function(error) {
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

        function sort(column) {
            // reverse if it is already ordered by that column
            if(vm.sortOrder.column === column) {
                vm.sortOrder.reverse = !vm.sortOrder.reverse
            } else {
                vm.sortOrder.column = column;
                vm.sortOrder.reverse = false;
            }
            vm.children = $filter('orderBy')(vm.children, vm.sortOrder.column, vm.sortOrder.reverse);
        }

        function close() {
            navigationService.hideDialog();
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Media.SortController", MediaSortController);
})();
