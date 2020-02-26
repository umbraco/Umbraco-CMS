(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbElementContentEditor', {
            templateUrl: 'views/common/infiniteeditors/elementeditor/elementContentEditor.component.html',
            controller: ElementEditorComponentController,
            controllerAs: 'vm',
            bindings: {
                content: '='
            }
        });

    function ElementEditorComponentController() {

        const vm = this;

        // TODO: we might not need this..

    }

})();
