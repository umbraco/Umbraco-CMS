(function () {
    "use strict";

    angular
    .module("umbraco")
    .controller("Umbraco.PropertyEditors.BlockListPropertyEditor.CreateButtonController", 
    function Controller($scope) {
        
        var vm = this;
        vm.plusPosX = 0;

        vm.onMouseMove = function ($event) {
            vm.plusPosX = $event.offsetX;
        };

    });

})();
