(function () {
    "use strict";
    angular
        .module("umbraco")
        .component("blockListPropertyEditorBlock", {
            templateUrl: "views/propertyeditors/blocklist/blocklist.block.component.html",
            controller: BlockListBlockController,
            controllerAs: "vm",
            bindings: {
                block: "=",
                blockEditorApi: "=",
                focusThisBlock: "<?"
            }
        });

    function BlockListBlockController($scope, blockEditorService) {
        
        var unsubscribe = [];
        var vm = this;

        console.log("BlockListBlockController", vm);

        vm.$onInit = function() {

            // Start watching each property value.
            var variant = vm.block.content.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    unsubscribe.push($scope.$watch("vm.block.content.variants[0].tabs["+t+"].properties["+p+"].value", createPropWatcher(prop)));
                }
            }
        }

        function createPropWatcher(prop)  {

            return function() {

                // sync data:
                console.log(prop.alias, prop.value);
                vm.block.contentModel[prop.alias] = prop.value;

                // update label:
                updateLabel();
            }

        }

        function updateLabel() {
            vm.block.label = blockEditorService.getBlockLabel(vm.block);
        }
       
        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });


    }

})();
