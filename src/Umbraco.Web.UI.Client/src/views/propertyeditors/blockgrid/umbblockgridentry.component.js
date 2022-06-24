(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridEntry
     * @description
     * renders each row for the block grid editor
     */
    
    angular
        .module("umbraco")
        .component("umbBlockGridEntry", {
            templateUrl: 'views/propertyeditors/blockgrid/umb-block-grid-entry.html',
            controller: BlockGridEntryController,
            controllerAs: "vm",
            bindings: {
                blockEditorApi: "<",
                layoutEntry: "<",
                index: "<",
                parentBlock: "<",
                areaKey: "<"
            }
        }
    );

    function BlockGridEntryController($scope, $element) {

        const vm = this;

        vm.scaleHandlerMouseDown = function($event) {
            $event.originalEvent.preventDefault();
            
            window.addEventListener('mousemove', vm.onMouseMove);
            window.addEventListener('mouseup', vm.onMouseUp);
            window.addEventListener('mouseleave', vm.onMouseUp);

            return false;
        }
        vm.onMouseMove = function(e) {
            console.log(e);
        }
        vm.onMouseUp = function(e) {
            console.log("up, leave", e);
            
            window.removeEventListener('mousemove', vm.onMouseMove);
            window.removeEventListener('mouseup', vm.onMouseUp);
            window.removeEventListener('mouseleave', vm.onMouseUp);
        }

    }   

})();
