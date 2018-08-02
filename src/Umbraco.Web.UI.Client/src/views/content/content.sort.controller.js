(function () {
    "use strict";

    function ContentSortController($scope, $timeout, contentResource) {

        var vm = this;
        var parentId = $scope.currentNode.parentId;

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

            contentResource.getChildren(parentId)
                .then(function(data){
                    vm.children = data.items;
                    vm.loading = false;
                    console.log(vm.children);
                });
        }

        function save() {

            console.log(vm.nodes);
            vm.saveButtonState = "busy";

            // fake loading
            $timeout(function () {
                vm.saveButtonState = "success";
            }, 1000);

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