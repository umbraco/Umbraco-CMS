(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridAreaEditor
     * @function
     *
     * @description
     * The component for the block grid area prevalue editor.
     */
    angular
        .module("umbraco")
        .component("umbBlockGridAreaBlockAllowanceEditor", {
            templateUrl: "views/propertyeditors/blockgrid/prevalue/umb-block-grid-area-block-allowance-editor.html",
            controller: BlockGridAreaBlockAllowanceController,
            controllerAs: "vm",
            bindings: {
                model: "=",
                allBlockTypes: "<",
                loadedElementTypes: "<",
                disabled: "<"
            },
            require: {
                propertyForm: "^form"
            }
        });

    function BlockGridAreaBlockAllowanceController($scope, $element, assetsService, localizationService, editorService) {

        var unsubscribe = [];

        var vm = this;
        vm.loading = true;

        vm.$onInit = function() {
            vm.loading = false;

            vm.model.forEach((x) => {
                x['$key'] = String.CreateGuid()
            })
        };

        vm.getElementTypeByKey = function(key) {
            if (vm.loadedElementTypes) {
                return vm.loadedElementTypes.find(function (type) {
                    return type.key === key;
                }) || null;
            }
        };

        vm.deleteAllowance = function(allowance) {
            const index = vm.model.indexOf(allowance);
            if(index !== -1) {
                vm.model.splice(index, 1);
            }
        }

        vm.onNewAllowanceClick = function() {
            const allowance = {
                $key: String.CreateGuid(),
                elementTypeKey: null,
                min: 0,
                max: 0
            };
            vm.model.push(allowance);
            setDirty();
        }
        
        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }
        
        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });
        
    }

})();
