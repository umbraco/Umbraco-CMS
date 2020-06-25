(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbElementEditorContent', {
            templateUrl: 'views/common/infiniteeditors/elementeditor/elementEditor.content.component.html',
            controller: ElementEditorContentComponentController,
            controllerAs: 'vm',
            bindings: {
                model: '='
            }
        });

    function ElementEditorContentComponentController() {

        // We need a controller for the component to work.

    }

})();
