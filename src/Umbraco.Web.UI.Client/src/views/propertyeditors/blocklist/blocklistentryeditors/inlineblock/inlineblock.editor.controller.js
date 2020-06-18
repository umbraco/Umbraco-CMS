(function () {
    'use strict';

    function InlineBlockEditor() {

        const bc = this;

        bc.openBlock = function(block) {
            block.active = !block.active;
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.InlineBlockEditor", InlineBlockEditor);

})();
