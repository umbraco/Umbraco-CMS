(function () {
    "use strict";
    angular
        .module("umbraco")
        .component("blockListPropertyEditorBlock", {
            templateUrl: "views/propertyeditors/blocklist/blocklist.block.component.html",
            transclude: true,
            controller: BlockListBlockController,
            controllerAs: "vm",
            bindings: {
                block: "=",
                blockEditorApi: "<",
                focusThisBlock: "<?",
                class: "@",
                showCopy: "@?"
            }
        });

    function BlockListBlockController($scope, blockEditorService) {
        
        var unsubscribe = [];
        var vm = this;

        vm.$onInit = function() {

            // Start watching each property value.
            var variant = vm.block.content.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    // Sadly we need to deep watch, cause its our only way to make sure that complex values gets synced. Alternative solution would be to sync on a broadcasted event, fired on Save and Copy eventually more.
                    // But to minimize the watch we only watch the value of properties. But because we are deep watching it means that we are watching everything of nested block editors, so this would only have a performance improvement for first levels of block editors.
                    // New thoughts, since the value of a property editors is just a pointer (if not primative) then we could properly live without deep watching? cause they reference the same?.. Lets investigate..
                    unsubscribe.push($scope.$watch("vm.block.content.variants[0].tabs["+t+"].properties["+p+"].value", createPropWatcher(prop)));
                }
            }
        }

        function createPropWatcher(prop)  {

            return function() {

                // sync data:
                vm.block.contentModel[prop.alias] = prop.value;

                //vm.blockEditorApi.sync();

                // update label:
                updateLabel();
            }

        }

        function updateLabel() {
            vm.block.label = blockEditorService.getBlockLabel(vm.block);
        }

        /**
         * Listening for properties
         */
        /*
        function onBlockEditorValueUpdated($event) {
            // Lets sync the value of the property that the event comes from, if we know that..

            //$event.stopPropagation();
            //$event.preventDefault();
        };

        unsubscribe.push($scope.$on("blockEditorValueUpdated", onBlockEditorValueUpdated));
        */
       
        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });


    }

})();
