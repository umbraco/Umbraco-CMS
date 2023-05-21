(function () {
    "use strict";

    angular
    .module("umbraco")
    .controller("Umbraco.PropertyEditors.MediaPicker3PropertyEditor.CreateButtonController",
    function Controller($scope) {

        var vm = this;
        vm.plusPosY = 0;

        vm.onMouseMove = function($event) {
            vm.plusPosY = $event.offsetY;
        }

    });

})();
