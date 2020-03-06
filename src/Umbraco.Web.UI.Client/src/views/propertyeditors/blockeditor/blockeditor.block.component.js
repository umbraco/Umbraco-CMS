(function () {
    "use strict";
    angular
        .module("umbraco")
        .component("blockEditorBlock", {
            templateUrl: "views/propertyeditors/blockeditor/blockeditor.block.component.html",
            transclude: true,
            controller: BlockEditorBlockBlockController,
            controllerAs: "vm",
            bindings: {
                block: "=",
                blockEditorApi: "<",
                focusThisBlock: "<?",
                class: "@",
                showCopy: "@?"
            }
        });

    function BlockEditorBlockBlockController($scope, blockEditorService) {
        /*
        var unsubscribe = [];
        var vm = this;

        vm.$onInit = function() {

            // Start watching each property value.
            var variant = vm.block.content.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    // Watch value of property since this is the only value we want to keep synced.
                    // Do notice that it is not performing a deep watch, meaning that we are only watching primatives and changes directly to the object of property-value.
                    // But we like to sync non-primative values as well! Yes, and this does happen, just not through this code, but through the nature of JavaScript. 
                    // Non-primative values act as references to the same data and are therefor synced.
                    unsubscribe.push($scope.$watch("vm.block.content.variants[0].tabs["+t+"].properties["+p+"].value", createPropWatcher(prop)));
                }
            }
        }

        function createPropWatcher(prop)  {

            return function() {

                // sync data:
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
        */

    }

})();
