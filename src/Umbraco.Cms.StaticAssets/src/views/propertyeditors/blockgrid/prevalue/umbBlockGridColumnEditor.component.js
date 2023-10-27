(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridColumnSpanEditor
     * @function
     *
     * @description
     * The component for the block grid column span prevalue editor.
     */
    angular
        .module("umbraco")
        .component("umbBlockGridColumnEditor", {
            templateUrl: "views/propertyeditors/blockgrid/prevalue/umb-block-grid-column-editor.html",
            controller: BlockGridColumnController,
            controllerAs: "vm",
            bindings: {
                model: "=",
                block: "<",
                gridColumns: "<"
            },
            require: {
                propertyForm: "^form"
            }
        });

    function BlockGridColumnController() {

        var vm = this;

        vm.$onInit = function() {
            
            vm.emptyGridColumnArray = Array.from(Array(vm.gridColumns + 1).keys()).slice(1);

            vm.block.columnSpanOptions = vm.block.columnSpanOptions.filter(
                (value, index, self) => {
                    return value.columnSpan <= vm.gridColumns &&
                        self.findIndex(v => v.columnSpan === value.columnSpan) === index;
                }
            );
        };

        vm.addSpanOption = function(colN) {
            vm.block.columnSpanOptions.push({'columnSpan': colN});
            setDirty();
        }
        vm.removeSpanOption = function(colN) {
            vm.block.columnSpanOptions = vm.block.columnSpanOptions.filter(value => value.columnSpan !== colN);
            setDirty();
        }

        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }
        
    }

})();
