(function () {
    'use strict';

    function InlineBlockEditor($scope) {

        const bc = this;

        bc.isOpen = false;
        bc.caretIconType = "icon-navigation-right";

        bc.openBlock = function() {
            bc.isOpen = !bc.isOpen;
            bc.caretIconType = bc.isOpen ? "icon-navigation-down" : "icon-navigation-right";
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.InlineBlockEditor", InlineBlockEditor);

})();
