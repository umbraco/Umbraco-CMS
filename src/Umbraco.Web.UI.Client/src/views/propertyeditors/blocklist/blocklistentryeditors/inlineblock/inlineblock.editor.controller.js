(function () {
    'use strict';

    function InlineBlockEditor() {

        const bc = this;

        bc.openBlock = function(block) {
            block.isOpen = !block.isOpen;
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.InlineBlockEditor", InlineBlockEditor);

})();
