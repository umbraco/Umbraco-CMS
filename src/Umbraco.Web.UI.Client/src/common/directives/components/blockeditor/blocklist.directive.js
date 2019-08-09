(function () {
    "use strict";

    /**
     * A component used to render a list of blocks
     */
    var umbBlockList = {
        templateUrl: 'views/components/blockeditor/umb-block-list.html',
        bindings: {
            blocks: "<",
            config: "<",
            scaffolds: "<",
            onEdit: "&",
            onRemove: "&",
            onSettings: "&",           
            onSort: "&?"
        },
        controllerAs: 'vm',
        controller: umbBlockListController
    };

    function umbBlockListController() { 
        var vm = this;

        vm.sortableOptions = {
            axis: "y",
            cursor: "move",
            handle: ".handle",
            tolerance: 'pointer',
            update: function (e, ui) {
                if (vm.onSort) {
                    vm.onSort({ e: e, ui: ui });
                }
            }
        };

        vm.editContent = function (block) {
            var scaffold = _.findWhere(vm.scaffolds, {
                udi: block.udi
            });
            var element = angular.copy(scaffold);
            _.each(element.variants[0].tabs, function (tab) {
                _.each(tab.properties, function (property) {
                    if (block.content[property.alias]) {
                        property.value = block.content[property.alias];
                    }
                });
            });

            vm.onEdit({ element: element, block: block });
        }

        vm.remove = function (block) {
            // this should be replaced by a custom dialog (pending some PRs)
            if (confirm("TODO: Are you sure?")) {
                vm.onRemove({ block: block });
            }
        }

    }

    angular.module('umbraco').component('umbBlockList', umbBlockList);


})();
