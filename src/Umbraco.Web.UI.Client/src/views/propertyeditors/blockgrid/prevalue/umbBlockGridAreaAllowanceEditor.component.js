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
        .component("umbBlockGridAreaAllowanceEditor", {
            templateUrl: "views/propertyeditors/blockgrid/prevalue/umb-block-grid-area-allowance-editor.html",
            controller: BlockGridAreaAllowanceController,
            controllerAs: "vm",
            bindings: {
                model: "=",
                allBlockTypes: "<",
                allBlockGroups: "<",
                loadedElementTypes: "<",
                disabled: "<"
            },
            require: {
                propertyForm: "^form"
            }
        });

    function BlockGridAreaAllowanceController($scope, $element, assetsService, localizationService, editorService) {

        var unsubscribe = [];

        var vm = this;
        vm.loading = true;

        vm.$onInit = function() {
            vm.loading = false;

            vm.model.forEach((x) => {
                x['$key'] = String.CreateGuid();

                // transfer the chosen key onto the $chosenValue property.
                if(x.groupKey) {
                    x['$chosenValue'] = "groupKey:"+x.groupKey;
                } else if (x.elementTypeKey) {
                    x['$chosenValue'] = "elementTypeKey:"+x.elementTypeKey;
                }
            });
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
                groupKey: null,
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

            // Set groupKey or elementTypeKey based on $chosenValue.
            vm.model.forEach((x) => {
                const value = x['$chosenValue'];
                if (value.indexOf('groupKey:') === 0) {
                    x.groupKey = value.slice(9);
                    x.elementTypeKey = null;
                } else if (value.indexOf('elementTypeKey:') === 0) {
                    x.groupKey = null;
                    x.elementTypeKey = value.slice(15);
                }
            });

            for (const subscription of unsubscribe) {
                subscription();
            }
        });
        
    }

})();
