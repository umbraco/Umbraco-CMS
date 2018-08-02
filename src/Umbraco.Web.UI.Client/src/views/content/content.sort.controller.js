(function () {
    "use strict";

    function ContentSortController($scope, $timeout) {

        var vm = this;

        vm.loading = false;
        vm.nodes = [];
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


        function activate() {

            vm.loading = true;

            // fake loading
            $timeout(function () {

                vm.loading = false;

                vm.nodes = [
                    {
                        "name": "Node 1",
                        "creationDate": "date",
                        "sortOrder": 0
                    },
                    {
                        "name": "Node 2",
                        "creationDate": "date",
                        "sortOrder": 1
                    },
                    {
                        "name": "Node 3",
                        "creationDate": "date",
                        "sortOrder": 2
                    }
                ];

            }, 1000);
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

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.SortController", ContentSortController);
})();